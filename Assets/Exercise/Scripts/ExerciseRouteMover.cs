using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class ExerciseRouteMover : MonoBehaviour
{
    [Header("이동 경로")]
    [SerializeField] private Transform routeRoot;

    [Header("거리 측정")]
    [SerializeField]
    private ExerciseDistanceTracker distanceTracker;

    [Header("걸음 연동 설정")]
    [Tooltip("실제 1m를 게임 안에서 몇 Unit으로 표현할지 설정")]
    [SerializeField, Min(0.001f)]
    private float worldUnitsPerMeter = 0.1f;

    [Tooltip("측정된 거리만큼 플레이어가 따라가는 최대 속도")]
    [SerializeField, Min(0f)]
    private float moveSpeed = 1f;

    [SerializeField]
    private bool loopRoute = true;

    [Header("애니메이션 보정")]
    [Tooltip("마지막 걸음 감지 후 걷기 애니메이션을 유지하는 시간")]
    [SerializeField, Min(0f)]
    private float walkAnimationHoldTime = 1.5f;

    // 마지막으로 확인한 실제 이동 거리
    private float lastMeasuredDistance;

    // 마지막으로 거리 증가가 감지된 시간
    private float lastDistanceIncreaseTime =
        float.NegativeInfinity;

    private readonly List<Transform> routePoints = new();

    private Rigidbody2D rb;
    private Animator animator;

    // 현재 플레이어가 향하고 있는 경로 포인트
    private int targetPointIndex;

    // 시작 버튼을 누른 뒤 경로 이동이 활성화되었는지 확인
    private bool isWalking;

    // 실제 거리값을 바탕으로 게임 안에서 이동한 누적 거리
    private float traveledRouteDistance;

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

    // 씬에 들어오면 Point_00에서 대기
    private void Start()
    {
        MoveToStartPoint();
    }

    private void FixedUpdate()
    {
        if (!isWalking ||
            routePoints.Count < 2 ||
            distanceTracker == null)
        {
            return;
        }

        if (!distanceTracker.IsMeasuring)
        {
            SetIdleAnimation();
            return;
        }

        float measuredDistance =
            distanceTracker.DistanceMeters;

        // 실제 이동 거리가 증가하면 최근 걸음 감지 시간을 갱신
        if (measuredDistance >
            lastMeasuredDistance + 0.001f)
        {
            lastMeasuredDistance = measuredDistance;
            lastDistanceIncreaseTime = Time.time;
        }
        // 측정을 다시 시작해 거리가 0으로 초기화된 경우
        else if (measuredDistance < lastMeasuredDistance)
        {
            lastMeasuredDistance = measuredDistance;
        }

        float targetRouteDistance =
            measuredDistance * worldUnitsPerMeter;

        float remainingDistance =
            targetRouteDistance -
            traveledRouteDistance;

        // 따라갈 거리가 없어도 최근에 걷고 있었다면
        // 일정 시간 동안 걷기 애니메이션을 유지
        if (remainingDistance <= 0.0001f)
        {
            UpdateRecentWalkingAnimation();
            return;
        }

        float movementAmount = Mathf.Min(
            moveSpeed * Time.fixedDeltaTime,
            remainingDistance
        );

        MoveToNextPoint(movementAmount);
    }

    private void UpdateRecentWalkingAnimation()
    {
        bool recentlyWalked =
            Time.time - lastDistanceIncreaseTime
            <= walkAnimationHoldTime;

        animator.SetFloat(
            SpeedHash,
            recentlyWalked ? 1f : 0f
        );
    }

    // ExerciseRoute의 자식들을 Hierarchy 순서대로 등록
    private void LoadRoutePoints()
    {
        routePoints.Clear();

        if (routeRoot == null)
        {
            Debug.LogError(
                "ExerciseRoute가 연결되지 않았습니다."
            );
            return;
        }

        foreach (Transform child in routeRoot)
        {
            routePoints.Add(child);
        }
    }

    // 플레이어를 Point_00으로 이동시키고 대기 상태로 초기화
    private void MoveToStartPoint()
    {
        if (routePoints.Count == 0)
        {
            return;
        }

        rb.position = routePoints[0].position;

        targetPointIndex = 1;
        traveledRouteDistance = 0f;
        isWalking = false;

        SetIdleAnimation();
    }

    // 시작 버튼에서 호출
    public void StartWalking()
    {
        if (routePoints.Count < 2)
        {
            return;
        }

        if (distanceTracker == null)
        {
            Debug.LogError(
                "ExerciseDistanceTracker가 연결되지 않았습니다."
            );
            return;
        }

        // 다시 시작할 경우 Point_00부터 진행
        rb.position = routePoints[0].position;

        targetPointIndex = 1;
        traveledRouteDistance = 0f;
        lastMeasuredDistance = distanceTracker.DistanceMeters;

        lastDistanceIncreaseTime = float.NegativeInfinity;
        isWalking = true;

        SetIdleAnimation();
    }

    public void StopWalking()
    {
        isWalking = false;
        SetIdleAnimation();
    }

    private void MoveToNextPoint(float movementAmount)
    {
        Vector2 currentPosition = rb.position;
        Vector2 targetPosition =
            routePoints[targetPointIndex].position;

        // 이미 포인트에 도착한 상태면 다음 포인트로 변경
        if (Vector2.Distance(
                currentPosition,
                targetPosition
            ) <= 0.001f)
        {
            GoToFollowingPoint();
            return;
        }

        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            targetPosition,
            movementAmount
        );

        Vector2 moveDirection =
            nextPosition - currentPosition;

        // 이번 프레임에서 실제로 이동한 거리
        float actualMovement = Vector2.Distance(
            currentPosition,
            nextPosition
        );

        rb.MovePosition(nextPosition);

        traveledRouteDistance += actualMovement;

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
        if (direction.sqrMagnitude <= 0.00000001f)
        {
            SetIdleAnimation();
            return;
        }

        Vector2 facingDirection =
            GetCardinalDirection(direction);

        animator.SetFloat(
            MoveXHash,
            facingDirection.x
        );

        animator.SetFloat(
            MoveYHash,
            facingDirection.y
        );

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

    private void SetIdleAnimation()
    {
        animator.SetFloat(SpeedHash, 0f);
    }

    private Vector2 GetCardinalDirection(
        Vector2 direction
    )
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