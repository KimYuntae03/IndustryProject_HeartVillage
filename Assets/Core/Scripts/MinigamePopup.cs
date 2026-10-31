using UnityEngine;
using UnityEngine.EventSystems;

public class MinigamePopup : MonoBehaviour
{
    [SerializeField] private GameObject popupWindow;
    [SerializeField] private PlayerController playerController;
    private string targetSceneName;

    public bool IsOpen => popupWindow != null && popupWindow.activeSelf;

    private void Start()
    {
        popupWindow.SetActive(false);
        playerController.SetCanMove(true);
    }

    public void OpenPopup(string sceneName)
    {
        if (IsOpen) //팝업이 열려있다면 다시 실행 X
        {
            return;
        }

        targetSceneName = sceneName;
        popupWindow.SetActive(true);

        playerController.SetCanMove(false);//열려있으면 이동 X
    }

    public void MoveScene()
    {
        if (SceneLoader.Instance == null)
        {
            Debug.LogError("SceneLoader.Instance가 없습니다. CoreGameplay의 SceneLoader를 확인하세요.");
            return;
        }

        playerController.SetCanMove(true);//씬 이동 전에 이동잠금 해제
        SceneLoader.Instance.LoadScene(targetSceneName);
    }

    public void ClosePopup()
    {
        popupWindow.SetActive(false);

        //찹업 닫으면 플레이어 이동 가능
        playerController.SetCanMove(true);

        // 취소 버튼이 Enter 키로 다시 눌리는 현상 방지
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
