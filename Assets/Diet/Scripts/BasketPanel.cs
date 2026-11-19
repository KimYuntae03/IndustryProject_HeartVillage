using UnityEngine;
using UnityEngine.UI;

public sealed class BasketPanel : MonoBehaviour
{
    [SerializeField] private GameObject basketButton;
    private GameObject outsideArea;

    public void SetAvailable(bool available)
    {
        basketButton.GetComponent<UnityEngine.UI.Button>().interactable = available;
        if (!available && gameObject.activeSelf)
            Close();
    }

    // The opening click explicitly sets both objects, even if the panel was already active.
    public void Open()
    {
        if (VillagePlayerData.IsDietFinalized(System.DateTime.Now.ToString("yyyy-MM-dd"))) return;
        gameObject.SetActive(true);
        basketButton.SetActive(false);
    }

    private void OnEnable()
    {
        basketButton.SetActive(false);
        if (outsideArea == null)
        {
            outsideArea = new GameObject("BasketOutsideArea", typeof(RectTransform), typeof(Image), typeof(Button));
            outsideArea.layer = gameObject.layer;
            var rect = (RectTransform)outsideArea.transform;
            rect.SetParent(transform.parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            outsideArea.GetComponent<Image>().color = Color.clear;
            var button = outsideArea.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(Close);
        }
        outsideArea.transform.SetAsLastSibling();
        transform.SetAsLastSibling();
        outsideArea.SetActive(true);
    }

    private void OnDisable()
    {
        if (outsideArea != null)
            outsideArea.SetActive(false);
        if (basketButton != null)
            basketButton.SetActive(true);
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (outsideArea != null)
            Destroy(outsideArea);
    }
}
