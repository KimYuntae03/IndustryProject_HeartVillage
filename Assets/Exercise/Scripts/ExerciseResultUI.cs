using System.Collections;
using TMPro;
using UnityEngine;

public class ExerciseResultUI : MonoBehaviour
{
    [Header("결과창")]
    [SerializeField] private GameObject resultRoot;
    [SerializeField] private TMP_Text currentRecordText;
    [SerializeField] private TMP_Text previousBestText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private GameObject returnButton;

    [Header("결과창 표시 전 숨길 운동 HUD")]
    [SerializeField] private GameObject distanceText;
    [SerializeField] private GameObject remainingTimeText;
    [SerializeField] private GameObject goBackButton;

    [Header("운동 기록")]
    [SerializeField] private ExerciseRecordManager recordManager;

    [Header("순차 표시 간격")]
    [SerializeField, Min(0f)] private float revealInterval = 0.35f;

    private Coroutine revealCoroutine;

    private void Awake()
    {
        if (resultRoot != null)
        {
            resultRoot.SetActive(false);
        }
    }

    // 6분 완료 후 기록 저장이 끝난 다음 호출
    public void ShowResult()
    {
        if (recordManager == null)
        {
            Debug.LogError("ExerciseRecordManager가 연결되지 않았습니다.");
            return;
        }

        if (resultRoot == null ||
            currentRecordText == null ||
            previousBestText == null ||
            rewardText == null ||
            returnButton == null)
        {
            Debug.LogError("ExerciseResultUI의 결과창 참조가 비어 있습니다.");
            return;
        }

        // 운동 중 표시되던 HUD를 숨김
        SetActiveIfAssigned(distanceText, false);
        SetActiveIfAssigned(remainingTimeText, false);
        SetActiveIfAssigned(goBackButton, false);

        // 순차 표시할 항목을 먼저 숨김
        currentRecordText.gameObject.SetActive(false);
        previousBestText.gameObject.SetActive(false);
        rewardText.gameObject.SetActive(false);
        returnButton.SetActive(false);

        // 이번 운동 기록 표시
        currentRecordText.text =
            $"측정 기록\n{recordManager.CurrentDistance:0.0} m";

        // 기존 최고 기록을 넘어야 신기록 보상을 받는다.
        if (recordManager.HadPreviousRecord)
        {
            previousBestText.text =
                $"이전 최고 기록\n{recordManager.PreviousBestDistance:0.0} m";
        }
        else
        {
            previousBestText.text =
                "이전 최고 기록\n기록 없음";
        }

        string reason = recordManager.DistanceCheck switch
        {
            ExerciseDistanceCheck.Mismatch => "걸음 거리와 위치 기록이 크게 달라요",
            ExerciseDistanceCheck.Inconclusive => "거리 확인이 어려워 참여 보상만 지급해요",
            _ => "운동 보상"
        };
        rewardText.text = $"{reason}\n+{recordManager.AwardedHeartPoints} 하트 포인트";

        // 반투명 배경을 먼저 표시
        resultRoot.SetActive(true);
        resultRoot.transform.SetAsLastSibling();

        // 이전 실행 중인 표시 코루틴이 있다면 중복 실행 방지
        if (revealCoroutine != null)
        {
            StopCoroutine(revealCoroutine);
        }

        revealCoroutine = StartCoroutine(RevealResultInOrder());
    }

    // 결과 내용을 일정 간격으로 하나씩 표시
    private IEnumerator RevealResultInOrder()
    {
        yield return new WaitForSecondsRealtime(revealInterval);
        currentRecordText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(revealInterval);
        previousBestText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(revealInterval);
        rewardText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(revealInterval);
        returnButton.SetActive(true);

        revealCoroutine = null;
    }

    private void SetActiveIfAssigned(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    // 마을로 돌아가기 버튼에서 호출
    public void ReturnToVillage()
    {
        Time.timeScale = 1f;

        if (SceneLoader.Instance == null)
        {
            Debug.LogError("SceneLoader.Instance가 없습니다. CoreGameplay에서 시작했는지 확인하세요.");
            return;
        }

        SceneLoader.Instance.LoadScene("CoreGameplay");
    }
}
