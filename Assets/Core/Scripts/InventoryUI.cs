using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
{
    [Header("인벤토리 UI")]
    [SerializeField] private GameObject inventoryRoot;
    [SerializeField] private RectTransform inventoryPanel;
    [SerializeField] private CanvasGroup inventoryCanvasGroup;

    [Header("애니메이션")]
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private float closedScale = 0.85f;

    //인벤토리를 열 때 카메라 줌 효과를 받기위한 컴포넌트 참조 변수들
    [Header("카메라 줌")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float inventoryCameraSize = 3.5f;//줌 인 될 카메라 크기
    [SerializeField] private float zoomSpeed = 6f; 

    private float normalCameraSize;
    private float targetCameraSize;

    private bool isOpen;
    private Coroutine animationCoroutine;

    private void Start()
    {
        //게임 시작 시 인벤토리를 닫힌 상태로 초기화
        isOpen = false;

        inventoryPanel.localScale = Vector3.one;
        inventoryCanvasGroup.alpha = 1f;
        inventoryRoot.SetActive(false);

        //인스펙터에서 카메라 등록이 안되었다면 메인카메라 태그가 설정된 카메라를 가져옴
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        //인벤토리 닫을 복구할 때 원래 카메라 크기 저장
        if (mainCamera != null)
        {
            normalCameraSize = mainCamera.orthographicSize;
            targetCameraSize = normalCameraSize;
        }

    }

    private void Update()
    {
        mainCamera.orthographicSize = Mathf.Lerp(
            mainCamera.orthographicSize,
            targetCameraSize,
            zoomSpeed * Time.unscaledDeltaTime
        );
    }

    public void ToggleInventory()
    {
        //현재 상태에 따라 인벤토리를 열거나 닫음
        if (isOpen)//열려있으면 닫고
        {
            CloseInventory();
        }
        else//닫혀있으면 열고
        {
            OpenInventory();
        }

        //클릭한 버튼의 선택 상태를 해제함.(엔터키로 다시 열리거나 닫히는 현상 방지)
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void OpenInventory()
    {
        isOpen = true;
        inventoryRoot.SetActive(true);

        //인벤토리를 여는 애니메이션이 재생 중이면 중단
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(
            AnimateInventory(true)
        );

        targetCameraSize = inventoryCameraSize;
    }

    public void CloseInventory()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;

        //기존 애니메이션이 재생 중이면 중단
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(
            AnimateInventory(false)
        );

        targetCameraSize = normalCameraSize;
    }

    //인벤토리가 열리고 닫힐때 애니메이션
    private IEnumerator AnimateInventory(bool opening)
    {   
        //인벤토리 크기와 투명도를 부드럽게 변경
        float elapsedTime = 0f;

        Vector3 startScale = inventoryPanel.localScale;
        Vector3 targetScale = opening
            ? Vector3.one
            : Vector3.one * closedScale;

        float startAlpha = inventoryCanvasGroup.alpha;
        float targetAlpha = opening ? 1f : 0f;

        if (opening)
        {
            startScale = Vector3.one * closedScale;
            startAlpha = 0f;

            inventoryPanel.localScale = startScale;
            inventoryCanvasGroup.alpha = startAlpha;
        }

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = elapsedTime / animationDuration;
            float smoothProgress = Mathf.SmoothStep(
                0f,
                1f,
                progress
            );

            inventoryPanel.localScale = Vector3.Lerp(
                startScale,
                targetScale,
                smoothProgress
            );

            inventoryCanvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                smoothProgress
            );

            yield return null;
        }

        inventoryPanel.localScale = targetScale;
        inventoryCanvasGroup.alpha = targetAlpha;

        if (!opening)
        {
            inventoryRoot.SetActive(false);

            // 다음에 열릴 때를 위한 초기화
            inventoryPanel.localScale = Vector3.one;
            inventoryCanvasGroup.alpha = 1f;
        }

        animationCoroutine = null;
    }
}