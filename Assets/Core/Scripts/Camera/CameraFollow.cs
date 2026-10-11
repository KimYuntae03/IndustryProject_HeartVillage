using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("추적 대상")]
    [SerializeField] private Transform target;

    [Header("카메라 설정")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float smoothTime = 0.15f;

    // 실제 카메라가 출력하는 화면의 한 픽셀 크기를 계산할 때 사용한다.
    private Camera cameraComponent;

    // 부드러운 이동 계산값과 화면에 표시할 픽셀 정렬 좌표를 분리한다.
    private Vector3 smoothedPosition;
    private Vector3 velocity;

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        smoothedPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        // 플레이어 위치를 기준으로 카메라가 이동할 목표 위치 계산
        Vector3 targetPosition = target.position + offset;

        // 플레이어를 부드럽게 따라가는 내부 좌표를 계산한다.
        smoothedPosition = Vector3.SmoothDamp(
            smoothedPosition,
            targetPosition,
            ref velocity,
            smoothTime
        );

        Vector3 renderedPosition = smoothedPosition;

        // 카메라를 화면 픽셀 단위로 정렬한다.
        // 부드러운 추적값은 유지하면서 Tilemap 경계가 서브픽셀 사이를 오가며
        // 흰 선처럼 번쩍이는 현상만 방지한다.
        if (cameraComponent != null && cameraComponent.pixelHeight > 0)
        {
            float worldUnitsPerScreenPixel =
                cameraComponent.orthographicSize * 2f / cameraComponent.pixelHeight;

            renderedPosition.x = Mathf.Round(renderedPosition.x / worldUnitsPerScreenPixel)
                * worldUnitsPerScreenPixel;
            renderedPosition.y = Mathf.Round(renderedPosition.y / worldUnitsPerScreenPixel)
                * worldUnitsPerScreenPixel;
        }

        transform.position = renderedPosition;
    }
}
