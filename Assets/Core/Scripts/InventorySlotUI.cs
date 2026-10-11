using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text quantityText;

    public void SetItem(ItemData item, int quantity)
    {
        // 아이템 아이콘 표시
        itemIcon.sprite = item.icon;
        itemIcon.enabled = true;

        // 아이템이 2개 이상일 때만 수량 표시
        quantityText.text = quantity > 1
            ? quantity.ToString()
            : "";
    }

    public void ClearSlot()
    {
        // 슬롯에 아이템이 없으면 아이콘과 수량 숨김
        itemIcon.sprite = null;
        itemIcon.enabled = false;
        quantityText.text = "";
    }
}