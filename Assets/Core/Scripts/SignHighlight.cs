using UnityEngine;
using UnityEngine.InputSystem;

public class SignHighlight : MonoBehaviour
{
    [Header("표지판 강조")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject highlightObject;
    [SerializeField] private float highlightDistance = 1.5f;

    // 실제 표지판 위치를 판단할 TouchArea 콜라이더
    [SerializeField] private Collider2D interactionArea;

    [Header("미니게임 이동")]
    [SerializeField] private MinigamePopup minigamePopup;
    [SerializeField] private string targetSceneName;

    private bool isPlayerNear;

    private void Start()
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        // TouchArea가 연결되어 있으면 콜라이더 중앙을 기준으로 거리 계산
        Vector2 signPosition = interactionArea != null
            ? interactionArea.bounds.center
            : transform.position;

        float distance = Vector2.Distance(
            signPosition,
            player.position
        );

        isPlayerNear =
            distance <= highlightDistance;

        if (highlightObject != null)
        {
            highlightObject.SetActive(isPlayerNear);
        }

        // 키보드가 존재하는 경우에만 엔터키 입력 검사
        if (isPlayerNear && IsEnterPressed())
        {
            TryInteract();
        }
    }

    private bool IsEnterPressed()
    {
        if (Keyboard.current == null)
        {
            return false;
        }

        return
            Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame;
    }

    // 키보드와 모바일 터치가 공통으로 호출
public void TryInteract()
{
    Debug.Log(
        $"[표지판 상호작용] {name}, " +
        $"플레이어 근처 여부: {isPlayerNear}"
    );

    if (!isPlayerNear)
    {
        return;
    }

    if (minigamePopup == null ||
        minigamePopup.IsOpen)
    {
        return;
    }

    minigamePopup.OpenPopup(targetSceneName);
}
}