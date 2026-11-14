using System;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public sealed class MedicationCheckSlot : MonoBehaviour
{
    [SerializeField] private string recordKey;
    [SerializeField] private UnityEngine.UI.Image checkmark;

    private UnityEngine.UI.Button button;
    private int slot;
    private float nextRefresh;

    private static int DayKey(DateTime day) => day.Year * 10000 + day.Month * 100 + day.Day;

    private void Awake()
    {
        button = GetComponent<UnityEngine.UI.Button>();
        slot = MedicationSchedule.FindSlot(recordKey);
        if (slot < 0)
        {
            Debug.LogError($"복약 시간 설정을 찾을 수 없습니다: {recordKey}", this);
            button.interactable = false;
            enabled = false;
            return;
        }
        GetComponentInChildren<TMPro.TMP_Text>(true).text =
            MedicationSchedule.TimeAt(slot).ToString(@"hh\:mm");
        button.onClick.AddListener(RecordDose);
    }

    private void OnEnable() => Refresh(DateTime.Now);

    private void Update()
    {
        if (Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + 0.5f;
        Refresh(DateTime.Now);
    }

    private void RecordDose()
    {
        var now = DateTime.Now;
        int today = DayKey(now);
        if (MedicationSchedule.CanRecord(slot, now) && PlayerPrefs.GetInt(recordKey) != today)
        {
            PlayerPrefs.SetInt(recordKey, today);
            PlayerPrefs.Save();
        }
        Refresh(now);
    }

    private void Refresh(DateTime now)
    {
        bool recorded = PlayerPrefs.GetInt(recordKey) == DayKey(now);
        checkmark.enabled = recorded;
        button.interactable = !recorded && MedicationSchedule.CanRecord(slot, now);
    }
}
