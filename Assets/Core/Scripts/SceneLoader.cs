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

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isLoading = true;

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
