using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    // 초당 이동 거리
    [SerializeField, Min(0f)]
    private float moveSpeed = 3f;

    // 물리 이동, 애니메이션, 입력 처리를 담당하는 컴포넌트
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerInput playerInput;

    // 실제 이동에 사용하는 정규화된 입력값
    private Vector2 moveInput;

    // 이동을 멈춘 뒤에도 Idle 방향을 유지하기 위한 마지막 방향
    private Vector2 lastMoveDirection = Vector2.down;

    // 문자열 오타와 매 프레임 문자열 검색을 피하기 위한 Animator 파라미터 해시
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");
    private static readonly int LastMoveXHash = Animator.StringToHash("LastMoveX");
    private static readonly int LastMoveYHash = Animator.StringToHash("LastMoveY");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();

        // 게임 시작 시 아래쪽을 바라보는 Idle 애니메이션이 재생되게 초기화
        animator.SetFloat(MoveXHash, 0f);
        animator.SetFloat(MoveYHash, -1f);
        animator.SetFloat(LastMoveXHash, 0f);
        animator.SetFloat(LastMoveYHash, -1f);
        animator.SetFloat(SpeedHash, 0f);
    }

    private void OnEnable()
    {
        playerInput.onActionTriggered += OnActionTriggered;
    }

    private void OnDisable()
    {
        playerInput.onActionTriggered -= OnActionTriggered;
        moveInput = Vector2.zero;
    }

    // Player Input에서 액션이 발생할 때 Move 액션만 골라 처리
    private void OnActionTriggered(InputAction.CallbackContext context)
    {
        if (context.action.name != "Move")
        {
            return;
        }

        Vector2 rawInput = context.ReadValue<Vector2>();

        // 대각선 입력의 길이를 1로 제한해 직선 이동보다 빨라지지 않게 함
        moveInput = Vector2.ClampMagnitude(rawInput, 1f);
    }

    private void Update()
    {
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        // Rigidbody2D 이동은 물리 프레임에서 처리
        Vector2 nextPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(nextPosition);
    }

    private void UpdateAnimation()
    {
        bool isMoving = moveInput.sqrMagnitude > 0.0001f;

        if (isMoving)
        {
            // 대각선 애니메이션이 없으므로 더 강하게 입력된 축을 4방향 값으로 변환
            Vector2 facingDirection = GetCardinalDirection(moveInput);
            lastMoveDirection = facingDirection;

            animator.SetFloat(MoveXHash, facingDirection.x);
            animator.SetFloat(MoveYHash, facingDirection.y);
            animator.SetFloat(LastMoveXHash, lastMoveDirection.x);
            animator.SetFloat(LastMoveYHash, lastMoveDirection.y);
        }

        // Speed가 0.01보다 크면 Walk, 작으면 Idle 상태로 전환
        animator.SetFloat(SpeedHash, moveInput.magnitude);
    }

    private Vector2 GetCardinalDirection(Vector2 input)
    {
        float absoluteX = Mathf.Abs(input.x);
        float absoluteY = Mathf.Abs(input.y);

        if (absoluteX > absoluteY)
        {
            return new Vector2(Mathf.Sign(input.x), 0f);
        }

        if (absoluteY > absoluteX)
        {
            return new Vector2(0f, Mathf.Sign(input.y));
        }

        // 키보드 대각선처럼 두 축의 세기가 같으면 마지막 방향의 축을 우선시
        if (Mathf.Abs(lastMoveDirection.x) > 0f)
        {
            return new Vector2(Mathf.Sign(input.x), 0f);
        }

        return new Vector2(0f, Mathf.Sign(input.y));
    }
}