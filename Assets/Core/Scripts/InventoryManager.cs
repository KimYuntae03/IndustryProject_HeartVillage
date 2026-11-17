using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private InventorySlotUI[] slots;
    [FormerlySerializedAs("testItem"), SerializeField] private ItemData starterItem;
    [SerializeField] private ItemData[] catalog;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text heartPointsText;
    [SerializeField] private Button medicationButton;
    [SerializeField] private MedicationCheckSlot[] medicationSlots;
    [SerializeField] private IslandPlacement placement;
    private string profileName;

    private void OnEnable()
    {
        var profile = LocalPlayerProfile.Load();
        profileName = profile?.playerName;
        var doses = profile?.doseMinutes;
        int doseCount = Mathf.Min(doses?.Length ?? 0, medicationSlots.Length);
        medicationButton.interactable = doseCount > 0;
        for (int i = 0; i < medicationSlots.Length; i++)
        {
            var slot = medicationSlots[i];
            if (i < doseCount) slot.SetDoseTime(doses[i]);
            slot.gameObject.SetActive(i < doseCount);
        }
        VillagePlayerData.GrantStarterOnce(starterItem);
        VillagePlayerData.Changed += Refresh;
        Refresh();
    }

    private void OnDisable() => VillagePlayerData.Changed -= Refresh;

    public ItemData FindItem(string id) => System.Array.Find(catalog, item => item != null && item.Id == id);

    public void AddItem(ItemData item)
    {
        if (item != null && FindItem(item.Id) != null)
            VillagePlayerData.AddItem(item);
    }

    private void Refresh()
    {
        playerNameText.richText = false;
        playerNameText.text = string.IsNullOrWhiteSpace(profileName) ? VillagePlayerData.PlayerName : profileName;
        heartPointsText.text = VillagePlayerData.HeartPoints.ToString();
        int index = 0;
        foreach (var item in catalog)
        {
            if (item == null || index == slots.Length) continue;
            int quantity = VillagePlayerData.Quantity(item.Id);
            if (quantity > 0) slots[index++].SetItem(item, quantity, placement);
        }
        while (index < slots.Length) slots[index++].ClearSlot();
    }
}
