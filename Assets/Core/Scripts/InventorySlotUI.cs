using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text quantityText;

    private ItemData current;
    private IslandPlacement placement;

    public void SetItem(ItemData item, int quantity, IslandPlacement target)
    {
        current = item;
        placement = target;
        // 아이템 아이콘 표시
        itemIcon.sprite = item.icon;
        itemIcon.preserveAspect = true;
        itemIcon.enabled = true;

        // 아이템이 2개 이상일 때만 수량 표시
        quantityText.text = quantity > 1
            ? quantity.ToString()
            : "";
    }

    public void ClearSlot()
    {
        current = null;
        // 슬롯에 아이템이 없으면 아이콘과 수량 숨김
        itemIcon.sprite = null;
        itemIcon.enabled = false;
        quantityText.text = "";
    }

    public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (current != null && eventData.button == UnityEngine.EventSystems.PointerEventData.InputButton.Left)
            placement.Begin(current);
    }
}
