using UnityEngine;

[CreateAssetMenu(
    fileName = "NewItem",
    menuName = "HeartVillage/Item Data"
)]
public class ItemData : ScriptableObject
{
    [Header("아이템 정보")]
    public string itemName;
    public Sprite icon;
    [SerializeField] private string itemId;
    [SerializeField] private Sprite placementSprite;
    [SerializeField] private GameObject placementPrefab;
    [Tooltip("바닥 격자 기준으로 설치에 필요한 가로, 세로 칸 수")]
    [SerializeField] private Vector2Int footprint = Vector2Int.one;

    public string Id => string.IsNullOrEmpty(itemId) ? name : itemId;
    public Sprite PlacementSprite => placementSprite != null ? placementSprite : icon;
    public GameObject PlacementPrefab => placementPrefab;
    public Vector2Int Footprint => Vector2Int.Max(Vector2Int.one, footprint);
}
