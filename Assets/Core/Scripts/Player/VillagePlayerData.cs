using System;
using System.Collections.Generic;
using UnityEngine;

// 마을과 미니게임이 공유하는 로컬 플레이어 데이터.
public static class VillagePlayerData
{
    private const string SaveKey = "HeartVillage.Player.V1";

    [Serializable]
    private class Stock
    {
        public string itemId;
        public int count;
    }

    [Serializable]
    public struct Decoration
    {
        public string itemId;
        public Vector3Int cell;
        public Vector2Int size;

        // 기존 한 칸짜리 꽃 기록에는 size가 없다.
        public RectInt Area => new RectInt((Vector2Int)cell, Vector2Int.Max(Vector2Int.one, size));
    }

    [Serializable]
    private class SaveData
    {
        public string playerName = "플레이어";
        public int heartPoints;
        public bool starterGranted;
        public List<Stock> inventory = new();
        public List<Decoration> decorations = new();
    }

    private static SaveData loaded;
    private static SaveData Data => loaded ??= PlayerPrefs.HasKey(SaveKey)
        ? JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey))
        : new SaveData();

    public static event Action Changed;
    public static string PlayerName => Data.playerName;
    public static int HeartPoints => Data.heartPoints;
    public static IReadOnlyList<Decoration> Decorations => Data.decorations;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        loaded = null;
        Changed = null;
    }

    public static void SetPlayerName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        Data.playerName = value.Trim();
        Save();
    }

    public static bool AddHeartPoints(int amount)
    {
        if (amount <= 0 || amount > int.MaxValue - Data.heartPoints) return false;
        Data.heartPoints += amount;
        Save();
        return true;
    }

    public static bool TrySpendHeartPoints(int amount)
    {
        if (amount <= 0 || amount > Data.heartPoints) return false;
        Data.heartPoints -= amount;
        Save();
        return true;
    }

    public static int Quantity(string itemId) =>
        Data.inventory.Find(entry => entry.itemId == itemId)?.count ?? 0;

    public static bool AddItem(ItemData item, int count = 1)
    {
        if (item == null || count <= 0 || !IncreaseStock(item.Id, count)) return false;
        Save();
        return true;
    }

    private static bool IncreaseStock(string id, int count)
    {
        var entry = Data.inventory.Find(stock => stock.itemId == id);
        if (entry == null)
        {
            entry = new Stock { itemId = id };
            Data.inventory.Add(entry);
        }
        if (count > int.MaxValue - entry.count) return false;
        entry.count += count;
        return true;
    }

    internal static void GrantStarterOnce(ItemData item)
    {
        if (Data.starterGranted || item == null) return;
        Data.starterGranted = true;
        AddItem(item);
    }

    internal static bool TryPlace(ItemData item, Vector3Int cell)
    {
        var entry = Data.inventory.Find(stock => stock.itemId == item.Id);
        if (entry == null || entry.count == 0 || IsOccupied(new RectInt((Vector2Int)cell, item.Footprint)))
            return false;
        entry.count--;
        Data.decorations.Add(new Decoration { itemId = item.Id, cell = cell, size = item.Footprint });
        Save();
        return true;
    }

    internal static bool IsOccupied(RectInt area) => Data.decorations.Exists(d => area.Overlaps(d.Area));

    internal static bool TryRecover(Decoration decoration)
    {
        int index = Data.decorations.FindIndex(d => d.cell == decoration.cell && d.itemId == decoration.itemId);
        if (index < 0 || !IncreaseStock(decoration.itemId, 1)) return false;
        Data.decorations.RemoveAt(index);
        Save();
        return true;
    }

    private static void Save()
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Data));
        PlayerPrefs.Save();
        Changed?.Invoke();
    }
}
