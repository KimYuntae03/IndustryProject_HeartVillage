using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadCoreGameplay()
    {
        SceneManager.LoadScene("CoreGameplay");
    }

    public void LoadBreathingMinigame()
    {
        SceneManager.LoadScene("BreathingMinigame");
    }

    public void LoadDietMinigame()
    {
        SceneManager.LoadScene("DietMinigame");
    }

    public void LoadExerciseMinigame()
    {
        SceneManager.LoadScene("ExerciseMinigame");
    }
}