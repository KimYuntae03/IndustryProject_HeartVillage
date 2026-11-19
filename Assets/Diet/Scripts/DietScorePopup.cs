using System;
using UnityEngine;

public sealed class DietScorePopup : MonoBehaviour
{
    [SerializeField] private DietFoodSearch foodSearch;
    [SerializeField] private UnityEngine.UI.Button scoreButton;
    [SerializeField] private GameObject scoreCheck;
    [SerializeField] private UnityEngine.UI.Button yesButton;
    [SerializeField] private UnityEngine.UI.Button noButton;
    [SerializeField] private DietResultUI resultUI;

    private GameObject outsideArea;
    private string confirmationDate;
    private string currentDate;
    private BasketPanel basketPanel;
    public int AwardedHeartPoints { get; private set; }
    public event Action<int> RewardGranted;

    private void Awake()
    {
        scoreCheck.SetActive(false);
        scoreButton.onClick.AddListener(Open);
        yesButton.onClick.AddListener(Confirm);
        noButton.onClick.AddListener(Close);
        outsideArea = new GameObject("ScoreCheckOutsideArea", typeof(RectTransform),
            typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
        outsideArea.layer = scoreCheck.layer;
        var rect = (RectTransform)outsideArea.transform;
        rect.SetParent(scoreCheck.transform.parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        outsideArea.GetComponent<UnityEngine.UI.Image>().color = Color.clear;
        var button = outsideArea.GetComponent<UnityEngine.UI.Button>();
        button.transition = UnityEngine.UI.Selectable.Transition.None;
        button.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
        button.onClick.AddListener(Close);
        outsideArea.SetActive(false);
        basketPanel = foodSearch.GetComponent<BasketPanel>();
        RefreshDailyAvailability();
    }

    private void Update()
    {
        RefreshDailyAvailability();
        if (scoreCheck.activeSelf)
        {
            if (confirmationDate != DateTime.Now.ToString("yyyy-MM-dd")) Close();
            else yesButton.interactable = foodSearch.CanFinalize;
        }
    }

    private void RefreshDailyAvailability()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        if (currentDate != today)
        {
            foodSearch.PrepareForScoring();
            currentDate = today;
        }
        bool available = !VillagePlayerData.IsDietFinalized(today);
        scoreButton.interactable = available;
        basketPanel.SetAvailable(available);
    }

    private void Open()
    {
        foodSearch.PrepareForScoring();
        if (foodSearch.IsFinalized) return;
        confirmationDate = DateTime.Now.ToString("yyyy-MM-dd");
        outsideArea.transform.SetAsLastSibling();
        scoreCheck.transform.SetAsLastSibling();
        outsideArea.SetActive(true);
        scoreCheck.SetActive(true);
        yesButton.interactable = foodSearch.CanFinalize;
        noButton.Select();
    }

    private void Confirm()
    {
        if (!foodSearch.TryFinalize(confirmationDate, out int points))
        {
            yesButton.interactable = false;
            return;
        }
        AwardedHeartPoints = points;
        RefreshDailyAvailability();
        Close();
        long sodiumMg = foodSearch.TotalSodiumMg;
        foodSearch.gameObject.SetActive(false);
        resultUI.ShowResult(sodiumMg, points);
        RewardGranted?.Invoke(points);
    }

    private void Close()
    {
        scoreCheck.SetActive(false);
        outsideArea.SetActive(false);
        if (UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }

    private void OnDestroy()
    {
        scoreButton.onClick.RemoveListener(Open);
        yesButton.onClick.RemoveListener(Confirm);
        noButton.onClick.RemoveListener(Close);
        if (outsideArea != null) Destroy(outsideArea);
    }
}
