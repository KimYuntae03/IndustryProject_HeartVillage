using UnityEngine;
using UnityEngine.InputSystem;

public class SignHighlight : MonoBehaviour
{
    [Header("표지판 강조")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject highlightObject;
    [SerializeField] private float highlightDistance = 1.5f;

    [Header("미니게임 이동")]
    [SerializeField] private MinigamePopup minigamePopup;
    [SerializeField] private string targetSceneName;

    private bool isPlayerNear;

    private void Start()
    {
        highlightObject.SetActive(false);
    }

    private void Update()
    {
        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        isPlayerNear = distance <= highlightDistance;
        highlightObject.SetActive(isPlayerNear);

        if (isPlayerNear &&
            !minigamePopup.IsOpen &&
            IsEnterPressed())
        {
            minigamePopup.OpenPopup(targetSceneName);
        }
    }

    private bool IsEnterPressed()
    {
        if (Keyboard.current == null)
        {
            return false;
        }

        return Keyboard.current.enterKey.wasPressedThisFrame ||
               Keyboard.current.numpadEnterKey.wasPressedThisFrame;
    }
}