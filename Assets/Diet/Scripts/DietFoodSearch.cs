using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class DietFoodSearch : MonoBehaviour
{
    [Serializable] public sealed class Food
    {
        public string id, name, category, serving;
        public int sodiumMg;
    }
    [Serializable] private sealed class Catalog { public List<Food> foods; }
    [Serializable] private sealed class Entry { public string foodId; public int count; }
    [Serializable] private sealed class DayRecord
    {
        public string date;
        public List<Entry> entries = new List<Entry>();
    }

    [SerializeField] private TextAsset catalog;
    [SerializeField] private TMP_InputField searchInput;
    [SerializeField] private UnityEngine.UI.Button addButton;
    [SerializeField] private RectTransform bar;
    [SerializeField] private TMP_Text sodiumLabel;
    [SerializeField, Min(1)] private int dailyTargetMg = 2000;

    private const string SaveKey = "HeartVillage.Diet.Day.V1";
    private readonly Dictionary<string, Food> byId = new Dictionary<string, Food>();
    private List<Food> foods;
    private DayRecord record;
    private Food selected;
    private RectTransform suggestions, cartContent;
    private SodiumGaugeGraphic gaugeFill;
    private ScrollRect suggestionsScroll, cartScroll;
    private TMP_Text detail;
    private TMP_Text addedMessage;
    private TMP_FontAsset font;
    private string Today => DateTime.Now.ToString("yyyy-MM-dd");
    public long TotalSodiumMg { get; private set; }
    public bool IsFinalized => record != null && VillagePlayerData.IsDietFinalized(record.date);
    public bool CanFinalize => record != null && record.date == Today && record.entries.Count > 0 && !IsFinalized;

    private void Awake()
    {
        Initialize();
    }

    public void PrepareForScoring()
    {
        Initialize();
        CheckDate();
    }

    private void Initialize()
    {
        if (foods != null) return;
        foods = JsonUtility.FromJson<Catalog>(catalog.text).foods;
        foods.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
        foreach (var food in foods) byId.Add(food.id, food);
        font = searchInput.textComponent.font;
        searchInput.textComponent.color = Color.black;
        if (searchInput.placeholder != null)
            searchInput.placeholder.color = Color.black;
        suggestionsScroll = CreateScroll("SearchSuggestions", new Vector2(-425, 70), new Vector2(650, 310), out suggestions);
        cartScroll = CreateScroll("BasketItems", new Vector2(425, -85), new Vector2(700, 510), out cartContent);
        detail = Label("SelectedFood", transform, new Vector2(-425, 0), new Vector2(650, 200), 32.4f);
        detail.alignment = TextAlignmentOptions.Center;
        addedMessage = Label("AddedMessage", transform, new Vector2(-425, 0), new Vector2(650, 160), 36);
        addedMessage.alignment = TextAlignmentOptions.Center;
        addedMessage.text = "장바구니에 추가했어요.\n다음 음식을 검색해 주세요.";
        addedMessage.gameObject.SetActive(false);
        sodiumLabel.font = font;
        sodiumLabel.color = Color.black;
        sodiumLabel.fontSize = 34;
        sodiumLabel.enableAutoSizing = false;
        sodiumLabel.rectTransform.sizeDelta = new Vector2(730, 75);
        sodiumLabel.rectTransform.anchoredPosition = new Vector2(425, 330);
        sodiumLabel.raycastTarget = false;
        var frame = bar.GetComponent<UnityEngine.UI.Image>();
        frame.raycastTarget = false;
        var track = Rect("SodiumTrack", bar);
        // Overlap the frame slightly so the rounded inside edge has no empty seam.
        Stretch(track, new Vector2(8, 6), new Vector2(-8, -6));
        gaugeFill = track.gameObject.AddComponent<SodiumGaugeGraphic>();
        gaugeFill.raycastTarget = false;
        var foreground = Rect("SodiumFrame", bar);
        Stretch(foreground, Vector2.zero, Vector2.zero);
        var frameImage = foreground.gameObject.AddComponent<UnityEngine.UI.Image>();
        frameImage.sprite = frame.sprite;
        frameImage.color = frame.color;
        frameImage.raycastTarget = false;
        searchInput.onValueChanged.AddListener(Search);
        searchInput.onSubmit.AddListener(Submit);
        addButton.onClick.AddListener(AddSelected);
        LoadRecord();
        Search(searchInput.text);
        RefreshCart();
        ApplyFinalizedState();
    }

    private void OnEnable()
    {
        if (record != null)
        {
            CheckDate();
            ApplyFinalizedState();
        }
    }

    private void Update() { CheckDate(); }

    private void CheckDate()
    {
        if (record.date == Today) return;
        record = new DayRecord { date = Today };
        Save();
        searchInput.SetTextWithoutNotify("");
        Search("");
        RefreshCart();
        ApplyFinalizedState();
    }

    public bool TryFinalize(string confirmationDate, out int awardedPoints)
    {
        awardedPoints = 0;
        CheckDate();
        if (confirmationDate != record.date || !CanFinalize) return false;
        RefreshCart();
        Save();
        if (!VillagePlayerData.TryGrantDietReward(record.date, TotalSodiumMg, dailyTargetMg, out awardedPoints))
            return false;
        RefreshCart();
        ApplyFinalizedState();
        return true;
    }

    private void ApplyFinalizedState()
    {
        searchInput.interactable = !IsFinalized;
        if (!IsFinalized) return;
        selected = null;
        addButton.interactable = false;
        suggestionsScroll.gameObject.SetActive(false);
        addedMessage.gameObject.SetActive(false);
        detail.gameObject.SetActive(true);
        detail.text = "오늘 식단이 확정되었어요.\n내일 다시 기록할 수 있어요.";
    }

    private void LoadRecord()
    {
        try { record = JsonUtility.FromJson<DayRecord>(PlayerPrefs.GetString(SaveKey, "")); }
        catch (ArgumentException) { record = null; }
        if (record == null || record.date != Today)
        {
            record = new DayRecord { date = Today };
            Save();
        }
        if (record.entries == null) record.entries = new List<Entry>();
        record.entries.RemoveAll(e => e == null || e.count <= 0 || string.IsNullOrEmpty(e.foodId) || !byId.ContainsKey(e.foodId));
    }

    private void Save()
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(record));
        PlayerPrefs.Save();
    }

    private void Search(string input)
    {
        if (IsFinalized)
        {
            ApplyFinalizedState();
            return;
        }
        addedMessage.gameObject.SetActive(false);
        selected = null;
        addButton.interactable = false;
        Clear(suggestions);
        string query = input.Trim();
        int exactMatches = 0;
        Food exact = null;
        int matches = 0;
        if (query.Length > 0)
        {
            foreach (var food in foods)
            {
                if (!food.name.StartsWith(query, StringComparison.OrdinalIgnoreCase)) continue;
                matches++;
                if (string.Equals(food.name, query, StringComparison.OrdinalIgnoreCase))
                {
                    exactMatches++;
                    exact = food;
                }
                var row = Row(suggestions, "Food_" + food.id, 118);
                var text = Label("Food", row, Vector2.zero, Vector2.zero, 30);
                Stretch(text.rectTransform, new Vector2(16, 4), new Vector2(-16, -4));
                text.text = food.name + "\n<size=24>" + food.serving + " · " + food.sodiumMg.ToString("N0") + "mg · " + food.category + "</size>";
                var button = row.gameObject.AddComponent<UnityEngine.UI.Button>();
                button.targetGraphic = row.GetComponent<UnityEngine.UI.Image>();
                button.onClick.AddListener(() => Select(food));
            }
        }
        suggestionsScroll.gameObject.SetActive(matches > 0);
        detail.gameObject.SetActive(matches == 0);
        suggestionsScroll.verticalNormalizedPosition = 1f;
        detail.text = query.Length == 0 ? "음식 이름을 입력해 주세요." : "검색 결과가 없습니다.";
        if (matches > 0) detail.text = matches + "개 음식 · 선택하면 나트륨을 확인할 수 있어요.";
        if (exactMatches == 1)
        {
            selected = exact;
            ShowSelection();
        }
        else if (exactMatches > 1) detail.text = "같은 이름의 음식이 있어요. 제공량과 분류를 확인해 선택해 주세요.";
    }

    private void Submit(string value)
    {
        if (selected != null) Select(selected);
    }

    private void Select(Food food)
    {
        if (IsFinalized) return;
        selected = food;
        searchInput.SetTextWithoutNotify(food.name);
        suggestionsScroll.gameObject.SetActive(false);
        ShowSelection();
    }

    private void ShowSelection()
    {
        suggestionsScroll.gameObject.SetActive(false);
        detail.gameObject.SetActive(true);
        detail.text = selected.name + "\n" + selected.serving + "\n나트륨 " + selected.sodiumMg.ToString("N0") + "mg";
        addButton.interactable = true;
    }

    private void AddSelected()
    {
        CheckDate();
        if (selected == null || IsFinalized) return;
        var entry = record.entries.Find(e => e.foodId == selected.id);
        if (entry == null)
        {
            entry = new Entry { foodId = selected.id };
            record.entries.Add(entry);
        }
        if (entry.count == int.MaxValue) return;
        entry.count++;
        Save();
        RefreshCart();
        searchInput.SetTextWithoutNotify("");
        selected = null;
        addButton.interactable = false;
        suggestionsScroll.gameObject.SetActive(false);
        detail.text = "";
        addedMessage.gameObject.SetActive(true);
    }

    private void RemoveOne(Entry entry)
    {
        CheckDate();
        if (IsFinalized || !record.entries.Contains(entry)) return;
        entry.count--;
        if (entry.count == 0) record.entries.Remove(entry);
        Save();
        RefreshCart();
    }

    private void RefreshCart()
    {
        Clear(cartContent);
        TotalSodiumMg = 0;
        foreach (var entry in record.entries)
        {
            var food = byId[entry.foodId];
            long amount = (long)food.sodiumMg * entry.count;
            TotalSodiumMg += amount;
            var row = Row(cartContent, "Cart_" + food.id, 137);
            var text = Label("Food", row, Vector2.zero, Vector2.zero, 30);
            Stretch(text.rectTransform, new Vector2(14, 3), new Vector2(-100, -3));
            text.text = food.name + "\n<size=24>" + food.serving + " × " + entry.count + "\n나트륨 " + amount.ToString("N0") + "mg</size>";
            var remove = Rect("RemoveOne", row);
            remove.anchorMin = new Vector2(1, 0);
            remove.anchorMax = Vector2.one;
            remove.offsetMin = new Vector2(-90, 12);
            remove.offsetMax = new Vector2(-8, -12);
            var image = remove.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.9f, 0.8f, 0.67f);
            var button = remove.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;
            button.interactable = !IsFinalized;
            button.onClick.AddListener(() => RemoveOne(entry));
            var label = Label("Label", remove, Vector2.zero, Vector2.zero, 24);
            Stretch(label.rectTransform, Vector2.zero, Vector2.zero);
            label.alignment = TextAlignmentOptions.Center;
            label.text = "−1";
        }
        sodiumLabel.text = "나트륨 " + TotalSodiumMg.ToString("N0") + "mg";
        gaugeFill.SetSodium(TotalSodiumMg, dailyTargetMg);
        if (record.entries.Count == 0)
        {
            var row = Row(cartContent, "Empty", 80);
            var text = Label("EmptyText", row, Vector2.zero, Vector2.zero, 25);
            Stretch(text.rectTransform, new Vector2(16, 4), new Vector2(-16, -4));
            text.text = "먹은 음식을 왼쪽에서 추가해 주세요.";
        }
    }

    private ScrollRect CreateScroll(string name, Vector2 position, Vector2 size, out RectTransform content)
    {
        var root = Rect(name, transform);
        root.anchoredPosition = position;
        root.sizeDelta = size;
        var viewport = Rect("Viewport", root);
        Stretch(viewport, Vector2.zero, Vector2.zero);
        viewport.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(1, 0.96f, 0.87f, 0.97f);
        viewport.gameObject.AddComponent<Mask>().showMaskGraphic = true;
        content = Rect("Content", viewport);
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = Vector2.one;
        content.pivot = new Vector2(0.5f, 1);
        content.sizeDelta = Vector2.zero;
        var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = 6;
        layout.padding = new RectOffset(5, 5, 5, 5);
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var scroll = root.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 35;
        return scroll;
    }

    private RectTransform Rect(string name, Transform parent)
    {
        var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        rect.gameObject.layer = gameObject.layer;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        return rect;
    }

    private TMP_Text Label(string name, Transform parent, Vector2 position, Vector2 size, float fontSize)
    {
        var rect = Rect(name, parent);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = fontSize;
        text.color = Color.black;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.raycastTarget = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        return text;
    }

    private RectTransform Row(RectTransform parent, string name, float height)
    {
        var rect = Rect(name, parent);
        rect.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
        rect.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(0.96f, 0.9f, 0.77f);
        return rect;
    }

    private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = min;
        rect.offsetMax = max;
    }

    private static void Clear(RectTransform content)
    {
        foreach (Transform child in content)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }

    private void OnDestroy()
    {
        searchInput.onValueChanged.RemoveListener(Search);
        searchInput.onSubmit.RemoveListener(Submit);
        addButton.onClick.RemoveListener(AddSelected);
    }
}
