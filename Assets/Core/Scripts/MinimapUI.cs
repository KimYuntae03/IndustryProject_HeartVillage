using UnityEngine;

public class MinimapUI : MonoBehaviour
{
    [Header("미니맵 UI")]
    [SerializeField] private GameObject minimap;

    [Header("시작 설정")]
    [SerializeField] private bool startVisible = true;

    private bool isVisible;

    private void Start()
    {
        // 설정한 초기 상태로 미니맵 표시
        SetMinimapVisible(startVisible);
    }

    public void ToggleMinimap()
    {
        // 현재 상태의 반대로 변경
        SetMinimapVisible(!isVisible);
    }

    private void SetMinimapVisible(bool visible)
    {
        isVisible = visible;
        minimap.SetActive(isVisible);
    }
}