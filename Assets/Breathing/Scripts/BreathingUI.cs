using UnityEngine;
using TMPro;

public class BreathingUI : MonoBehaviour
{
    public BreathingManager breathingManager;
    public TextMeshProUGUI durationText;
    public TextMeshProUGUI bestText;
    public TextMeshProUGUI goalText;

    void Update()
    {
        if (breathingManager == null) return;

        float current = breathingManager.GetBreathDuration();
        float best = breathingManager.GetBestDuration();
        float goal = breathingManager.GetSuccessDuration();
        float remaining = Mathf.Max(0f, goal - current);

        durationText.text = $"현재: {current:F1}초";
        bestText.text = $"최고 기록: {best:F1}초";
        goalText.text = breathingManager.IsMissionSuccess()
            ? "목표 달성!"
            : $"목표까지: {remaining:F1}초";
    }
}