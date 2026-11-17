using System;

internal static class MedicationSchedule
{
    // 모바일 알림에서 사용하는 기기 현지 시각.
    private static readonly TimeSpan[] times =
    {
        new TimeSpan(9, 0, 0),
        new TimeSpan(13, 0, 0),
        new TimeSpan(18, 0, 0)
    };

    public static int Count => times.Length;
    public static TimeSpan TimeAt(int index) => times[index];
}
