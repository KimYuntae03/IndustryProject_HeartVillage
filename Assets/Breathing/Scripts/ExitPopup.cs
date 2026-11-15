using UnityEngine;

public class ExitPopup : MonoBehaviour
{
    public GameObject popupRoot;

    void Start()
    {
        popupRoot.SetActive(false);
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    public void Open()
    {
        popupRoot.SetActive(true);
        popupRoot.transform.SetAsLastSibling();
        Time.timeScale = 0f;
    }

    public void Close()
    {
        popupRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ConfirmExit()
    {
        Time.timeScale = 1f;
        if (SceneLoader.Instance == null)
        {
            UnityEngine.Debug.LogError("SceneLoader.Instance가 없습니다. CoreGameplay에서 시작했는지 확인하세요.");
            return;
        }
        SceneLoader.Instance.LoadScene("CoreGameplay");
    }
}