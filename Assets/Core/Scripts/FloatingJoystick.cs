using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

public class FloatingJoystick : OnScreenControl,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [Header("조이스틱 UI")]
    [SerializeField] private RectTransform joystickBackground;
    [SerializeField] private RectTransform joystickHandle;

    [Header("조이스틱 설정")]
    [SerializeField, Min(1f)]
    private float movementRadius = 70f;

    [Header("터치 상호작용")]
    [SerializeField] private Camera worldCamera;

    // 이 거리보다 적게 움직이고 손을 떼면 드래그가 아닌 터치로 판정
    [SerializeField, Min(0f)]
    private float tapMovementThreshold = 25f;

    // 처음에는 Everything으로 사용 가능
    [SerializeField]
    private LayerMask interactableLayer = ~0;

    [InputControl(layout = "Vector2")]
    [SerializeField]
    private string inputControlPath = "<Gamepad>/leftStick";

    private RectTransform touchArea;

    private int activePointerId = int.MinValue;
    private Vector2 pointerDownPosition;
    private bool hasDragged;

    protected override string controlPathInternal
    {
        get => inputControlPath;
        set => inputControlPath = value;
    }

    private void Awake()
    {
        touchArea = GetComponent<RectTransform>();

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        joystickBackground.gameObject.SetActive(false);
    }

    // 화면의 빈 공간을 처음 터치했을 때 호출
    public void OnPointerDown(PointerEventData eventData)
    {
        // 이미 다른 손가락으로 조작 중이면 추가 입력을 무시
        if (activePointerId != int.MinValue)
        {
            return;
        }

        activePointerId = eventData.pointerId;
        pointerDownPosition = eventData.position;
        hasDragged = false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            touchArea,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 touchPosition
        );

        // 터치한 위치에 조이스틱을 표시
        joystickBackground.anchoredPosition = touchPosition;
        joystickHandle.anchoredPosition = Vector2.zero;
        joystickBackground.gameObject.SetActive(true);

        SendValueToControl(Vector2.zero);
    }

    // 화면을 누른 채 움직였을 때 호출
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId)
        {
            return;
        }

        // 최초 터치 지점에서 일정 거리 이상 움직이면 드래그로 판정
        float dragDistance = Vector2.Distance(
            pointerDownPosition,
            eventData.position
        );

        if (dragDistance > tapMovementThreshold)
        {
            hasDragged = true;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 dragPosition
        );

        Vector2 clampedPosition =
            Vector2.ClampMagnitude(
                dragPosition,
                movementRadius
            );

        joystickHandle.anchoredPosition = clampedPosition;

        // 조이스틱 위치를 -1~1 범위의 이동 입력으로 변환
        Vector2 inputValue =
            clampedPosition / movementRadius;

        SendValueToControl(inputValue);
    }

    // 화면에서 손가락을 뗐을 때 호출
    public void OnPointerUp(PointerEventData eventData)
{
    if (eventData.pointerId != activePointerId)
    {
        return;
    }

    bool isTap = !hasDragged;
    Vector2 tapPosition = eventData.position;

    Debug.Log(
        $"[터치 종료] isTap: {isTap}, " +
        $"화면 위치: {tapPosition}"
    );

    ResetJoystick();

    if (isTap)
    {
        TryTouchInteraction(tapPosition);
    }
}

    private void TryTouchInteraction(Vector2 screenPosition)
{
    if (worldCamera == null)
    {
        Debug.LogError("[터치] World Camera가 연결되지 않았습니다.");
        return;
    }

    Vector3 worldPosition =
        worldCamera.ScreenToWorldPoint(screenPosition);

    Collider2D[] touchedColliders =
        Physics2D.OverlapPointAll(
            worldPosition,
            interactableLayer
        );

    Debug.Log(
        $"[터치 검사] 월드 위치: {worldPosition}, " +
        $"감지된 콜라이더: {touchedColliders.Length}개"
    );

    foreach (Collider2D touchedCollider in touchedColliders)
    {
        Debug.Log(
            $"[터치 감지] 콜라이더: {touchedCollider.name}"
        );

        SignTouchTarget touchTarget =
            touchedCollider.GetComponent<SignTouchTarget>();

        if (touchTarget == null)
        {
            continue;
        }

        Debug.Log(
            $"[터치 감지] 상호작용 영역 발견: " +
            $"{touchedCollider.name}"
        );

        touchTarget.Interact();
        return;
    }
}

    private void ResetJoystick()
    {
        activePointerId = int.MinValue;
        hasDragged = false;

        joystickHandle.anchoredPosition = Vector2.zero;
        joystickBackground.gameObject.SetActive(false);

        if (control != null)
        {
            SendValueToControl(Vector2.zero);
        }
    }

    protected override void OnDisable()
    {
        if (control != null)
        {
            SendValueToControl(Vector2.zero);
        }

        activePointerId = int.MinValue;
        hasDragged = false;

        if (joystickBackground != null)
        {
            joystickBackground.gameObject.SetActive(false);
        }

        base.OnDisable();
    }
}