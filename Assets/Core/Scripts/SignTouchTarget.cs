using UnityEngine;

public class SignTouchTarget : MonoBehaviour
{
    [SerializeField]
    private SignHighlight signHighlight;

    public void Interact()
    {
        if (signHighlight == null)
        {
            Debug.LogError(
                $"{name}: SignHighlight가 연결되지 않았습니다."
            );

            return;
        }

        signHighlight.TryInteract();
    }
}