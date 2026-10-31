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

    public bool HadPreviousRecord { get; private set; }
    public bool IsNewBestRecord { get; private set; }

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

        HadPreviousRecord =
            PlayerPrefs.GetInt(HasRecordKey, 0) == 1;

        PreviousBestDistance =
            PlayerPrefs.GetFloat(BestDistanceKey, 0f);

        IsNewBestRecord =
            !HadPreviousRecord ||
            CurrentDistance > PreviousBestDistance;

        UpdatedBestDistance = IsNewBestRecord
            ? CurrentDistance
            : PreviousBestDistance;

        PlayerPrefs.SetInt(HasRecordKey, 1);

        PlayerPrefs.SetFloat(
            LastDistanceKey,
            CurrentDistance
        );

        PlayerPrefs.SetFloat(
            BestDistanceKey,
            UpdatedBestDistance
        );

        // 6분 완료 시 한 번만 실제 저장
        PlayerPrefs.Save();

        recordSavedThisSession = true;

        Debug.Log(
            $"운동 기록 저장 완료 - " +
            $"현재: {CurrentDistance:0.0}m, " +
            $"이전 최고: {PreviousBestDistance:0.0}m, " +
            $"갱신 여부: {IsNewBestRecord}"
        );
    }
}