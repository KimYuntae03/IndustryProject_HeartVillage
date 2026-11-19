using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class DietResultUI : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private Sprite returnButtonSprite;

    private GameObject resultRoot;
    private TMP_Text sodiumText, baseRewardText, bonusRewardText;
    private UnityEngine.UI.Button returnButton;
    private Coroutine revealCoroutine;
    private bool returning;

    public void ShowResult(long sodiumMg, int awardedPoints)
    {
        if (resultRoot == null) CreateUI();
        if (revealCoroutine != null) StopCoroutine(revealCoroutine);
        sodiumText.text = $"하루 섭취 나트륨\n{sodiumMg:N0}mg";
        baseRewardText.text = "기본보상: 10 포인트";
        bonusRewardText.text = awardedPoints > 10
            ? $"추가보상: +{awardedPoints - 10} 포인트"
            : "추가보상: 없음";
        sodiumText.gameObject.SetActive(false);
        baseRewardText.gameObject.SetActive(false);
        bonusRewardText.gameObject.SetActive(false);
        returnButton.gameObject.SetActive(false);
        resultRoot.SetActive(true);
        resultRoot.transform.SetAsLastSibling();
        revealCoroutine = StartCoroutine(RevealInOrder());
    }

    private void CreateUI()
    {
        var root = CreateRect("DietResult", transform, Vector2.zero, Vector2.zero);
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = root.offsetMax = Vector2.zero;
        resultRoot = root.gameObject;
        var background = resultRoot.AddComponent<UnityEngine.UI.Image>();
        background.color = new Color(0, 0, 0, 0.65f);
        background.raycastTarget = true;
        sodiumText = CreateText("DailySodium", root, 260);
        baseRewardText = CreateText("BaseReward", root, 90);
        bonusRewardText = CreateText("BonusReward", root, -80);
        var buttonRect = CreateRect("ReturnToVillage", root, new Vector2(0, -290), new Vector2(680, 227));
        var image = buttonRect.gameObject.AddComponent<UnityEngine.UI.Image>();
        image.sprite = returnButtonSprite;
        image.preserveAspect = true;
        returnButton = buttonRect.gameObject.AddComponent<UnityEngine.UI.Button>();
        returnButton.targetGraphic = image;
        returnButton.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
        returnButton.onClick.AddListener(ReturnToVillage);
    }

    private IEnumerator RevealInOrder()
    {
        yield return new WaitForSecondsRealtime(1f);
        sodiumText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1f);
        baseRewardText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1f);
        bonusRewardText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1f);
        returnButton.gameObject.SetActive(true);
        returnButton.Select();
        revealCoroutine = null;
    }

    private TMP_Text CreateText(string name, Transform parent, float y)
    {
        var rect = CreateRect(name, parent, new Vector2(0, y), new Vector2(1000, 140));
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = 48;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private RectTransform CreateRect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        rect.gameObject.layer = gameObject.layer;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    public void ReturnToVillage()
    {
        if (returning) return;
        returning = true;
        returnButton.interactable = false;
        Time.timeScale = 1f;
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene("CoreGameplay");
        else
            SceneManager.LoadScene("CoreGameplay");
    }
}
