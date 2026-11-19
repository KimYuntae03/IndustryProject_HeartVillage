using System;
using System.Collections.Generic;
using System.Globalization;

public enum HealthCategory { Breathing, Exercise, Medication, Diet }
public enum HealthWeather { Sunny, Clear, Cloudy, Rainy }

[Serializable]
public sealed class HealthDailyRecord
{
    public HealthCategory category;
    public string date;
    public bool hasAccuracy;
    public double accuracy;
}

public readonly struct HealthCategoryScore
{
    public readonly double? Accuracy;
    public readonly int RecordedDays;
    public readonly int StreakDays;
    public double Completion => RecordedDays / 7d * 100d;
    public double Streak => StreakDays / 7d * 100d;
    public double Score => (Accuracy ?? 0d) * 0.4d + Completion * 0.35d + Streak * 0.25d;

    public HealthCategoryScore(double? accuracy, int recordedDays, int streakDays)
    {
        Accuracy = accuracy;
        RecordedDays = recordedDays;
        StreakDays = streakDays;
    }
}

public sealed class HealthScoreSnapshot
{
    public readonly DateTime Date;
    public readonly HealthCategoryScore[] Categories;
    public double HealthIndex => (Categories[0].Score + Categories[1].Score +
        Categories[2].Score + Categories[3].Score) / 4d;
    public HealthWeather Weather => HealthScoreRules.Weather(HealthIndex);

    public HealthScoreSnapshot(DateTime date, HealthCategoryScore[] categories)
    {
        Date = date.Date;
        Categories = categories;
    }
}

// Pure calculation: no rewards, PlayerPrefs, or intermediate rounding.
public static class HealthScoreRules
{
    public static string Day(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    public static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    public static double ClampAccuracy(double value) => Math.Max(0d, Math.Min(100d, value));

    public static double? BreathingAccuracy(double heldSeconds, double targetSeconds) =>
        !IsFinite(heldSeconds) || !IsFinite(targetSeconds) || heldSeconds < 0d || targetSeconds <= 0d
            ? (double?)null : ClampAccuracy(heldSeconds / targetSeconds * 100d);

    // The first valid session establishes a baseline, including a zero-distance baseline.
    public static double? ExerciseAccuracy(double distance, double previousBest) =>
        !IsFinite(distance) || !IsFinite(previousBest) || distance < 0d || previousBest <= 0d
            ? (double?)null : ClampAccuracy(distance / previousBest * 100d);

    public static double? DietAccuracy(long sodiumMg) =>
        sodiumMg < 0 ? (double?)null : sodiumMg <= 1800 ? 100d : sodiumMg <= 2000 ? 70d : 0d;

    // Three daily doses deliberately produce 0, 33, 66, 100 (truncate each ratio).
    public static double? MedicationAccuracy(int taken, int scheduled) => scheduled <= 0
        ? (double?)null : Math.Floor(Math.Max(0, Math.Min(taken, scheduled)) * 100d / scheduled);

    public static HealthWeather Weather(double score) => score >= 90d ? HealthWeather.Sunny :
        score >= 70d ? HealthWeather.Clear : score >= 50d ? HealthWeather.Cloudy : HealthWeather.Rainy;

    public static HealthCategoryScore Calculate(HealthCategory category, DateTime today,
        IReadOnlyList<HealthDailyRecord> records, double? unrecordedAccuracy = null)
    {
        string day = Day(today);
        var days = new HashSet<string>();
        double? accuracy = unrecordedAccuracy;
        foreach (var record in records)
        {
            if (record == null || record.category != category ||
                !DateTime.TryParseExact(record.date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var recordDate) || recordDate.Date > today.Date) continue;
            days.Add(record.date);
            if (record.date == day && record.hasAccuracy && IsFinite(record.accuracy))
                accuracy = Math.Max(accuracy ?? 0d, ClampAccuracy(record.accuracy));
        }

        int completed = 0;
        for (int i = 0; i < 7; i++)
            if (days.Contains(Day(today.AddDays(-i)))) completed++;

        // Today's opportunity stays open until midnight; retain yesterday's streak meanwhile.
        var cursor = days.Contains(day) ? today.Date : today.Date.AddDays(-1);
        int streak = 0;
        while (streak < 7 && days.Contains(Day(cursor)))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }
        return new HealthCategoryScore(accuracy, completed, streak);
    }
}
