using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private InventorySlotUI[] slots;

    private readonly List<ItemData> items = new();

    [Header("테스트용 아이템")]
    [SerializeField] private ItemData testItem;

    private void Start()
    {
        RefreshInventory();
        // 인벤토리 표시 테스트
        if (testItem != null)
            AddItem(testItem);
    }

    public void AddItem(ItemData item)
    {
        // 빈 공간이 없으면 아이템을 추가하지 않음
        if (items.Count >= slots.Length)
        {
            Debug.Log("인벤토리가 가득 찼습니다.");
            return;
        }

        items.Add(item);
        RefreshInventory();
    }

    private void RefreshInventory()
    {
        // 모든 슬롯을 먼저 비움
        foreach (InventorySlotUI slot in slots)
            slot.ClearSlot();

        // 가지고 있는 아이템을 순서대로 표시
        for (int i = 0; i < items.Count; i++)
            slots[i].SetItem(items[i], 1);
    }
}