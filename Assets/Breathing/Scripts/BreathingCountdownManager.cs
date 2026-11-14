using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BreathingCountdownManager : MonoBehaviour
{
    public GameObject countdownPanel;
    public TMP_Text countdownText;
    public BreathingManager breathingManager;

    void Start()
    {
        breathingManager.enabled = false;
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
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