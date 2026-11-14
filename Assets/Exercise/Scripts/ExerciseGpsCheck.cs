using System;

public enum ExerciseDistanceCheck
{
    Inconclusive,
    Consistent,
    Mismatch
}

// 한 운동 동안만 유지한다. 원래 경로 전체를 저장하지 않는다.
internal sealed class ExerciseGpsCheck
{
    // 보상 판정용 초기값. 정확도가 낮거나 관측 공백이 큰 구간은 비교에서 제외한다.
    private const double MaxAccuracy = 8;
    private const double MaxGapSeconds = 10;
    private double lastTimestamp;
    private double lastLat, lastLon, lastAccuracy, lastStepMeters;
    private double anchorLat, anchorLon, anchorAccuracy;
    private double coveredSeconds, coveredStepMeters;
    private int intervals;
    private bool hasFix;
    public double DistanceMeters { get; private set; }

    public void BreakSegment() => hasFix = false;

    public void Sample(double timestamp, double lat, double lon, double accuracy, double stepMeters)
    {
        if (!(timestamp > lastTimestamp)) return;
        double dt = timestamp - lastTimestamp;
        lastTimestamp = timestamp;
        if (!(accuracy > 0 && accuracy <= MaxAccuracy && Math.Abs(lat) <= 90 && Math.Abs(lon) <= 180))
        {
            BreakSegment();
            return;
        }

        if (hasFix && dt <= MaxGapSeconds &&
            Meters(lastLat, lastLon, lat, lon) > 4 * dt + accuracy + lastAccuracy)
        {
            // 순간이동처럼 튄 좌표와 그곳에서 돌아오는 구간을 모두 누적하지 않는다.
            BreakSegment();
            return;
        }

        if (!hasFix || dt > MaxGapSeconds)
        {
            anchorLat = lat;
            anchorLon = lon;
            anchorAccuracy = accuracy;
        }
        else
        {
            intervals++;
            coveredSeconds += dt;
            coveredStepMeters += Math.Max(0, stepMeters - lastStepMeters);
            double movement = Meters(anchorLat, anchorLon, lat, lon);
            if (movement >= Math.Max(3, anchorAccuracy + accuracy))
            {
                DistanceMeters += movement;
                anchorLat = lat;
                anchorLon = lon;
                anchorAccuracy = accuracy;
            }
        }
        lastLat = lat;
        lastLon = lon;
        lastAccuracy = accuracy;
        lastStepMeters = stepMeters;
        hasFix = true;
    }

    public ExerciseDistanceCheck Evaluate(double stepMeters, double sessionSeconds)
    {
        if (sessionSeconds <= 0 || intervals < 3 || coveredSeconds < sessionSeconds * 0.8 ||
            coveredStepMeters < stepMeters * 0.8 || stepMeters <= 0)
            return ExerciseDistanceCheck.Inconclusive;

        // 짧은 보행이나 애매한 차이로 정상 보행을 거리 불일치로 처리하지 않는다.
        if (coveredStepMeters >= 80 && DistanceMeters < coveredStepMeters * 0.30)
            return ExerciseDistanceCheck.Mismatch;
        if (DistanceMeters < 6 || DistanceMeters < coveredStepMeters * 0.5 ||
            DistanceMeters > stepMeters * 2 + 10)
            return ExerciseDistanceCheck.Inconclusive;
        return ExerciseDistanceCheck.Consistent;
    }

    private static double Meters(double lat1, double lon1, double lat2, double lon2)
    {
        // 몇 초 간격의 보행 좌표에 쓰는 국소 평면 근사.
        const double radians = Math.PI / 180;
        double dy = (lat2 - lat1) * radians;
        double dx = Math.IEEERemainder(lon2 - lon1, 360) * radians * Math.Cos((lat1 + lat2) * 0.5 * radians);
        return 6371000 * Math.Sqrt(dx * dx + dy * dy);
    }
}
