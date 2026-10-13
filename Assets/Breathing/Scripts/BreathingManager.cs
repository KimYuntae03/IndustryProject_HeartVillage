using System.Diagnostics;
using UnityEngine;

public class BreathingManager : MonoBehaviour
{
    private AudioClip micClip;
    private string micDevice;
    private float currentAmplitude;

    public float minAmplitude = 0.1f;
    public float maxAmplitude = 0.4f;

    private float breathDuration = 0f;
    private bool isBreathing = false;
    private float bestDuration = 0f;

    void Start()
    {
        bestDuration = PlayerPrefs.GetFloat("BestBreathDuration", 0f);

        if (Microphone.devices.Length > 0)
        {
            micDevice = Microphone.devices[0];
            micClip = Microphone.Start(micDevice, true, 10, 44100);
            UnityEngine.Debug.Log("마이크 시작: " + micDevice);
            UnityEngine.Debug.Log("micClip null? " + (micClip == null));
        }
        else
        {
            UnityEngine.Debug.Log("마이크 없음!");
        }
    }

    void Update()
    {
        currentAmplitude = GetMicAmplitude();

        if (currentAmplitude >= minAmplitude && currentAmplitude <= maxAmplitude)
        {
            isBreathing = true;
            breathDuration += Time.deltaTime;
        }
        else
        {
            if (isBreathing)
            {
                if (breathDuration > bestDuration)
                {
                    bestDuration = breathDuration;
                    PlayerPrefs.SetFloat("BestBreathDuration", bestDuration);
                    PlayerPrefs.Save();
                    UnityEngine.Debug.Log("새 기록: " + bestDuration + "초");
                }
                breathDuration = 0f;
                isBreathing = false;
            }
        }
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
    public bool IsInGreenZone() => isBreathing;
}