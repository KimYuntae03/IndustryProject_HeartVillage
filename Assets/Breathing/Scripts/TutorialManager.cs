using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public GameObject tutorialUI;
    public GameObject countdownText;
    public RectTransform breathCircle;
    public TextMeshProUGUI guideText;

    private float minSize = 80f;
    private float maxSize = 250f;
    private float breathSpeed = 0.5f;
    private float t = 0f;
    private bool growing = true;

    void Update()
    {
        if (tutorialUI == null || !tutorialUI.activeSelf) return;

        if (growing)
        {
            t += Time.deltaTime * breathSpeed;
            if (t >= 1f) { t = 1f; growing = false; }
        }
        else
        {
            t -= Time.deltaTime * breathSpeed;
            if (t <= 0f) { t = 0f; growing = true; }
        }

        float size = Mathf.Lerp(minSize, maxSize, t);
        breathCircle.sizeDelta = new Vector2(size, size);

        if (growing)
            guideText.text = "천천히 불어보세요...";
        else
            guideText.text = "잠깐 멈추세요...";
    }

    public void StartGame()
    {
        tutorialUI.SetActive(false);
        countdownText.SetActive(true);
    }
}