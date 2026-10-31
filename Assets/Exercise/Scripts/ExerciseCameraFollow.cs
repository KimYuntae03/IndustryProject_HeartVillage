using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ExerciseCameraFollow : MonoBehaviour
{
    [Header("추적 대상")]
    [SerializeField] private Transform target;

    [Header("카메라 이동")]
    [SerializeField] private float followSmoothTime = 0.15f;

    [Header("카메라 줌")]
    [SerializeField] private float startSize = 2f;
    [SerializeField] private float targetSize = 2f;
    [SerializeField] private float zoomSpeed = 1f;

    private Camera targetCamera;
    private Vector3 smoothedPosition;
    private Vector3 followVelocity;

    // 씬 시작 시 카메라 컴포넌트와 초기 줌 크기를 설정
    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
        smoothedPosition = transform.position;

        // 운동 씬 진입 시 카메라 크기를 2로 시작
        targetCamera.orthographicSize = startSize;

        // 픽셀 아트 타일 경계가 이동 중 보간되어 번쩍이지 않도록 한다.
        targetCamera.allowMSAA = false;
        targetCamera.allowDynamicResolution = false;
    }

    // 플레이어 이동이 끝난 후 카메라 위치를 갱신하고 목표 크기까지 확대(카메라 떨림 방지)
    private void LateUpdate()
    {
        UpdateZoom();
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

    private void UpdateZoom()
    {
        // Orthographic Size를 2에서 1.5까지 부드럽게 변경
        targetCamera.orthographicSize =
            Mathf.MoveTowards(
                targetCamera.orthographicSize,
                targetSize,
                zoomSpeed * Time.deltaTime
            );
    }
}
