using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class ExerciseDistanceTracker : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text distanceText;

    [Header("거리 계산")]
    [SerializeField, Min(0.1f)]
    private float strideLengthMeters = 0.7f;

    private StepCounter stepCounter;
    private Coroutine prepareCoroutine;

    // 미니게임 측정을 시작한 순간의 누적 걸음 수
    private int startStepCount;

    private bool isMeasuring;
    private bool sensorReady;

#if UNITY_ANDROID && !UNITY_EDITOR
    private const string ActivityPermission =
        "android.permission.ACTIVITY_RECOGNITION";

    private PermissionCallbacks permissionCallbacks;
#endif

    public int SessionStepCount { get; private set; }
    public float DistanceMeters { get; private set; }
    public bool IsMeasuring => isMeasuring;
    public bool SensorReady => sensorReady;

    private void OnEnable()
    {
        isMeasuring = false;
        sensorReady = false;

        SessionStepCount = 0;
        DistanceMeters = 0f;

        RequestPermissionAndPrepareSensor();
    }

    private void Update()
    {
        if (!isMeasuring || !sensorReady || stepCounter == null)
        {
            return;
        }

        // Android 기기가 기록한 현재 누적 걸음 수
        int currentStepCount =
            stepCounter.stepCounter.ReadValue();

        // 미니게임 시작 이후 걸은 걸음 수만 계산
        SessionStepCount = Mathf.Max(
            0,
            currentStepCount - startStepCount
        );

        // 걸음 수 × 평균 보폭으로 실제 이동 거리 계산
        DistanceMeters =
            SessionStepCount * strideLengthMeters;

        UpdateDistanceText();
    }

    private void RequestPermissionAndPrepareSensor()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Android 10 이상에서 걸음 수 센서를 사용하기 위한 권한
        if (!Permission.HasUserAuthorizedPermission(ActivityPermission))
        {
            distanceText.text = "신체 활동 권한이 필요합니다.";

            permissionCallbacks = new PermissionCallbacks();
            permissionCallbacks.PermissionGranted += OnPermissionGranted;
            permissionCallbacks.PermissionDenied += OnPermissionDenied;
            permissionCallbacks.PermissionDeniedAndDontAskAgain +=
                OnPermissionDenied;

            Permission.RequestUserPermission(
                ActivityPermission,
                permissionCallbacks
            );

            return;
        }
#endif

        BeginPrepareStepCounter();
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void OnPermissionGranted(string permissionName)
    {
        BeginPrepareStepCounter();
    }

    private void OnPermissionDenied(string permissionName)
    {
        sensorReady = false;
        distanceText.text = "신체 활동 권한이 거부되었습니다.";
    }
#endif

    private void BeginPrepareStepCounter()
    {
        if (prepareCoroutine != null)
        {
            StopCoroutine(prepareCoroutine);
        }

        prepareCoroutine = StartCoroutine(
            PrepareStepCounterCoroutine()
        );
    }

    private IEnumerator PrepareStepCounterCoroutine()
    {
        distanceText.text = "걸음 센서 준비 중...";

        // 기기 센서가 Input System에 등록될 때까지 잠시 대기
        for (int i = 0; i < 120; i++)
        {
            stepCounter = StepCounter.current;

            if (stepCounter != null)
            {
                InputSystem.EnableDevice(stepCounter);

                sensorReady = true;
                prepareCoroutine = null;

                UpdateDistanceText();
                yield break;
            }

            yield return null;
        }

        sensorReady = false;
        prepareCoroutine = null;
        distanceText.text = "걸음 센서를 사용할 수 없습니다.";
    }

    public void StartMeasurement()
    {
        if (!sensorReady || stepCounter == null)
        {
            distanceText.text = "걸음 센서 준비가 필요합니다.";
            BeginPrepareStepCounter();
            return;
        }

        // 시작 버튼을 누른 순간의 누적 걸음 수를 기준값으로 저장
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
        isMeasuring = false;

        if (prepareCoroutine != null)
        {
            StopCoroutine(prepareCoroutine);
            prepareCoroutine = null;
        }

        if (stepCounter != null)
        {
            InputSystem.DisableDevice(stepCounter);
        }
    }
}