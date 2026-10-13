using UnityEngine;

public class ExerciseQuitPopup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject quitPopup;

    [Header("씬 이동")]
    [SerializeField] private SceneLoader sceneLoader;

    private void Start()
    {
        // 씬에 들어왔을 때 게임이 정상적으로 진행되도록 초기화
        Time.timeScale = 1f;
        quitPopup.SetActive(false);
    }

    public void OpenPopup()
    {
        // 팝업을 열고 게임을 일시정지
        quitPopup.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ClosePopup()
    {
        // 팝업을 닫고 게임을 다시 진행
        quitPopup.SetActive(false);
        Time.timeScale = 1f;
    }

    public void QuitExercise()
    {
        // Time.timeScale은 씬이 바뀌어도 유지되므로 반드시 복구
        Time.timeScale = 1f;
        sceneLoader.LoadScene("CoreGameplay");
    }
}