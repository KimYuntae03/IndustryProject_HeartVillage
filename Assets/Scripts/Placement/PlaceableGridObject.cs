using UnityEngine;

/// <summary>
/// 격자에 맞춰 설치되는 구조물이 차지하는 셀 정보와 스냅 계산을 담당합니다.
/// 실제 설치 가능 여부와 인벤토리 소비는 이후 배치 시스템에서 처리합니다.
/// </summary>
public class PlaceableGridObject : MonoBehaviour
{
    [Header("설치 시 점유하는 격자 크기")]
    [SerializeField, Min(1)] private int footprintWidth = 2;
    [SerializeField, Min(1)] private int footprintHeight = 2;

    [Header("전체 그래픽 크기 (돌출 영역 포함)")]
    [SerializeField, Min(1)] private int visualWidth = 2;
    [SerializeField, Min(1)] private int visualHeight = 3;

    public Vector2Int FootprintSize => new Vector2Int(footprintWidth, footprintHeight);
    public Vector2Int VisualSize => new Vector2Int(visualWidth, visualHeight);

    /// <summary>
    /// 지정한 월드 위치가 들어 있는 셀을 점유 영역의 왼쪽 아래 칸으로 삼아,
    /// 프리팹의 아래쪽 중앙 기준점을 격자 경계에 정확히 맞춘 위치를 반환합니다.
    /// </summary>
    public Vector3 GetSnappedWorldPosition(GridLayout grid, Vector3 worldPosition)
    {
        if (grid == null)
        {
            return worldPosition;
        }

        Vector3Int bottomLeftCell = grid.WorldToCell(worldPosition);
        Vector3 bottomLeftWorld = grid.CellToWorld(bottomLeftCell);
        Vector3 rightEdgeWorld = grid.CellToWorld(
            bottomLeftCell + new Vector3Int(footprintWidth, 0, 0)
        );

        // 루트가 구조물 아래쪽 중앙에 있으므로 가로 점유 폭의 절반만큼 이동합니다.
        Vector3 snappedPosition = Vector3.Lerp(bottomLeftWorld, rightEdgeWorld, 0.5f);
        snappedPosition.z = worldPosition.z;
        return snappedPosition;
    }

    /// <summary>
    /// 미리보기 또는 설치가 확정된 구조물을 지정한 격자에 정렬합니다.
    /// </summary>
    public void SnapToGrid(GridLayout grid, Vector3 worldPosition)
    {
        transform.position = GetSnappedWorldPosition(grid, worldPosition);
    }
}
