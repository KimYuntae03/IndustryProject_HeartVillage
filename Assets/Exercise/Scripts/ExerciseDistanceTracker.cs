using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

//스마트폰의 걸음 수 센서를 이용해 미니게임 시작 후 이동한 예상 거리를 계산하고 표시
public class ExerciseDistanceTracker : MonoBehaviour
{
    [Header("UI")] //UI에 업데이트
    [SerializeField] private TMP_Text distanceText;

    [Header("거리 계산")]//임시로 0.7m로 가정.
    [SerializeField, Min(0.1f)]
    private float stepLengthMeters = 0.7f;

     // 스마트폰에서 제공하는 걸음 수 센서
    private StepCounter stepCounter;

    // 기기 재부팅 이후의 누적 걸음 수 중
    // 미니게임을 시작한 시점의 값을 저장
    private int startStepCount;

    private bool isMeasuring;

    public int SessionStepCount { get; private set; } // 미니게임 시작 이후 측정된 걸음 수
    public float DistanceMeters { get; private set; } // 걸음 수와 한 걸음 길이로 계산한 이동 거리

    private void OnEnable()
    {
        PrepareStepCounter();
        UpdateDistanceText();
    }

    private void Update()
    {
        // 측정 시작 전이거나 센서가 없다면 거리 계산을 하지 않음
        if (!isMeasuring || stepCounter == null)
        {
            return;
        }

        // 현재 기기의 전체 누적 걸음 수
        int currentStepCount =
            stepCounter.stepCounter.ReadValue();

        // 미니게임 시작 이후에 걸은 걸음 수만 계산
        SessionStepCount = Mathf.Max(
            0,
            currentStepCount - startStepCount
        );

        // 걸음 수에 평균 보폭을 곱해 이동 거리를 계산
        DistanceMeters =
            SessionStepCount * stepLengthMeters;

        UpdateDistanceText();
    }

    private bool PrepareStepCounter()
    {
        if (stepCounter == null)
        {
            stepCounter = StepCounter.current;
        }

        if (stepCounter == null)
        {
            if (distanceText != null)
            {
                distanceText.text = "거리 측정 불가";
            }

            return false;
        }

        // 스마트폰의 걸음 수 센서를 활성화
        InputSystem.EnableDevice(stepCounter);
        return true;
    }

    public void StartMeasurement()
    {
        if (!PrepareStepCounter())
        {
            return;
        }

        // 시작 버튼을 누른 순간의 누적 걸음 수를 기준점으로 저장
        startStepCount =
            stepCounter.stepCounter.ReadValue();

        SessionStepCount = 0;
        DistanceMeters = 0f;
        isMeasuring = true;

        UpdateDistanceText();
    }

    public void StopMeasurement()
    {
        isMeasuring = false;
    }

    private void UpdateDistanceText()
    {
        if (distanceText == null)
        {
            return;
        }

        distanceText.text =
            $"거리 {DistanceMeters:0.0} m";
    }

    private void OnDisable()
    {
        if (stepCounter != null)
        {
            // 운동 씬을 벗어나면 센서 사용 중지
            InputSystem.DisableDevice(stepCounter);
        }
    }
}