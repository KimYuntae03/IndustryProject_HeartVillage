using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public sealed class SignTouchTarget : MonoBehaviour
{
    [SerializeField] private SignHighlight signHighlight;
    [SerializeField] private MinigamePopup popupController;
    [SerializeField] private GameObject informationWindow;

    public bool TryInteract()
    {
        if (informationWindow != null && popupController != null)
        {
            popupController.OpenWindow(informationWindow);
            return true;
        }
        return signHighlight != null && signHighlight.TryInteract();
    }
}
