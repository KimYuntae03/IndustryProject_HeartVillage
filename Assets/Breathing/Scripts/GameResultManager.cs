using System.Collections;
using UnityEngine;
using TMPro;

public class GameResultManager : MonoBehaviour
{
    [Header("결과창")]
    public BreathingManager breathingManager;
    public GameObject resultUI;
    public GameObject gameUI;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI rewardText;
    public GameObject returnButton;

    [Header("임시 보상 설정")]
    [SerializeField, Min(0)] private int baseReward = 100;
    [SerializeField, Min(0)] private int newRecordBonus = 50;

    [Header("순차 표시 간격")]
    [SerializeField, Min(0f)] private float revealInterval = 0.35f;

    private float gameTime = 30f;
    private float timer = 0f;
    private bool gameOver = false;

    void Update()
    {
        if (gameOver) return;

        timer += Time.deltaTime;

        if (timer >= gameTime)
            EndGame();
    }

    void EndGame()
    {
        gameOver = true;
        gameUI.SetActive(false);
        resultUI.SetActive(true);
        resultUI.transform.SetAsLastSibling();

        bool success = breathingManager.IsMissionSuccess();
        float best = breathingManager.GetBestDuration();
        float goal = breathingManager.GetSuccessDuration();

        resultText.text = success
            ? $"미션 성공!\n최고 기록: {best:F1}초 / 목표: {goal:F1}초"
            : $"미션 실패\n최고 기록: {best:F1}초 / 목표: {goal:F1}초";

        bool isNewRecord = best > PlayerPrefs.GetFloat("BestBreathDuration_Prev", 0f);
        int bonus = (success && isNewRecord) ? newRecordBonus : 0;
        int total = baseReward + bonus;

        if (success && isNewRecord)
            rewardText.text = $"기본 보상 {baseReward} 코인\n신기록 추가 보상 +{bonus} 코인\n총 보상 {total} 코인";
        else
            rewardText.text = $"기본 보상 {baseReward} 코인\n총 보상 {total} 코인";

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