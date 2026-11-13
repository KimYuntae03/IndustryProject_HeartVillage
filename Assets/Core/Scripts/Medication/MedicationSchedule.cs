using System;

internal static class MedicationSchedule
{
    // 가입 정보가 연결되기 전까지 사용하는 기기 현지 시각.
    private static readonly (string key, TimeSpan time)[] slots =
    {
        ("HeartVillage.Medication.Slot0", new TimeSpan(9, 0, 0)),
        ("HeartVillage.Medication.Slot1", new TimeSpan(13, 0, 0)),
        ("HeartVillage.Medication.Slot2", new TimeSpan(18, 0, 0))
    };

    public static int Count => slots.Length;
    public static TimeSpan TimeAt(int index) => slots[index].time;
    public static int FindSlot(string recordKey) => Array.FindIndex(slots, slot => slot.key == recordKey);

    public static bool CanRecord(int index, DateTime localTime)
    {
        var elapsed = localTime.TimeOfDay - TimeAt(index);
        return elapsed >= TimeSpan.Zero && elapsed < TimeSpan.FromMinutes(30);
    }
}
