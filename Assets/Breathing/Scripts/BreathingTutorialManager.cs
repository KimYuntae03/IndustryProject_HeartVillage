using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BreathingTutorialManager : MonoBehaviour
{
    public GameObject tutorialPanel;
    public Image fingerImage;
    public Sprite[] fingerFrames;
    public TMP_Text tutorialText;
    public Button nextButton;
    public BreathingCountdownManager countdownManager;
    public GameObject resultUI;

    private int currentPage = 0;
    private string[] pages = {
        "마이크에 숨을 불어넣으세요!",
        "풍선이 초록색이 될 때까지\n계속 불어주세요!",
        "30초 동안 유지하면\n미션 성공!"
    };

    void Start()
    {
        countdownManager.enabled = false;
        tutorialPanel.SetActive(true);
        ShowPage(0);
        StartCoroutine(AnimateFinger());
        if (resultUI != null) resultUI.SetActive(false);
    }

    void ShowPage(int index)
    {
        tutorialText.text = pages[index];
        if (index == pages.Length - 1)
        {
            nextButton.GetComponentInChildren<TMP_Text>().text = "시작!";
        }
        else
        {
            nextButton.GetComponentInChildren<TMP_Text>().text = "다음";
        }
    }

    public void OnNextButton()
    {
        currentPage++;
        if (currentPage >= pages.Length)
        {
            tutorialPanel.SetActive(false);
            countdownManager.enabled = true;
        }
        else
        {
            ShowPage(currentPage);
        }
    }

    IEnumerator AnimateFinger()
    {
        int frame = 0;
        while (true)
        {
            if (fingerFrames != null && fingerFrames.Length > 0)
            {
                fingerImage.sprite = fingerFrames[frame];
                frame = (frame + 1) % fingerFrames.Length;
            }
            yield return new WaitForSeconds(0.4f);
        }
    }
}