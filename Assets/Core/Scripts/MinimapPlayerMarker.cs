using UnityEngine;

public class MinimapPlayerMarker : MonoBehaviour
{
    [Header("월드 오브젝트")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform worldBottomLeft;
    [SerializeField] private Transform worldTopRight;

    [Header("미니맵 UI")]
    [SerializeField] private RectTransform minimapRect;
    [SerializeField] private RectTransform playerMarker;

    private void LateUpdate()
    {
        // 맵 전체 범위에서 플레이어가 위치한 비율 계산
        float normalizedX = Mathf.InverseLerp(
            worldBottomLeft.position.x,
            worldTopRight.position.x,
            player.position.x
        );

        float normalizedY = Mathf.InverseLerp(
            worldBottomLeft.position.y,
            worldTopRight.position.y,
            player.position.y
        );

        // 계산한 비율을 미니맵 내부 좌표로 변환
        float markerX = Mathf.Lerp(
            minimapRect.rect.xMin,
            minimapRect.rect.xMax,
            normalizedX
        );

        float markerY = Mathf.Lerp(
            minimapRect.rect.yMin,
            minimapRect.rect.yMax,
            normalizedY
        );

        playerMarker.anchoredPosition =
            new Vector2(markerX, markerY);
    }
}