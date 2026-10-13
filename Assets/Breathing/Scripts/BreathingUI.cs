using UnityEngine;
using TMPro;

public class BreathingUI : MonoBehaviour
{
    public BreathingManager breathingManager;
    public TextMeshProUGUI durationText;
    public TextMeshProUGUI bestText;

    void Update()
    {
        if (breathingManager == null) return;

        durationText.text = "Current: " + breathingManager.GetBreathDuration().ToString("F1") + "s";
        bestText.text = "Best: " + breathingManager.GetBestDuration().ToString("F1") + "s";
    }
}
