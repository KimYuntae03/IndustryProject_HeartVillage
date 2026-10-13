using TMPro;
using UnityEngine;

// 운동 미니게임의 6분 제한 시간을 관리하고
// 남은 시간을 분:초 형식으로 화면에 표시
public class ExerciseTimer : MonoBehaviour
{
    [Header("UI")]

    // 남은 시간을 표시할 TextMeshPro 텍스트
    [SerializeField] private TMP_Text remainingTimeText;

    [Header("시간 설정")]

    // 운동 제한 시간으로 360초는 6분
    [SerializeField, Min(1f)]
    private float exerciseDuration = 360f;

    [Header("운동 종료 시 중지할 기능")]

    // 시간이 끝나면 거리 측정을 중지
    [SerializeField] private ExerciseDistanceTracker distanceTracker;

    // 시간이 끝나면 플레이어 자동 이동을 중지
    [SerializeField] private ExerciseRouteMover routeMover;

    private float remainingTime;
    private bool isRunning;

    public float RemainingTime => remainingTime;
    public bool IsRunning => isRunning;

    private void Awake()
    {
        // 게임 시작 전에는 6분으로 표시하고 대기
        ResetTimer();
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        // Time.timeScale이 0이면 deltaTime도 0이 되므로
        // 종료 확인 팝업이 열린 동안 타이머가 자동으로 멈춤
        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            FinishTimer();
        }

        UpdateTimerText();
    }

    // 시작 버튼을 눌렀을 때 6분 카운트다운 시작
    public void StartTimer()
    {
        remainingTime = exerciseDuration;
        isRunning = true;

        UpdateTimerText();
    }

    // 필요할 때 타이머를 수동으로 중지
    public void StopTimer()
    {
        isRunning = false;
    }

    // 타이머를 처음 설정한 시간으로 초기화
    public void ResetTimer()
    {
        remainingTime = exerciseDuration;
        isRunning = false;

        UpdateTimerText();
    }

    // 제한 시간이 모두 끝났을 때 운동 관련 기능 중지
    private void FinishTimer()
    {
        isRunning = false;

        if (distanceTracker != null)
        {
            distanceTracker.StopMeasurement();
        }

        if (routeMover != null)
        {
            routeMover.StopWalking();
        }

        // 이후 결과 팝업을 여는 기능을 이곳에 추가하면 됨
    }

    // 남은 시간을 06:00 형태로 변환해 표시
    private void UpdateTimerText()
    {
        if (remainingTimeText == null)
        {
            return;
        }

        int totalSeconds = Mathf.CeilToInt(remainingTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        remainingTimeText.text =
            $"남은 시간 {minutes:00}:{seconds:00}";
    }
}