using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>Sand를 기준으로 Sand Shore만 다시 칠합니다. 다른 Tilemap은 수정하지 않습니다.</summary>
public static class SandShoreAutoPainter
{
    private const string AutoTilePath = "Assets/Tiles/Generated/SandShore/SandShoreAuto.asset";
    private const string OldQuarterFolder = "Assets/Tiles/Generated/SandShore/XPAnimated/";

    [MenuItem("Tools/HeartVillage/Rebuild Sand Shore From Sand")]
    public static void Rebuild()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Exit Play mode before rebuilding Sand Shore.");
            return;
        }

        GameObject sandObject = GameObject.Find("SandGrid 16x16/Sand");
        GameObject shoreObject = GameObject.Find("SandGrid 16x16/Sand Shore");
        SandShoreAutoTile autoTile = AssetDatabase.LoadAssetAtPath<SandShoreAutoTile>(AutoTilePath);
        if (sandObject == null || shoreObject == null || autoTile == null)
        {
            Debug.LogError("Sand, Sand Shore, or SandShoreAuto.asset is missing.");
            return;
        }

        Tilemap sand = sandObject.GetComponent<Tilemap>();
        Tilemap shore = shoreObject.GetComponent<Tilemap>();
        if (sand == null || shore == null || sand.transform.parent != shore.transform.parent)
        {
            Debug.LogError("Sand and Sand Shore must be Tilemaps under the same Grid.");
            return;
        }

        var wanted = new HashSet<Vector3Int>();
        foreach (Vector3Int position in sand.cellBounds.allPositionsWithin)
            if (SandShoreAutoTile.IsShoreCell(sand, position))
                wanted.Add(position);

        var changes = new Dictionary<Vector3Int, TileBase>();
        foreach (Vector3Int position in shore.cellBounds.allPositionsWithin)
        {
            TileBase current = shore.GetTile(position);
            if (current == null || wanted.Contains(position))
                continue;

            string path = AssetDatabase.GetAssetPath(current);
            if (current == autoTile || path.StartsWith(OldQuarterFolder, System.StringComparison.Ordinal))
                changes[position] = null;
        }

        int preservedCustomTiles = 0;
        foreach (Vector3Int position in wanted)
        {
            TileBase current = shore.GetTile(position);
            if (current == autoTile)
                continue;

            string path = current != null ? AssetDatabase.GetAssetPath(current) : string.Empty;
            if (current != null && !path.StartsWith(OldQuarterFolder, System.StringComparison.Ordinal))
            {
                preservedCustomTiles++;
                continue;
            }

            changes[position] = autoTile;
        }

        Undo.RegisterCompleteObjectUndo(shore, "Rebuild Sand Shore Auto Tiles");
        foreach (var change in changes)
            shore.SetTile(change.Key, change.Value);

        EditorUtility.SetDirty(shore);
        EditorSceneManager.MarkSceneDirty(shore.gameObject.scene);
        Debug.Log($"Sand Shore rebuilt: {wanted.Count} coast cells, {changes.Count} changed, " +
                  $"{preservedCustomTiles} custom tiles preserved. Save the scene when satisfied.");
    }
}
