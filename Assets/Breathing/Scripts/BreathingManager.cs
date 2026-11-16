using UnityEngine;
using Debug = UnityEngine.Debug;

public class BreathingManager : MonoBehaviour
{
    private AudioClip micClip;
    private string micDevice;
    private float currentAmplitude;

    public float minAmplitude = 0.01f;
    public float maxAmplitude = 0.4f;

    [Header("미션 설정")]
    public float successDuration = 5f;
    public float totalGameTime = 30f;

    private float breathDuration = 0f;
    private bool isBreathing = false;
    private float bestDuration = 0f;
    private float sessionBest = 0f;
    private bool missionSuccess = false;
    private float gameTimer = 0f;
    private bool gameOver = false;

    void Awake()
    {
        enabled = false;
        gameTimer = totalGameTime;
        bestDuration = DataManager.Instance.GetBestScore("breathing");
    }

    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            micDevice = Microphone.devices[0];
            micClip = Microphone.Start(micDevice, true, 10, 44100);
        }
        else
        {
            Debug.Log("마이크 없음!");
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

        currentAmplitude = GetMicAmplitude();

        if (currentAmplitude >= minAmplitude && currentAmplitude <= maxAmplitude)
        {
            isBreathing = true;
            breathDuration += Time.deltaTime;

            if (breathDuration >= successDuration)
            {
                missionSuccess = true;
                UpdateBest();
            }
        }
        else if (isBreathing)
        {
            UpdateBest();
            breathDuration = 0f;
            isBreathing = false;
        }
    }

    void UpdateBest()
    {
        if (breathDuration > sessionBest) sessionBest = breathDuration;
        if (breathDuration > bestDuration) bestDuration = breathDuration;
    }

    float GetMicAmplitude()
    {
        if (micClip == null) return 0f;

        float[] samples = new float[1024];
        int position = Microphone.GetPosition(micDevice);
        if (position < 1024) return 0f;

        micClip.GetData(samples, position - 1024);

        float maxVal = 0f;
        foreach (float s in samples)
        {
            float abs = Mathf.Abs(s);
            if (abs > maxVal) maxVal = abs;
        }
        return maxVal;
    }

    public float GetCurrentAmplitude() => currentAmplitude;
    public float GetBreathDuration() => breathDuration;
    public float GetBestDuration() => bestDuration;
    public float GetSessionBest() => sessionBest;
    public float GetPlaySeconds() => totalGameTime - gameTimer;
    public bool IsInGreenZone() => isBreathing;
    public bool IsMissionSuccess() => missionSuccess;
    public float GetSuccessDuration() => successDuration;
    public float GetGameTimer() => gameTimer;
    public bool IsGameOver() => gameOver;
}