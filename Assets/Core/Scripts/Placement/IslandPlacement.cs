using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public sealed class IslandPlacement : MonoBehaviour
{
    [SerializeField] private Tilemap ground;
    [SerializeField] private Grid objectsGrid;
    [SerializeField] private Collider2D playerBody;
    [SerializeField] private InventoryManager inventory;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private MinigamePopup popups;
    [SerializeField] private TMP_Text hint;
    [SerializeField, Range(1, 254)] private int previewAlpha = 155;

    private Tilemap[] objectMaps;
    private SpriteRenderer[] objectSprites;
    private Collider2D[] objectColliders;
    private ItemData selected;
    private GameObject preview;
    private readonly Dictionary<Vector3Int, GameObject> placedObjects = new();
    private readonly List<Collider2D> collidersToEnableOnPlacement = new();
    private SpriteRenderer[] previewParts;
    private Color[] originalColors;
    private int[] originalOrders;
    private Vector3Int lastValidCell;
    private Vector3 grabOffset;
    private bool grabbed;
    private bool readyToConfirm;
    public bool IsPlacing => selected != null;
    public bool IsRecovering { get; private set; }

    private void Awake()
    {
        objectMaps = objectsGrid.GetComponentsInChildren<Tilemap>(true);
        objectSprites = objectsGrid.GetComponentsInChildren<SpriteRenderer>(true);
        objectColliders = objectsGrid.GetComponentsInChildren<Collider2D>(true);
    }

    private void OnEnable()
    {
        inventoryUI.Opened += Cancel;
        popups.Opened += Cancel;
    }

    private void Start()
    {
        foreach (var decoration in VillagePlayerData.Decorations)
        {
            var item = inventory.FindItem(decoration.itemId);
            if (item != null)
                placedObjects[decoration.cell] = CreateDecoration(item, decoration.cell, decoration.Area.size);
        }
    }

    private void OnDisable()
    {
        inventoryUI.Opened -= Cancel;
        popups.Opened -= Cancel;
        Cancel();
    }

    public void Begin(ItemData item)
    {
        if ((item.PlacementPrefab == null && item.PlacementSprite == null) || VillagePlayerData.Quantity(item.Id) <= 0) return;
        Cancel();
        inventoryUI.CloseInventory();
        hint.gameObject.SetActive(true);
        selected = item;
        if (!TryFindNearbyCell(out lastValidCell))
        {
            selected = null;
            hint.text = "주변에 빈 땅이 없어요 · 이동한 뒤 다시 선택하세요";
            return;
        }
        preview = CreateDecoration(item, lastValidCell, item.Footprint);
        foreach (var collider in preview.GetComponentsInChildren<Collider2D>(true))
        {
            if (!collider.enabled) continue;
            collider.enabled = false;
            collidersToEnableOnPlacement.Add(collider);
        }
        previewParts = preview.GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[previewParts.Length];
        originalOrders = new int[previewParts.Length];
        int highest = int.MinValue;
        for (int i = 0; i < previewParts.Length; i++)
        {
            originalColors[i] = previewParts[i].color;
            originalOrders[i] = previewParts[i].sortingOrder;
            highest = Mathf.Max(highest, originalOrders[i]);
        }
        for (int i = 0; i < previewParts.Length; i++)
            previewParts[i].sortingOrder = Mathf.Max(short.MinValue, short.MaxValue - (highest - originalOrders[i]));
        TintPreview(true);
        hint.text = "미리보기를 끌어서 옮긴 뒤 다시 누르면 설치해요";
    }

    public void ToggleRecovery()
    {
        if (popups.IsOpen) return;
        bool turnOn = !IsRecovering;
        Cancel();
        if (!turnOn) return;
        inventoryUI.CloseInventory();
        IsRecovering = true;
        hint.text = "가방에 담을 장식을 누르세요";
        hint.gameObject.SetActive(true);
    }

    public void RecoverAt(Vector3 worldPosition)
    {
        if (!IsRecovering) return;
        var cell = (Vector2Int)ground.WorldToCell(worldPosition);
        foreach (var decoration in VillagePlayerData.Decorations)
        {
            if (!decoration.Area.Contains(cell) || !placedObjects.TryGetValue(decoration.cell, out var instance))
                continue;
            foreach (var part in instance.GetComponentsInChildren<SpriteRenderer>())
            {
                var bounds = part.bounds;
                if (!part.enabled || !bounds.Contains(new Vector3(worldPosition.x, worldPosition.y, bounds.center.z)))
                    continue;
                if (!VillagePlayerData.TryRecover(decoration)) return;
                placedObjects.Remove(decoration.cell);
                instance.SetActive(false);
                Destroy(instance);
                return;
            }
        }
    }

    // 플레이어 칸에서 시작해 가까운 순서(상하좌우 이동 거리)로 찾는다.
    private bool TryFindNearbyCell(out Vector3Int cell)
    {
        var origin = ground.WorldToCell(playerBody.bounds.center);
        origin.z = 0;
        const int radius = 16;
        for (int distance = 0; distance <= radius; distance++)
        {
            for (int x = -distance; x <= distance; x++)
            {
                int y = distance - Mathf.Abs(x);
                cell = origin + new Vector3Int(x, -y, 0);
                if (IsAvailable(cell)) return true;
                if (y == 0) continue;
                cell.y += 2 * y;
                if (IsAvailable(cell)) return true;
            }
        }
        cell = default;
        return false;
    }

    public bool TryGrab(Vector2 screenPosition)
    {
        if (!IsPlacing || !TryHitPreview(screenPosition, out Vector3 world)) return false;
        grabbed = true;
        grabOffset = ground.GetCellCenterWorld(lastValidCell) - world;
        return true;
    }

    private bool TryHitPreview(Vector2 screenPosition, out Vector3 world)
    {
        world = default;
        var camera = Camera.main;
        if (camera == null || preview == null) return false;
        var bounds = VisualBounds(previewParts);
        var center = camera.WorldToScreenPoint(bounds.center);
        if (center.z <= 0f) return false;
        var corner = camera.WorldToScreenPoint(bounds.max);
        // 한 칸짜리 꽃도 손가락으로 잡을 수 있게 최소 터치 영역을 확보한다.
        float minRadius = Mathf.Clamp(Screen.dpi * 0.12f, 18f, 48f);
        if (Mathf.Abs(screenPosition.x - center.x) > Mathf.Max(minRadius, Mathf.Abs(corner.x - center.x)) ||
            Mathf.Abs(screenPosition.y - center.y) > Mathf.Max(minRadius, Mathf.Abs(corner.y - center.y)))
            return false;
        world = camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, center.z));
        return true;
    }

    public void DragPreview(Vector2 screenPosition)
    {
        if (!grabbed) return;
        var camera = Camera.main;
        if (camera == null) return;
        var depth = camera.WorldToScreenPoint(ground.transform.position).z;
        var world = camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, depth));
        var cell = ground.WorldToCell(world + grabOffset);
        cell.z = 0;
        bool available = IsAvailable(cell);
        if (available) lastValidCell = cell;
        MoveToCell(preview, previewParts, selected, cell, selected.Footprint);
        TintPreview(available);
    }

    public void ReleasePreview(Vector2 screenPosition, bool dragged, bool overWorld)
    {
        if (!grabbed) return;
        if (dragged && overWorld) DragPreview(screenPosition);
        bool confirm = overWorld && !dragged && readyToConfirm && TryHitPreview(screenPosition, out _);
        ReturnToValidCell();
        if (!overWorld) return;
        if (confirm && IsAvailable(lastValidCell) && VillagePlayerData.TryPlace(selected, lastValidCell))
        {
            foreach (var collider in collidersToEnableOnPlacement)
                collider.enabled = true;
            for (int i = 0; i < previewParts.Length; i++)
            {
                previewParts[i].color = originalColors[i];
                previewParts[i].sortingOrder = originalOrders[i];
            }
            placedObjects[lastValidCell] = preview;
            preview = null; // 확정된 미리보기 오브젝트가 설치물로 남는다.
            Cancel();
            return;
        }
        readyToConfirm = true;
    }

    public void ReturnToValidCell()
    {
        if (!grabbed) return;
        grabbed = false;
        MoveToCell(preview, previewParts, selected, lastValidCell, selected.Footprint);
        TintPreview(IsAvailable(lastValidCell));
    }

    private void TintPreview(bool available)
    {
        Color tint = available ? Color.white : new Color32(255, 90, 90, 255);
        tint.a = previewAlpha / 255f;
        for (int i = 0; i < previewParts.Length; i++)
            previewParts[i].color = originalColors[i] * tint;
    }

    private bool IsAvailable(Vector3Int cell)
    {
        var areaCells = new RectInt((Vector2Int)cell, selected.Footprint);
        if (VillagePlayerData.IsOccupied(areaCells)) return false;
        foreach (var position in areaCells.allPositionsWithin)
            if (!ground.HasTile((Vector3Int)position)) return false;

        // 바닥과 소품 격자의 셀 크기가 달라서 바닥 한 칸의 전체 면적을 검사한다.
        Vector2 min = ground.CellToWorld(cell);
        Vector2 max = ground.CellToWorld((Vector3Int)areaCells.max);
        Vector2 inset = (max - min) * 0.001f;
        min += inset;
        max -= inset;
        var area = new Rect(min, max - min);
        foreach (var map in objectMaps)
        {
            if (!map.gameObject.activeInHierarchy) continue;
            var first = map.WorldToCell(min);
            var last = map.WorldToCell(max);
            for (int y = first.y; y <= last.y; y++)
                for (int x = first.x; x <= last.x; x++)
                    if (map.HasTile(new Vector3Int(x, y, 0))) return false;
        }
        foreach (var sprite in objectSprites)
            if (sprite.enabled && sprite.gameObject.activeInHierarchy && Overlaps(area, sprite.bounds)) return false;

        // 타일맵의 합성 콜라이더 바운드는 떨어진 소품 사이의 빈 땅까지 포함한다.
        // 실제 물리 모양에 겹친 콜라이더만 소품/플레이어인지 확인한다.
        foreach (var hit in Physics2D.OverlapAreaAll(min, max))
            if (hit == playerBody || System.Array.IndexOf(objectColliders, hit) >= 0) return false;
        return true;
    }

    private static bool Overlaps(Rect area, Bounds bounds) =>
        area.Overlaps(Rect.MinMaxRect(bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y));

    private GameObject CreateDecoration(ItemData item, Vector3Int cell, Vector2Int sizeInCells)
    {
        GameObject instance;
        if (item.PlacementPrefab != null)
        {
            instance = Instantiate(item.PlacementPrefab, ground.transform, false);
        }
        else
        {
            instance = new GameObject(item.itemName);
            instance.transform.SetParent(ground.transform, false);
            var renderer = instance.AddComponent<SpriteRenderer>();
            renderer.sprite = item.PlacementSprite;
            var groundRenderer = ground.GetComponent<TilemapRenderer>();
            renderer.sortingLayerID = groundRenderer.sortingLayerID;
            renderer.sortingOrder = groundRenderer.sortingOrder + 1;
            Vector3 size = renderer.sprite.bounds.size;
            float scale = Mathf.Min(ground.cellSize.x / size.x, ground.cellSize.y / size.y);
            renderer.transform.localScale = new Vector3(scale, scale, 1f);
        }
        MoveToCell(instance, instance.GetComponentsInChildren<SpriteRenderer>(), item, cell, sizeInCells);
        return instance;
    }

    private static Bounds VisualBounds(SpriteRenderer[] parts)
    {
        var bounds = parts[0].bounds;
        for (int i = 1; i < parts.Length; i++) bounds.Encapsulate(parts[i].bounds);
        return bounds;
    }

    private void MoveToCell(GameObject instance, SpriteRenderer[] parts, ItemData item,
        Vector3Int cell, Vector2Int sizeInCells)
    {
        var first = ground.GetCellCenterWorld(cell);
        var last = ground.GetCellCenterWorld(cell + (Vector3Int)(sizeInCells - Vector2Int.one));
        var target = (first + last) * 0.5f;
        var bounds = VisualBounds(parts);
        var anchor = bounds.center;
        if (item.PlacementPrefab != null)
        {
            // 프리팹의 피벗이 소품마다 달라도 받침대 하단을 점유 영역 아래쪽에 맞춘다.
            anchor.y = bounds.min.y;
            target.y = ground.CellToWorld(cell).y;
        }
        instance.transform.position += target - anchor;
    }

    private void Cancel()
    {
        IsRecovering = false;
        grabbed = false;
        readyToConfirm = false;
        selected = null;
        if (hint != null) hint.gameObject.SetActive(false);
        if (preview != null) Destroy(preview);
        collidersToEnableOnPlacement.Clear();
        preview = null;
        previewParts = null;
        originalColors = null;
        originalOrders = null;
    }
}
