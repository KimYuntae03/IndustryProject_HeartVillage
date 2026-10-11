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
}