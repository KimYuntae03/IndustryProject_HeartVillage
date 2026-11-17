using UnityEngine;
using TMPro;
using System.Collections;

public class BreathingCountdownManager : MonoBehaviour
{
    public GameObject countdownPanel;
    public TMP_Text countdownText;
    public BreathingManager breathingManager;

    public void StartCountdown()
    {
        breathingManager.BeginCalibration();
        StartCoroutine(CountdownCoroutine());
    }

    IEnumerator CountdownCoroutine()
    {
        countdownPanel.SetActive(true);

        string[] steps = { "3", "2", "1", "Ω√¿€!" };
        foreach (string step in steps)
        {
            countdownText.text = step;
            yield return new WaitForSeconds(1f);
        }

        countdownPanel.SetActive(false);
        breathingManager.enabled = true;
    }
}