using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class ExerciseRouteMover : MonoBehaviour
{
    [Header("이동 경로")]
    [SerializeField] private Transform routeRoot;

    [Header("임시 이동 설정")]
    [SerializeField, Min(0f)] private float moveSpeed = 1.5f;
    [SerializeField] private bool loopRoute = true;

    private readonly List<Transform> routePoints = new();

    private Rigidbody2D rb;
    private Animator animator;

    // 현재 이동하고 있는 목표 포인트 번호
    private int targetPointIndex;

    // 시작 버튼을 누른 이후에만 true
    private bool isWalking;

    private static readonly int MoveXHash =
        Animator.StringToHash("MoveX");

    private static readonly int MoveYHash =
        Animator.StringToHash("MoveY");

    private static readonly int LastMoveXHash =
        Animator.StringToHash("LastMoveX");

    private static readonly int LastMoveYHash =
        Animator.StringToHash("LastMoveY");

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        LoadRoutePoints();
    }

    // 플레이어를 첫 번째 지점인 Point_00으로 이동시키고 대기 상태로 초기화
    private void Start()
    {
        MoveToStartPoint();
    }

    private void FixedUpdate()
    {
        if (!isWalking || routePoints.Count < 2)
        {
            return;
        }

        MoveToNextPoint();
    }

    // ExerciseRoute의 자식들을 Hierarchy 순서대로 가져옴
    private void LoadRoutePoints()
    {
        routePoints.Clear();

        if (routeRoot == null)
        {
            Debug.LogError("ExerciseRoute가 연결되지 않았습니다.");
            return;
        }

        foreach (Transform child in routeRoot)
        {
            routePoints.Add(child);
        }
    }

    // 씬에 들어오면 플레이어를 Point_00에 배치
    private void MoveToStartPoint()
    {
        if (routePoints.Count == 0)
        {
            return;
        }

        rb.position = routePoints[0].position;
        targetPointIndex = 1;
        isWalking = false;

        animator.SetFloat(SpeedHash, 0f);
    }

    // 시작 버튼에서 호출
    public void StartWalking()
    {
        if (routePoints.Count < 2)
        {
            return;
        }

        targetPointIndex = 1;
        isWalking = true;
    }

    public void StopWalking()
    {
        isWalking = false;
        animator.SetFloat(SpeedHash, 0f);
    }

    private void MoveToNextPoint()
    {
        Vector2 currentPosition = rb.position;
        Vector2 targetPosition =
            routePoints[targetPointIndex].position;

        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            targetPosition,
            moveSpeed * Time.fixedDeltaTime
        );

        Vector2 moveDirection =
            nextPosition - currentPosition;

        rb.MovePosition(nextPosition);
        UpdateAnimation(moveDirection);

        // 목표 포인트에 도착하면 다음 포인트로 변경
        if (Vector2.Distance(
                nextPosition,
                targetPosition
            ) <= 0.01f)
        {
            GoToFollowingPoint();
        }
    }

    private void GoToFollowingPoint()
    {
        targetPointIndex++;

        if (targetPointIndex < routePoints.Count)
        {
            return;
        }

        if (loopRoute)
        {
            // 마지막 포인트 다음에는 Point_00으로 이동
            targetPointIndex = 0;
        }
        else
        {
            StopWalking();
        }
    }

    private void UpdateAnimation(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
        {
            animator.SetFloat(SpeedHash, 0f);
            return;
        }

        Vector2 facingDirection =
            GetCardinalDirection(direction);

        animator.SetFloat(MoveXHash, facingDirection.x);
        animator.SetFloat(MoveYHash, facingDirection.y);
        animator.SetFloat(
            LastMoveXHash,
            facingDirection.x
        );
        animator.SetFloat(
            LastMoveYHash,
            facingDirection.y
        );
        animator.SetFloat(SpeedHash, 1f);
    }

    private Vector2 GetCardinalDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) >
            Mathf.Abs(direction.y))
        {
            return new Vector2(
                Mathf.Sign(direction.x),
                0f
            );
        }

        return new Vector2(
            0f,
            Mathf.Sign(direction.y)
        );
    }
}