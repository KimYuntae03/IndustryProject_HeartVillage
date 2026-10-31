using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class FloatingJoystick : OnScreenControl,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [Header("Joystick UI")]
    [SerializeField] private RectTransform joystickBackground;
    [SerializeField] private RectTransform joystickHandle;

    [Header("Joystick Settings")]
    [SerializeField, Min(1f)] private float movementRadius = 70f;
    [SerializeField, Min(1f)] private float dragStartDistance = 20f;
    [SerializeField, Min(0.05f)] private float maxTapDuration = 0.35f;

    [InputControl(layout = "Vector2")]
    [SerializeField] private string inputControlPath = "<Gamepad>/leftStick";

    private RectTransform touchArea;
    private int activePointerId = int.MinValue;
    private Vector2 pointerDownScreenPosition;
    private Vector2 joystickOriginLocalPosition;
    private float pointerDownTime;
    private bool isDraggingJoystick;

    protected override string controlPathInternal
    {
        get => inputControlPath;
        set => inputControlPath = value;
    }

    private void Awake()
    {
        if (!EnsureInitialized())
        {
            enabled = false;
            return;
        }

        HideJoystick();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!EnsureInitialized())
        {
            return;
        }

        if (activePointerId != int.MinValue)
        {
            return;
        }

        activePointerId = eventData.pointerId;
        pointerDownScreenPosition = eventData.position;
        pointerDownTime = Time.unscaledTime;
        isDraggingJoystick = false;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                touchArea,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPosition))
        {
            ResetJoystick();
            return;
        }

        joystickOriginLocalPosition = localPosition;
        joystickHandle.anchoredPosition = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId)
        {
            return;
        }

        Vector2 screenDelta = eventData.position - pointerDownScreenPosition;

        if (!isDraggingJoystick)
        {
            if (screenDelta.magnitude < dragStartDistance)
            {
                return;
            }

            isDraggingJoystick = true;
            joystickBackground.anchoredPosition = joystickOriginLocalPosition;
            joystickBackground.gameObject.SetActive(true);
            SendValueToControl(Vector2.zero);
        }

        Vector2 clampedPosition = Vector2.ClampMagnitude(screenDelta, movementRadius);
        joystickHandle.anchoredPosition = clampedPosition;
        SendValueToControl(clampedPosition / movementRadius);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId == activePointerId)
        {
            if (!isDraggingJoystick && Time.unscaledTime - pointerDownTime <= maxTapDuration)
            {
                TryWorldInteraction(eventData.position);
            }

            ResetJoystick();
        }
    }

    private void TryWorldInteraction(Vector2 screenPosition)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        Collider2D[] hitColliders = Physics2D.OverlapPointAll(worldPosition);

        foreach (Collider2D hitCollider in hitColliders)
        {
            SignTouchTarget touchTarget = hitCollider.GetComponent<SignTouchTarget>();
            if (touchTarget != null && touchTarget.TryInteract())
            {
                return;
            }
        }
    }

    private void ResetJoystick()
    {
        if (control != null)
        {
            SendValueToControl(Vector2.zero);
        }

        activePointerId = int.MinValue;
        isDraggingJoystick = false;
        HideJoystick();
    }

    private bool EnsureInitialized()
    {
        if (touchArea == null)
        {
            touchArea = (RectTransform)transform;
        }

        if (joystickBackground != null && joystickHandle != null)
        {
            return true;
        }

        Debug.LogError("FloatingJoystick UI references are not assigned.", this);
        return false;
    }

    private void HideJoystick()
    {
        if (joystickHandle != null)
        {
            joystickHandle.anchoredPosition = Vector2.zero;
        }

        if (joystickBackground != null)
        {
            joystickBackground.gameObject.SetActive(false);
        }
    }

    protected override void OnDisable()
    {
        ResetJoystick();
        base.OnDisable();
    }
}
