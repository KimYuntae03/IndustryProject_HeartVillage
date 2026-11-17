using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    private bool loading;

    public void StartGame()
    {
        if (loading) return;
        loading = true;
        SceneManager.LoadSceneAsync("CoreGameplay");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
