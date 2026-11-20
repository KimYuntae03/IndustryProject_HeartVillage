using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// 메뉴로 시작하는 시각적 미리보기. 모바일 빌드에는 포함되지 않는다.
[InitializeOnLoad]
public static class ExerciseWalkingPreview
{
    private const string PendingKey = "HeartVillage.ExerciseWalkingPreview.Pending";
    private const string ScenePath = "Assets/Exercise/Scenes/ExerciseMinigame.unity";

    static ExerciseWalkingPreview()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorApplication.update += StartWhenReady;
    }

    [MenuItem("Tools/HeartVillage/Preview Exercise Walking")]
    private static void StartPreview()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(ScenePath);
        SessionState.SetBool(PendingKey, true);
        EditorApplication.isPlaying = true;
    }

    [MenuItem("Tools/HeartVillage/Preview Exercise Walking", true)]
    private static bool CanStartPreview() => !EditorApplication.isPlayingOrWillChangePlaymode;

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
            SessionState.SetBool(PendingKey, false);
    }

    private static void StartWhenReady()
    {
        if (!SessionState.GetBool(PendingKey, false) || !EditorApplication.isPlaying
            || EditorApplication.isPaused || Time.timeSinceLevelLoad <= 0f) return;

        var mover = Object.FindFirstObjectByType<ExerciseRouteMover>();
        SessionState.SetBool(PendingKey, false);
        if (mover == null)
        {
            Debug.LogError("운동 미리보기: ExerciseRouteMover를 찾을 수 없습니다.");
            return;
        }

        // 센서와 완료 이벤트를 멈추고 UI를 숨겨 걷는 모습만 표시한다.
        foreach (var timer in Object.FindObjectsByType<ExerciseTimer>(FindObjectsSortMode.None))
        {
            timer.StopTimer();
            timer.enabled = false;
        }
        foreach (var tracker in Object.FindObjectsByType<ExerciseDistanceTracker>(FindObjectsSortMode.None))
            tracker.enabled = false;
        foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            canvas.gameObject.SetActive(false);

        mover.StartEditorWalkingPreview();
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        Debug.Log("운동 걷기 미리보기 시작: 0.5초마다 0.7m 가상 걸음. 기록/보상 저장 없음. Play 버튼으로 종료하세요.");
    }
}
