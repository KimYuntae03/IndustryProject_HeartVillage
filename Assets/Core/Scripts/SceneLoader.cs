using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("페이드 화면")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [Header("전환 시간")]
    [SerializeField, Min(0f)] private float fadeDuration = 0.35f;

    private bool isLoading;
    private Vector3? coreReturnPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeCanvasGroup != null)
        {
            // 시작 화면을 검게 덮은 뒤 처음 씬을 보여줌
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = true;
            fadeCanvasGroup.interactable = false;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (fadeCanvasGroup != null)
        {
            StartCoroutine(FadeInAtStartup());
        }
    }

    private IEnumerator FadeInAtStartup()
    {
        yield return Fade(0f);

        // 투명해진 뒤에는 화면 입력을 막지 않음
        fadeCanvasGroup.blocksRaycasts = false;
    }

    public void LoadScene(string sceneName)
    {
        if (isLoading)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"씬을 불러올 수 없습니다. 빌드 설정을 확인해 주세요: {sceneName}");
            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    // Consume once when the returning Core player is created. A fresh game uses its normal spawn.
    public bool TryConsumeCoreReturnPosition(out Vector3 position)
    {
        position = coreReturnPosition.GetValueOrDefault();
        bool hasPosition = coreReturnPosition.HasValue;
        coreReturnPosition = null;
        return hasPosition;
    }

    private PlayerController SaveCoreReturnPosition(string destination)
    {
        var source = SceneManager.GetActiveScene();
        bool isMinigame = destination == "BreathingMinigame" ||
            destination == "ExerciseMinigame" || destination == "DietMinigame";
        if (source.name != "CoreGameplay" || !isMinigame) return null;

        var player = FindFirstObjectByType<PlayerController>();
        if (player == null || player.gameObject.scene != source) return null;

        coreReturnPosition = player.transform.position;
        // Do not let input move the player away from the saved position during the fade.
        player.SetCanMove(false);
        return player;
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isLoading = true;
        var leavingPlayer = SaveCoreReturnPosition(sceneName);

        // 일시정지 상태로 다음 씬에 넘어가지 않도록 초기화
        Time.timeScale = 1f;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;
            yield return Fade(1f);
        }

        AsyncOperation loadOperation =
            SceneManager.LoadSceneAsync(sceneName);

        if (loadOperation == null)
        {
            Debug.LogError($"씬을 불러오지 못했습니다: {sceneName}");

            if (leavingPlayer != null)
            {
                coreReturnPosition = null;
                leavingPlayer.SetCanMove(true);
            }

            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 0f;
                fadeCanvasGroup.blocksRaycasts = false;
            }

            isLoading = false;
            yield break;
        }

        while (!loadOperation.isDone)
        {
            yield return null;
        }

        if (fadeCanvasGroup != null)
        {
            yield return Fade(0f);
            fadeCanvasGroup.blocksRaycasts = false;
        }

        isLoading = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvasGroup == null)
        {
            yield break;
        }

        float startAlpha = fadeCanvasGroup.alpha;
        float elapsedTime = 0f;

        if (fadeDuration <= 0f)
        {
            fadeCanvasGroup.alpha = targetAlpha;
            yield break;
        }

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime / fadeDuration
            );

            fadeCanvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                Mathf.SmoothStep(0f, 1f, progress)
            );

            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }
}
