using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DietBackPopup : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject panel;
    [SerializeField] private Button noButton;
    [SerializeField] private Button yesButton;

    private GameObject backdrop;

    private void Awake()
    {
        backdrop = new GameObject("Backcheck_Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
        backdrop.layer = panel.layer;
        var rect = (RectTransform)backdrop.transform;
        rect.SetParent(panel.transform.parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        backdrop.GetComponent<Image>().color = Color.clear;
        var outsideButton = backdrop.GetComponent<Button>();
        outsideButton.transition = Selectable.Transition.None;
        outsideButton.navigation = new Navigation { mode = Navigation.Mode.None };
        outsideButton.onClick.AddListener(Close);
        backButton.onClick.AddListener(Open);
        noButton.onClick.AddListener(Close);
        yesButton.onClick.AddListener(ReturnToVillage);

        panel.SetActive(false);
        backdrop.SetActive(false);
    }

    private void Open()
    {
        backdrop.transform.SetAsLastSibling();
        panel.transform.SetAsLastSibling();
        backdrop.SetActive(true);
        panel.SetActive(true);
        noButton.Select();
    }

    private void Close()
    {
        panel.SetActive(false);
        backdrop.SetActive(false);
        backButton.Select();
    }

    private void ReturnToVillage()
    {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene("CoreGameplay");
        else
            SceneManager.LoadScene("CoreGameplay");
    }
}
