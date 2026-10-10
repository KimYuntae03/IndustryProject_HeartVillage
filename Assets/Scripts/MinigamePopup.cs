using UnityEngine;

public class MinigamePopup : MonoBehaviour
{
    [SerializeField] private GameObject popupWindow;
    [SerializeField] private SceneLoader sceneLoader;

    private string targetSceneName;

    public bool IsOpen => popupWindow.activeSelf;

    private void Start()
    {
        popupWindow.SetActive(false);
    }

    public void OpenPopup(string sceneName)
    {
        targetSceneName = sceneName;
        popupWindow.SetActive(true);
    }

    public void MoveScene()
    {
        sceneLoader.LoadScene(targetSceneName);
    }

    public void ClosePopup()
    {
        popupWindow.SetActive(false);
    }
}