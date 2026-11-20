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
    private float worldUnitsPerMeter = 0.48f;

    [Tooltip("측정된 거리만큼 플레이어가 따라가는 최대 속도")]
    [SerializeField, Min(0f)]
    private float moveSpeed = 3.84f;

    [Tooltip("걸음마다 증가하는 거리를 부드럽게 따라가는 시간(초)")]
    [SerializeField, Min(0.05f)]
    private float movementSmoothTime = 0.6f;

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
    private float routeVelocity;

#if UNITY_EDITOR
    private bool editorWalkingPreview;
    private float editorPreviewSeconds;

    // 에디터에서만 가상 걸음으로 실제 이동 로직을 확인한다. 측정/기록에는 전달하지 않는다.
    public void StartEditorWalkingPreview()
    {
        StartWalking();
        editorPreviewSeconds = 0f;
        editorWalkingPreview = true;
    }
#endif

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

#if UNITY_EDITOR
        if (editorWalkingPreview)
        {
            editorPreviewSeconds += Time.fixedDeltaTime;
            FollowMeasuredDistance(Mathf.Floor(editorPreviewSeconds / 0.5f) * 0.7f);
            return;
        }
#endif

        if (!distanceTracker.IsMeasuring)
        {
            routeVelocity = 0f;
            SetIdleAnimation();
            return;
        }

        FollowMeasuredDistance(distanceTracker.DistanceMeters);
    }

    private void FollowMeasuredDistance(float measuredDistance)
    {
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
            routeVelocity = 0f;
            UpdateRecentWalkingAnimation();
            return;
        }

        // 센서의 계단식 거리 증가를 완만한 이동으로 바꾼다.
        // 측정 거리 자체는 수정하지 않고 화면상의 경로 이동만 보간한다.
        float nextRouteDistance = Mathf.SmoothDamp(
            traveledRouteDistance,
            targetRouteDistance,
            ref routeVelocity,
            movementSmoothTime,
            moveSpeed,
            Time.fixedDeltaTime
        );
        float movementAmount = Mathf.Clamp(
            nextRouteDistance - traveledRouteDistance,
            0f,
            Mathf.Min(remainingDistance, moveSpeed * Time.fixedDeltaTime));

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
        routeVelocity = 0f;
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
        routeVelocity = 0f;
        lastMeasuredDistance = distanceTracker.DistanceMeters;

        lastDistanceIncreaseTime = float.NegativeInfinity;
        isWalking = true;

        SetIdleAnimation();
    }

    public void StopWalking()
    {
#if UNITY_EDITOR
        editorWalkingPreview = false;
#endif
        isWalking = false;
        routeVelocity = 0f;
        SetIdleAnimation();
    }

    private void MoveToNextPoint(float movementAmount)
    {
        Vector2 nextPosition = rb.position;
        Vector2 facingDirection = Vector2.zero;
        int visitedPoints = 0;

        // 모퉁이에 도착해도 남은 이동량은 같은 물리 프레임에 이어서 사용한다.
        // 겹치는 경로 포인트가 있어도 무한 반복하지 않도록 순회 횟수를 제한한다.
        while (movementAmount > 0f && isWalking && visitedPoints < routePoints.Count)
        {
            Vector2 targetPosition = routePoints[targetPointIndex].position;
            Vector2 offset = targetPosition - nextPosition;
            float distanceToPoint = offset.magnitude;
            if (distanceToPoint <= 0.00001f)
            {
                GoToFollowingPoint();
                visitedPoints++;
                continue;
            }

            float segmentMovement = Mathf.Min(movementAmount, distanceToPoint);
            facingDirection = offset / distanceToPoint;
            nextPosition = Vector2.MoveTowards(nextPosition, targetPosition, segmentMovement);
            traveledRouteDistance += segmentMovement;
            movementAmount -= segmentMovement;

            if (segmentMovement >= distanceToPoint)
            {
                GoToFollowingPoint();
                visitedPoints++;
            }
        }

        rb.MovePosition(nextPosition);
        if (!isWalking)
            SetIdleAnimation();
        else if (facingDirection != Vector2.zero)
            UpdateAnimation(facingDirection);
        else
            UpdateRecentWalkingAnimation();
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
