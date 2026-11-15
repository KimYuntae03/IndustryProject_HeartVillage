using UnityEngine;
using TMPro;

public class BreathingUI : MonoBehaviour
{
    public BreathingManager breathingManager;
    public TextMeshProUGUI durationText;
    public TextMeshProUGUI bestText;
    public TextMeshProUGUI goalText;
    public TextMeshProUGUI timerText;

    void Update()
    {
        if (breathingManager == null) return;

        float current = breathingManager.GetBreathDuration();
        float best = breathingManager.GetBestDuration();
        float goal = breathingManager.GetSuccessDuration();
        float remaining = Mathf.Max(0f, goal - current);
        float gameTimer = breathingManager.GetGameTimer();

        durationText.text = $"현재: {current:F1}초";
        bestText.text = $"최고 기록: {best:F1}초";
        goalText.text = breathingManager.IsMissionSuccess()
            ? "목표 달성!"
            : $"목표까지: {remaining:F1}초";

        if (timerText != null)
        {
            timerText.text = $"남은 시간: {Mathf.CeilToInt(gameTimer)}초";
            timerText.color = gameTimer <= 10f
                ? new Color(1f, 0.45f, 0.4f)
                : new Color(1f, 0.914f, 0.722f);
        }
    }
}