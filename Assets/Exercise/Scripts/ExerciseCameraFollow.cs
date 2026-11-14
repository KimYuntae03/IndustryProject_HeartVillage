using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ExerciseCameraFollow : MonoBehaviour
{
    [Header("추적 대상")]
    [SerializeField] private Transform target;

    [Header("카메라 이동")]
    [SerializeField] private float followSmoothTime = 0.15f;

    private Camera targetCamera;
    private float landscapeSize;
    private Vector3 smoothedPosition;
    private Vector3 followVelocity;

    // 씬 시작 시 카메라 컴포넌트와 초기 줌 크기를 설정
    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
        smoothedPosition = transform.position;

        // 씬에 설정된 크기는 마을의 가로 화면을 기준으로 한다.
        landscapeSize = targetCamera.orthographicSize;
        MatchLandscapeZoom();

        // 픽셀 아트 타일 경계가 이동 중 보간되어 번쩍이지 않도록 한다.
        targetCamera.allowMSAA = false;
        targetCamera.allowDynamicResolution = false;
    }

    private void LateUpdate()
    {
        // 기기 회전이 Awake 이후 완료되는 경우에도 실제 뷰포트에 맞춘다.
        MatchLandscapeZoom();
        FollowPlayer();
    }

    private void FollowPlayer()
    {
        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z
        );

        smoothedPosition = Vector3.SmoothDamp(
            smoothedPosition,
            targetPosition,
            ref followVelocity,
            followSmoothTime
        );

        Vector3 renderedPosition = smoothedPosition;

        // 부드러운 추적 좌표는 유지하되 실제 카메라 위치만 화면 픽셀에 맞춘다.
        // Tilemap이 서브픽셀 사이를 오갈 때 세로 경계가 흰 선처럼
        // 번쩍이는 현상을 CoreGameplay 카메라와 같은 방식으로 방지한다.
        if (targetCamera.pixelHeight > 0)
        {
            float worldUnitsPerScreenPixel =
                targetCamera.orthographicSize * 2f / targetCamera.pixelHeight;

            renderedPosition.x = Mathf.Round(renderedPosition.x / worldUnitsPerScreenPixel)
                * worldUnitsPerScreenPixel;
            renderedPosition.y = Mathf.Round(renderedPosition.y / worldUnitsPerScreenPixel)
                * worldUnitsPerScreenPixel;
        }

        transform.position = renderedPosition;
    }

    private void MatchLandscapeZoom()
    {
        int shortSide = Mathf.Min(targetCamera.pixelWidth, targetCamera.pixelHeight);
        if (shortSide > 0)
            targetCamera.orthographicSize = landscapeSize * targetCamera.pixelHeight / shortSide;
    }
}
