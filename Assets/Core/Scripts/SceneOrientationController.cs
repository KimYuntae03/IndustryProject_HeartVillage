using UnityEngine;

public class SceneOrientationController : MonoBehaviour
{
    public enum OrientationMode
    {
        Landscape,
        Portrait
    }

    [Header("이 씬의 화면 방향")]
    [SerializeField]
    private OrientationMode orientationMode;

    private void Awake()
    {
        ApplyOrientation();
    }

    private void ApplyOrientation()
    {
        if (orientationMode == OrientationMode.Portrait)
        {
            SetPortrait();
        }
        else
        {
            SetLandscape();
        }
    }

    private void SetPortrait()
    {
        // 세로 정방향만 허용
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;

        Screen.orientation = ScreenOrientation.Portrait;
    }

    private void SetLandscape()
    {
        // 가로 양방향만 허용
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        Screen.orientation = ScreenOrientation.AutoRotation;
    }
}