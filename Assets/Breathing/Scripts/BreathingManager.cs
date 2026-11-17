using System.Collections;
using UnityEngine;

public class BreathingManager : MonoBehaviour
{
    [Header("미션 설정")]
    public float successDuration = 5f;
    public float totalGameTime = 30f;

    [Header("인식 감도")]
    public float breathMargin = 0.008f;
    public float greenMargin = 0.025f;
    public float maxAmplitude = 0.95f;
    public float smoothing = 12f;
    public float dropGraceTime = 0.4f;

    [Header("소음 보정")]
    public float defaultNoiseFloor = 0.005f;
    public float maxNoiseFloor = 0.05f;
    public float noiseMultiplier = 1.5f;
    public bool showDebugLog = false;

    private const int SampleWindow = 1024;
    private readonly float[] samples = new float[SampleWindow];

    private AudioClip micClip;
    private string micDevice;
    private bool micStarted = false;
    private float lastRawLevel = 0f;
    private float currentAmplitude = 0f;
    private float noiseFloor;
    private float breathLevel = 0f;

    private float breathDuration = 0f;
    private bool isBreathing = false;
    private float dropTimer = 0f;
    private float bestDuration = 0f;
    private float sessionBest = 0f;
    private bool missionSuccess = false;
    private float gameTimer = 0f;
    private bool gameOver = false;
    private float debugTimer = 0f;

    void Awake()
    {
        enabled = false;
        gameTimer = totalGameTime;
        noiseFloor = defaultNoiseFloor;
        bestDuration = DataManager.Instance.GetBestScore("breathing");
    }

    void Start()
    {
        StartMic();
    }

    void StartMic()
    {
        if (micStarted) return;
        micStarted = true;

        if (Microphone.devices.Length > 0)
        {
            micDevice = Microphone.devices[0];
            micClip = Microphone.Start(micDevice, true, 10, 44100);
        }
        else
        {
            UnityEngine.Debug.Log("마이크 없음!");
        }
    }

    public void BeginCalibration()
    {
        StartMic();
        StartCoroutine(Calibrate());
    }

    IEnumerator Calibrate()
    {
        yield return new WaitForSecondsRealtime(0.4f);

        float sum = 0f;
        int count = 0;
        while (!enabled)
        {
            sum += GetMicLevel();
            count++;
            noiseFloor = Mathf.Clamp(sum / count, 0.0005f, maxNoiseFloor);
            yield return null;
        }
    }

    void Update()
    {
        if (gameOver || missionSuccess) return;

        gameTimer -= Time.deltaTime;
        if (gameTimer <= 0f)
        {
            gameTimer = 0f;
            gameOver = true;
            UpdateBest();
            return;
        }

        float raw = GetMicLevel();
        currentAmplitude = Mathf.Lerp(currentAmplitude, raw, 1f - Mathf.Exp(-smoothing * Time.deltaTime));

        float floor = noiseFloor * noiseMultiplier;
        float breathStart = floor + breathMargin;
        float greenStart = floor + greenMargin;

        breathLevel = Mathf.InverseLerp(breathStart, greenStart, currentAmplitude);
        bool inZone = currentAmplitude >= greenStart && currentAmplitude <= maxAmplitude;

        if (inZone)
        {
            isBreathing = true;
            dropTimer = 0f;
            breathDuration += Time.deltaTime;

            if (breathDuration >= successDuration)
            {
                breathDuration = successDuration;
                missionSuccess = true;
                UpdateBest();
            }
        }
        else if (isBreathing)
        {
            dropTimer += Time.deltaTime;
            if (dropTimer >= dropGraceTime)
            {
                UpdateBest();
                breathDuration = 0f;
                isBreathing = false;
            }
        }

        if (showDebugLog)
        {
            debugTimer += Time.deltaTime;
            if (debugTimer >= 0.5f)
            {
                debugTimer = 0f;
                UnityEngine.Debug.Log($"소리={currentAmplitude:F4} 소음기준={noiseFloor:F4} 부풀기시작={breathStart:F4} 초록시작={greenStart:F4}");
            }
        }
    }

    void UpdateBest()
    {
        if (breathDuration > sessionBest) sessionBest = breathDuration;
        if (breathDuration > bestDuration) bestDuration = breathDuration;
    }

    float GetMicLevel()
    {
        if (micClip == null) return 0f;

        int position = Microphone.GetPosition(micDevice);
        if (position < SampleWindow) return lastRawLevel;

        micClip.GetData(samples, position - SampleWindow);

        float sum = 0f;
        for (int i = 0; i < SampleWindow; i++)
            sum += samples[i] * samples[i];

        lastRawLevel = Mathf.Sqrt(sum / SampleWindow);
        return lastRawLevel;
    }

    public float GetCurrentAmplitude() => currentAmplitude;
    public float GetBreathLevel() => breathLevel;
    public float GetNoiseFloor() => noiseFloor;
    public float GetBreathDuration() => breathDuration;
    public float GetHoldRemaining() => Mathf.Max(0f, successDuration - breathDuration);
    public float GetBestDuration() => bestDuration;
    public float GetSessionBest() => sessionBest;
    public float GetPlaySeconds() => totalGameTime - gameTimer;
    public bool IsInGreenZone() => isBreathing;
    public bool IsMissionSuccess() => missionSuccess;
    public float GetSuccessDuration() => successDuration;
    public float GetGameTimer() => gameTimer;
    public bool IsGameOver() => gameOver;
}