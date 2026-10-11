using System.Collections;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("인벤토리 UI")]
    [SerializeField] private GameObject inventoryRoot;
    [SerializeField] private RectTransform inventoryPanel;
    [SerializeField] private CanvasGroup inventoryCanvasGroup;

    [Header("애니메이션")]
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private float closedScale = 0.85f;

    private bool isOpen;
    private Coroutine animationCoroutine;

    private void Start()
    {
        //게임 시작 시 인벤토리를 닫힌 상태로 초기화
        isOpen = false;

        inventoryPanel.localScale = Vector3.one;
        inventoryCanvasGroup.alpha = 1f;

        inventoryRoot.SetActive(false);
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