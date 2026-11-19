using UnityEngine;

public class ExerciseRecordManager : MonoBehaviour
{
    private const string HasRecordKey =
        "Exercise.HasRecord";

    private const string LastDistanceKey =
        "Exercise.LastDistanceMeters";

    private const string BestDistanceKey =
        "Exercise.BestDistanceMeters";

    [Header("거리 측정")]
    [SerializeField]
    private ExerciseDistanceTracker distanceTracker;

    private bool recordSavedThisSession;

    public int AwardedHeartPoints { get; private set; }
    public ExerciseDistanceCheck DistanceCheck { get; private set; }
    public bool HadPreviousRecord { get; private set; }
    public bool IsNewBestRecord { get; private set; }
    public bool MeetsRewardDistanceRequirement { get; private set; }

    public float PreviousBestDistance { get; private set; }
    public float CurrentDistance { get; private set; }
    public float UpdatedBestDistance { get; private set; }

    // ExerciseTimer의 정상 완료 이벤트에서만 호출
    public void SaveCompletedRecord()
    {
        // 완료 이벤트가 중복 호출돼도 한 번만 저장
        if (recordSavedThisSession)
        {
            return;
        }

        if (distanceTracker == null)
        {
            Debug.LogError(
                "ExerciseDistanceTracker가 연결되지 않았습니다."
            );

            return;
        }

        CurrentDistance =
            Mathf.Max(0f, distanceTracker.DistanceMeters);
        if (float.IsNaN(CurrentDistance) || float.IsInfinity(CurrentDistance)) CurrentDistance = 0f;

        PreviousBestDistance =
            PlayerPrefs.GetFloat(BestDistanceKey, 0f);
        if (float.IsNaN(PreviousBestDistance) || float.IsInfinity(PreviousBestDistance))
            PreviousBestDistance = 0f;

        // A zero-distance record from an older save does not establish a walking baseline.
        HadPreviousRecord = PlayerPrefs.GetInt(HasRecordKey, 0) == 1 &&
            PlayerPrefs.HasKey(BestDistanceKey) && PreviousBestDistance > 0f;
        MeetsRewardDistanceRequirement = CurrentDistance > 0f &&
            (!HadPreviousRecord || CurrentDistance >= PreviousBestDistance * 0.5f);

        recordSavedThisSession = true;
        DistanceCheck = distanceTracker.DistanceCheck;
        UpdatedBestDistance = PreviousBestDistance;
        if (DistanceCheck == ExerciseDistanceCheck.Mismatch) return;

        var completedAt = System.DateTime.Now;
        int reward = MeetsRewardDistanceRequirement ? 10 : 0;
        if (DistanceCheck == ExerciseDistanceCheck.Consistent && CurrentDistance > 0f)
        {
            IsNewBestRecord = !HadPreviousRecord || CurrentDistance > PreviousBestDistance;
            if (IsNewBestRecord)
            {
                UpdatedBestDistance = CurrentDistance;
                if (HadPreviousRecord) reward = 15;
            }
            PlayerPrefs.SetInt(HasRecordKey, 1);
            PlayerPrefs.SetFloat(LastDistanceKey, CurrentDistance);
            PlayerPrefs.SetFloat(BestDistanceKey, UpdatedBestDistance);
        }
        // Only the same GPS-confirmed distances accepted for records may establish accuracy.
        // Inconclusive completion still counts as a recorded day, with accuracy unassessed.
        HealthScoreData.RecordBest(HealthCategory.Exercise, completedAt,
            DistanceCheck == ExerciseDistanceCheck.Consistent && HadPreviousRecord
                ? HealthScoreRules.ExerciseAccuracy(CurrentDistance, PreviousBestDistance) : null);
        if (reward == 0)
        {
            // Keep the completed session's score, but do not pay a distance-ineligible reward.
            PlayerPrefs.Save();
        }
        else if (VillagePlayerData.AddHeartPoints(reward))
        {
            // 공용 지갑이 바뀐 기록 키와 하트 잔액을 PlayerPrefs에 함께 저장한다.
            AwardedHeartPoints = reward;
        }
        else
        {
            PlayerPrefs.Save();
            Debug.LogWarning("하트 포인트 보유 한도를 초과해 운동 보상을 지급하지 못했습니다.");
        }

        Debug.Log(
            $"운동 기록 저장 완료 - " +
            $"현재: {CurrentDistance:0.0}m, " +
            $"이전 최고: {PreviousBestDistance:0.0}m, " +
            $"갱신 여부: {IsNewBestRecord}"
        );
    }
}
