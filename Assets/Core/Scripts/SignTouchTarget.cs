using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public sealed class SignTouchTarget : MonoBehaviour
{
    [SerializeField] private SignHighlight signHighlight;

    public bool TryInteract()
    {
        return signHighlight != null && signHighlight.TryInteract();
    }
}
