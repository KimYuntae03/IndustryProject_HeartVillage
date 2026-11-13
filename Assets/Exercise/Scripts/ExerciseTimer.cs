using TMPro;
using UnityEngine;
using UnityEngine.Events;

// 운동 미니게임의 제한 시간을 관리하고
// 남은 시간을 분:초 형식으로 화면에 표시
public class ExerciseTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private TMP_Text remainingTimeText;

    [Header("시간 설정")]
    [SerializeField, Min(1f)]
    private float exerciseDuration = 360f;

    [Header("운동 종료 시 중지할 기능")]
    [SerializeField]
    private ExerciseDistanceTracker distanceTracker;

    [SerializeField]
    private ExerciseRouteMover routeMover;

    [Header("6분 걷기 완료")]
    [SerializeField]
    private UnityEvent onExerciseCompleted;

    private float remainingTime;
    private bool isRunning;
    private bool hasCompleted;

    public float RemainingTime => remainingTime;
    public bool IsRunning => isRunning;

    private void Awake()
    {
        ResetTimer();
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        // 종료 확인 팝업으로 Time.timeScale이 0이 되면
        // deltaTime도 0이 되어 타이머가 자동으로 멈춤
        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;

            FinishTimer();
            UpdateTimerText();

            return;
        }

        UpdateTimerText();
    }

    // 시작 버튼에서 호출
    public void StartTimer()
    {
        remainingTime = exerciseDuration;
        isRunning = true;

        // 새로운 운동을 시작하므로 완료 여부 초기화
        hasCompleted = false;

        UpdateTimerText();
    }

    // 중도 종료나 별도의 정지 기능에서 사용
    // 완료 이벤트와 기록 저장은 실행하지 않음
    public void StopTimer()
    {
        isRunning = false;
    }

    // 타이머를 처음 상태로 초기화
    public void ResetTimer()
    {
        remainingTime = exerciseDuration;
        isRunning = false;
        hasCompleted = false;

        UpdateTimerText();
    }

    // 제한 시간이 정상적으로 모두 지났을 때만 호출
    private void FinishTimer()
    {
        // 완료 처리가 중복으로 실행되는 것을 방지
        if (hasCompleted)
        {
            return;
        }

        hasCompleted = true;
        isRunning = false;

        // 최종 이동 거리를 확정하기 위해 측정부터 중지
        if (distanceTracker != null)
        {
            distanceTracker.StopMeasurement();
        }

        // 플레이어 자동 이동 중지
        if (routeMover != null)
        {
            routeMover.StopWalking();
        }

        // 기록 저장 및 이후 결과창 표시 이벤트 호출
        onExerciseCompleted?.Invoke();
    }

    private void UpdateTimerText()
    {
        if (remainingTimeText == null)
        {
            return;
        }

        int totalSeconds =
            Mathf.CeilToInt(remainingTime);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        remainingTimeText.text =
            $"남은 시간 {minutes:00}:{seconds:00}";
    }
}