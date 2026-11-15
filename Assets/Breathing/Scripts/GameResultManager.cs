using System.Collections;
using UnityEngine;
using TMPro;
using Debug = UnityEngine.Debug;

public class GameResultManager : MonoBehaviour
{
    [Header("결과창")]
    public BreathingManager breathingManager;
    public GameObject resultUI;
    public GameObject gameUI;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI rewardText;
    public GameObject returnButton;

    [Header("끝내기 연출")]
    public RectTransform balloon;
    public float balloonFlyDuration = 2.5f;
    public float balloonFlyDistance = 1400f;

    [Header("임시 보상 설정")]
    [SerializeField, Min(0)] private int baseReward = 100;
    [SerializeField, Min(0)] private int newRecordBonus = 50;

    [Header("순차 표시 간격")]
    [SerializeField, Min(0f)] private float revealInterval = 0.35f;

    private bool gameOver = false;
    private bool isLeaving = false;
    private bool success = false;

    void Update()
    {
        if (gameOver) return;

        if (breathingManager.IsMissionSuccess() || breathingManager.IsGameOver())
            EndGame();
    }

    void EndGame()
    {
        gameOver = true;
        gameUI.SetActive(false);
        resultUI.SetActive(true);
        resultUI.transform.SetAsLastSibling();

        success = breathingManager.IsMissionSuccess();
        float best = breathingManager.GetBestDuration();
        float goal = breathingManager.GetSuccessDuration();

        resultText.text = success
            ? $"미션 성공!\n최고 기록: {best:F1}초 / 목표: {goal:F1}초"
            : $"다시 도전!\n최고 기록: {best:F1}초 / 목표: {goal:F1}초";

        bool isNewRecord = best > PlayerPrefs.GetFloat("BestBreathDuration_Prev", 0f);
        int bonus = (success && isNewRecord) ? newRecordBonus : 0;
        int total = baseReward + bonus;

        if (success && isNewRecord)
            rewardText.text = $"보상: {baseReward} Heart Point\n신기록 보너스: +{bonus} Heart Point\n합계: {total} Heart Point";
        else
            rewardText.text = $"보상: {baseReward} Heart Point\n합계: {total} Heart Point";

        returnButton.SetActive(false);
        StartCoroutine(RevealResult());
    }

    private IEnumerator RevealResult()
    {
        resultText.gameObject.SetActive(false);
        rewardText.gameObject.SetActive(false);

        yield return new WaitForSecondsRealtime(revealInterval);
        resultText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(revealInterval);
        rewardText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(revealInterval);
        returnButton.SetActive(true);
    }

    public void ReturnToVillage()
    {
        if (isLeaving) return;
        isLeaving = true;

        if (success && balloon != null)
            StartCoroutine(FlyAndLeave());
        else
            LoadVillage();
    }

    private IEnumerator FlyAndLeave()
    {
        returnButton.SetActive(false);
        resultText.gameObject.SetActive(false);
        rewardText.gameObject.SetActive(false);

        balloon.SetAsLastSibling();
        Vector2 startPos = balloon.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0f, balloonFlyDistance);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / balloonFlyDuration;
            float eased = t * t;
            float sway = Mathf.Sin(t * Mathf.PI * 3f) * 30f;
            balloon.anchoredPosition = Vector2.Lerp(startPos, endPos, eased) + new Vector2(sway, 0f);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.3f);
        LoadVillage();
    }

    private void LoadVillage()
    {
        Time.timeScale = 1f;
        if (SceneLoader.Instance == null)
        {
            Debug.LogError("SceneLoader.Instance가 없습니다. CoreGameplay에서 시작했는지 확인하세요.");
            return;
        }
        SceneLoader.Instance.LoadScene("CoreGameplay");
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}