using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Image = UnityEngine.UI.Image;

public class BreathingTutorialManager : MonoBehaviour
{
    public GameObject tutorialPanel;
    public Image fingerImage;
    public Sprite[] fingerFrames;
    public TMP_Text tutorialText;
    public Button nextButton;
    public BreathingCountdownManager countdownManager;
    public GameObject resultUI;

    [Header("Character Animation")]
    public Image characterImage;
    public Sprite[] characterFrames;

    [Header("Tutorial Balloon")]
    public Image tutorialBalloon;
    public Sprite[] balloonFrames;

    [Header("Success Page")]
    public Image successBalloon;
    public TMP_Text successText;

    private int currentPage = 0;
    private string[] pages = {
        "마이크에 숨을 불어 넣으세요!",
        "풍선이 초록색이 될 때까지\n계속 불어주세요!",
        "30초 안에 5초를 유지하면\n미션 성공!"
    };

    void Start()
    {
        countdownManager.enabled = false;
        tutorialPanel.SetActive(true);
        ShowPage(0);
        StartCoroutine(AnimateFinger());
        if (resultUI != null) resultUI.SetActive(false);
        if (characterImage != null) characterImage.gameObject.SetActive(true);
        StartCoroutine(AnimateCharacter());
    }

    void ShowPage(int index)
    {
        tutorialText.text = pages[index];
        if (characterImage != null)
            characterImage.gameObject.SetActive(index == 0);
        if (tutorialBalloon != null)
        {
            tutorialBalloon.gameObject.SetActive(index == 1);
            if (index == 1) StartCoroutine(AnimateTutorialBalloon());
        }
        if (successBalloon != null)
            successBalloon.gameObject.SetActive(index == 2);
        if (successText != null)
            successText.gameObject.SetActive(index == 2);
        if (index == 2) StartCoroutine(AnimateSuccessPage());
    }

    public void OnNextButton()
    {
        currentPage++;
        if (currentPage >= pages.Length)
        {
            tutorialPanel.SetActive(false);
            countdownManager.StartCountdown();
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

    IEnumerator AnimateCharacter()
    {
        int frame = 0;
        while (true)
        {
            if (characterFrames != null && characterFrames.Length > 0 && characterImage != null)
            {
                characterImage.sprite = characterFrames[frame];
                frame = (frame + 1) % characterFrames.Length;
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator AnimateTutorialBalloon()
    {
        if (balloonFrames == null || balloonFrames.Length == 0) yield break;

        for (int i = 0; i < balloonFrames.Length; i++)
        {
            tutorialBalloon.sprite = balloonFrames[i];
            float scale = Mathf.Lerp(0.5f, 1.5f, (float)i / (balloonFrames.Length - 1));
            tutorialBalloon.rectTransform.localScale = new Vector3(scale, scale, 1f);
            tutorialBalloon.color = Color.Lerp(
                new Color(1f, 0.4f, 0.4f),
                new Color(0.2f, 0.8f, 0.2f),
                (float)i / (balloonFrames.Length - 1)
            );
            yield return new WaitForSeconds(0.15f);
        }

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(AnimateTutorialBalloon());
    }

    IEnumerator AnimateSuccessPage()
    {
        if (balloonFrames != null && balloonFrames.Length > 0 && successBalloon != null)
        {
            successBalloon.sprite = balloonFrames[balloonFrames.Length - 1];
            successBalloon.color = new Color(0.2f, 0.8f, 0.2f);
            successBalloon.rectTransform.localScale = Vector3.one;
            successBalloon.rectTransform.anchoredPosition = new Vector2(0, 0);
        }

        if (successText != null)
        {
            successText.text = "미션 성공!";
            successText.rectTransform.localScale = Vector3.zero;
        }

        float flyT = 0f;
        Vector2 startPos = successBalloon != null ? successBalloon.rectTransform.anchoredPosition : Vector2.zero;
        while (flyT < 1f)
        {
            flyT += Time.deltaTime * 0.35f;
            if (successBalloon != null)
            {
                float eased = Mathf.SmoothStep(0f, 1f, flyT);
                successBalloon.rectTransform.anchoredPosition = Vector2.Lerp(startPos, startPos + new Vector2(0, 500f), eased);
                successBalloon.color = new Color(0.2f, 0.8f, 0.2f, 1f - flyT);
            }
            yield return null;
        }
        if (successBalloon != null)
            successBalloon.gameObject.SetActive(false);
    }
}