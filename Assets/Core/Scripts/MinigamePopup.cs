using UnityEngine;
using UnityEngine.EventSystems;

public class MinigamePopup : MonoBehaviour
{
    [SerializeField] private GameObject popupWindow;
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private PlayerController playerController;
    private string targetSceneName;
    private GameObject activeWindow;

    public bool IsOpen => activeWindow != null && activeWindow.activeSelf;
    public event System.Action Opened;

    private void Start()
    {
        popupWindow.SetActive(false);
        playerController.SetCanMove(true);
    }

    public void OpenPopup(string sceneName)
    {
        if (IsOpen) return;
        targetSceneName = sceneName;
        OpenWindow(popupWindow);
    }

    public void OpenWindow(GameObject window)
    {
        if (IsOpen || window == null) return;
        Opened?.Invoke();
        activeWindow = window;
        activeWindow.SetActive(true);
        playerController.SetCanMove(false);
    }

    public void MoveScene()
    {
        playerController.SetCanMove(true);//씬 이동 전에 이동잠금 해제
        sceneLoader.LoadScene(targetSceneName);
    }

    public void ClosePopup()
    {
        if (!IsOpen) return;
        activeWindow.SetActive(false);
        activeWindow = null;

        //찹업 닫으면 플레이어 이동 가능
        playerController.SetCanMove(true);

        // 취소 버튼이 Enter 키로 다시 눌리는 현상 방지
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
