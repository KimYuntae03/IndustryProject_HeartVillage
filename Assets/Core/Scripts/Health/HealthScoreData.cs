using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

// One record per local calendar day and category. Rewards remain owned by each game.
public static class HealthScoreData
{
    private const string SaveKey = "HeartVillage.HealthScores.V1";
    private const string DoseKeyPrefix = "HeartVillage.Medication.Slot";

    [Serializable]
    private sealed class SaveData
    {
        public bool importedLegacy;
        public List<HealthDailyRecord> days = new();
    }

    // Read-only adapters for the pre-existing save formats.
    [Serializable] private sealed class LegacyGames { public List<LegacyPlay> playLogs; }
    [Serializable] private sealed class LegacyPlay
    {
        public string gameId;
        public string playedAt;
        public bool success;
        public bool hasHealthAccuracy;
        public float healthAccuracy;
    }
    [Serializable] private sealed class LegacyVillage { public List<LegacyDiet> dietRewards; }
    [Serializable] private sealed class LegacyDiet { public string date; public long sodiumMg; }

    private static SaveData data;
    public static event Action Changed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession() { data = null; Changed = null; }

    private static void EnsureLoaded()
    {
        if (data != null) return;
        data = Read<SaveData>(SaveKey) ?? new SaveData();
        data.days ??= new List<HealthDailyRecord>();
        if (data.importedLegacy) return;

        var games = Read<LegacyGames>("HV_SaveData");
        if (games?.playLogs != null)
            foreach (var log in games.playLogs)
            {
                if (log == null || log.gameId != "breathing" || string.IsNullOrEmpty(log.playedAt) ||
                    log.playedAt.Length < 10 || !TryDay(log.playedAt.Substring(0, 10), out var day)) continue;
                // Old logs did not store their target duration. Success proves 100%; don't guess
                // a failed session's ratio using today's potentially different target duration.
                double? accuracy = log.hasHealthAccuracy ? log.healthAccuracy : log.success ? 100d : (double?)null;
                Put(HealthCategory.Breathing, day, accuracy, true);
            }

        var village = Read<LegacyVillage>("HeartVillage.Player.V1");
        if (village?.dietRewards != null)
            foreach (var receipt in village.dietRewards)
                if (receipt != null && TryDay(receipt.date, out var day))
                    Put(HealthCategory.Diet, day, HealthScoreRules.DietAccuracy(receipt.sodiumMg), false);

        // The old medication slots retain only their most recent date, not complete histories.
        for (int i = 0; i < 3; i++)
        {
            string oldDay = PlayerPrefs.GetInt(DoseKeyPrefix + i, 0).ToString(CultureInfo.InvariantCulture);
            if (DateTime.TryParseExact(oldDay, "yyyyMMdd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var day)) Put(HealthCategory.Medication, day, null, true);
        }
        // Old exercise preferences have no completion date: retain the distance baseline in its
        // original store but never invent participation days from it.
        data.importedLegacy = true;
        Save(false);
    }

    private static T Read<T>(string key) where T : class
    {
        string json = PlayerPrefs.GetString(key, "");
        if (string.IsNullOrEmpty(json)) return null;
        try { return JsonUtility.FromJson<T>(json); }
        catch (ArgumentException) { Debug.LogWarning("점수 기록을 읽지 못했습니다: " + key); return null; }
    }

    private static bool TryDay(string value, out DateTime date) => DateTime.TryParseExact(value,
        "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    public static void RecordBest(HealthCategory category, DateTime date, double? accuracy) =>
        Record(category, date, accuracy, true);

    public static void RecordDaily(HealthCategory category, DateTime date, double? accuracy) =>
        Record(category, date, accuracy, false);

    private static void Record(HealthCategory category, DateTime date, double? accuracy, bool keepBest)
    {
        EnsureLoaded();
        if (Put(category, date, accuracy, keepBest)) Save(true);
    }

    private static bool Put(HealthCategory category, DateTime date, double? accuracy, bool keepBest)
    {
        if (accuracy.HasValue && !HealthScoreRules.IsFinite(accuracy.Value)) return false;
        string day = HealthScoreRules.Day(date);
        var entry = data.days.Find(r => r != null && r.category == category && r.date == day);
        bool added = entry == null;
        if (added)
        {
            entry = new HealthDailyRecord { category = category, date = day };
            data.days.Add(entry);
        }
        double? next = accuracy.HasValue ? HealthScoreRules.ClampAccuracy(accuracy.Value) : (double?)null;
        if (keepBest && entry.hasAccuracy) next = Math.Max(entry.accuracy, next ?? 0d);
        bool changed = added || entry.hasAccuracy != next.HasValue || (next.HasValue && entry.accuracy != next.Value);
        entry.hasAccuracy = next.HasValue;
        entry.accuracy = next ?? 0d;
        return changed;
    }

    public static void RefreshMedication(DateTime date)
    {
        EnsureLoaded();
        int scheduled = LocalPlayerProfile.Load()?.doseMinutes?.Length ?? 0;
        int dayKey = date.Year * 10000 + date.Month * 100 + date.Day;
        int taken = 0;
        for (int i = 0; i < scheduled; i++)
            if (PlayerPrefs.GetInt(DoseKeyPrefix + i, 0) == dayKey) taken++;
        // No dose checked means 0 accuracy, but does not count as a participation day.
        if (taken > 0) RecordDaily(HealthCategory.Medication, date,
            HealthScoreRules.MedicationAccuracy(taken, scheduled));
    }

    public static HealthScoreSnapshot GetSnapshot(DateTime date)
    {
        EnsureLoaded();
        var categories = new HealthCategoryScore[4];
        int scheduled = LocalPlayerProfile.Load()?.doseMinutes?.Length ?? 0;
        for (int i = 0; i < categories.Length; i++)
            categories[i] = HealthScoreRules.Calculate((HealthCategory)i, date.Date, data.days,
                i == (int)HealthCategory.Medication ? HealthScoreRules.MedicationAccuracy(0, scheduled) : null);
        return new HealthScoreSnapshot(date, categories);
    }

    public static void ClearCategory(HealthCategory category)
    {
        EnsureLoaded();
        if (data.days.RemoveAll(r => r != null && r.category == category) > 0) Save(true);
    }

    private static void Save(bool notify)
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
        if (notify) Changed?.Invoke();
    }
}
