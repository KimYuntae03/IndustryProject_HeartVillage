using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Sand Tilemap의 실제 16×16 셀 배치를 읽어 해안 조각을 고르는 타일입니다.
/// 기존 RPG Maker XP 해안 Animated Tile의 프레임과 에셋은 그대로 재사용합니다.
/// </summary>
[CreateAssetMenu(fileName = "SandShoreAuto", menuName = "HeartVillage/Tiles/Sand Shore Auto Tile")]
public sealed class SandShoreAutoTile : TileBase
{
    [SerializeField] private TileBase[] quarterTiles = new TileBase[48];

    public void SetQuarterTiles(TileBase[] tiles)
    {
        if (tiles == null || tiles.Length != 48)
            throw new System.ArgumentException("Exactly 48 XP quarter tiles are required.", nameof(tiles));

        quarterTiles = (TileBase[])tiles.Clone();
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = null;
        tileData.color = Color.white;
        tileData.transform = Matrix4x4.identity;
        tileData.colliderType = Tile.ColliderType.None;

        TileBase quarter = GetQuarter(position, tilemap);
        if (quarter == null)
            return;

        quarter.GetTileData(position, tilemap, ref tileData);
        tileData.colliderType = Tile.ColliderType.None;
    }

    public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData data)
    {
        TileBase quarter = GetQuarter(position, tilemap);
        if (quarter == null || !quarter.GetTileAnimationData(position, tilemap, ref data))
            return false;

        // 서로 다른 조각을 사용해도 같은 8프레임 시계로 재생합니다.
        data.flags |= TileAnimationFlags.SyncAnimation;
        return true;
    }

    private TileBase GetQuarter(Vector3Int position, ITilemap tilemap)
    {
        Tilemap shore = tilemap.GetComponent<Tilemap>();
        if (quarterTiles == null)
            return null;

        // Palette에는 Sand가 없으므로 위쪽 해안 조각을 아이콘처럼 보여줍니다.
        if (shore == null || shore.transform.parent == null)
            return quarterTiles.Length > 14 ? quarterTiles[14] : null;

        Transform sandTransform = shore.transform.parent.Find("Sand");
        Tilemap sand = sandTransform != null ? sandTransform.GetComponent<Tilemap>() : null;
        if (sand == null)
            return quarterTiles.Length > 14 ? quarterTiles[14] : null;
        if (!sand.HasTile(position))
            return null;

        int index = SelectQuarterIndex(sand, position);
        return index >= 0 && index < quarterTiles.Length ? quarterTiles[index] : null;
    }

    public static bool IsShoreCell(Tilemap sand, Vector3Int position)
    {
        if (sand == null || !sand.HasTile(position))
            return false;

        for (int y = -1; y <= 1; y++)
            for (int x = -1; x <= 1; x++)
                if ((x != 0 || y != 0) && !sand.HasTile(position + new Vector3Int(x, y, 0)))
                    return true;

        return false;
    }

    private static int CardinalMask(Tilemap sand, Vector3Int p)
    {
        int mask = 0;
        if (!sand.HasTile(p + Vector3Int.up)) mask |= 1;
        if (!sand.HasTile(p + Vector3Int.right)) mask |= 2;
        if (!sand.HasTile(p + Vector3Int.down)) mask |= 4;
        if (!sand.HasTile(p + Vector3Int.left)) mask |= 8;
        return mask;
    }

    public static int SelectQuarterIndex(Tilemap sand, Vector3Int p)
    {
        if (sand == null || !sand.HasTile(p))
            return -1;

        bool Has(int x, int y) => sand.HasTile(p + new Vector3Int(x, y, 0));
        int mask = CardinalMask(sand, p);

        // XP 원본의 모서리 조각: NW=12, NE=17, SW=42, SE=47.
        // 특히 SW/SE는 기존 배치에서 뒤바뀌어 아래쪽에 톱니가 생겼습니다.
        switch (mask)
        {
            case 9: return 12;
            case 3: return 17;
            case 12: return 42;
            case 6: return 47;

            case 1: // 위쪽 바다
                if (Has(-1, 0) && CardinalMask(sand, p + Vector3Int.left) == 9) return 13;
                if (Has(1, 0) && CardinalMask(sand, p + Vector3Int.right) == 3) return 16;
                return (p.x & 1) == 0 ? 14 : 15;

            case 2: // 오른쪽 바다
                if (Has(0, 1) && CardinalMask(sand, p + Vector3Int.up) == 3) return 23;
                if (Has(0, -1) && CardinalMask(sand, p + Vector3Int.down) == 6) return 41;
                return (p.y & 1) == 0 ? 35 : 29;

            case 4: // 아래쪽 바다
                if (Has(-1, 0) && CardinalMask(sand, p + Vector3Int.left) == 12) return 43;
                if (Has(1, 0) && CardinalMask(sand, p + Vector3Int.right) == 6) return 46;
                return (p.x & 1) == 0 ? 44 : 45;

            case 8: // 왼쪽 바다
                if (Has(0, 1) && CardinalMask(sand, p + Vector3Int.up) == 9) return 18;
                if (Has(0, -1) && CardinalMask(sand, p + Vector3Int.down) == 12) return 36;
                return (p.y & 1) == 0 ? 30 : 24;

            case 0: // 오목한 모서리는 대각선도 검사합니다.
                if (!Has(-1, 1)) return 4;
                if (!Has(1, 1)) return 5;
                if (!Has(-1, -1)) return 10;
                if (!Has(1, -1)) return 11;
                return 26;

            default:
                // 현재 섬에는 없지만, 나중에 매우 좁은 지형을 그릴 때의 안전한 대체값입니다.
                if ((mask & 1) != 0 && (mask & 8) != 0) return 12;
                if ((mask & 1) != 0 && (mask & 2) != 0) return 17;
                if ((mask & 4) != 0 && (mask & 8) != 0) return 42;
                if ((mask & 4) != 0 && (mask & 2) != 0) return 47;
                if ((mask & 1) != 0) return 14;
                if ((mask & 4) != 0) return 44;
                if ((mask & 8) != 0) return 24;
                if ((mask & 2) != 0) return 29;
                return 26;
        }
    }
}
