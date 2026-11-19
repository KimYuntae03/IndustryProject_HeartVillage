using System;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public sealed class MedicationCheckSlot : MonoBehaviour
{
    [SerializeField] private string recordKey;
    [SerializeField] private UnityEngine.UI.Image checkmark;

    private UnityEngine.UI.Button button;
    private TimeSpan doseTime;
    private float nextRefresh;

    private static int DayKey(DateTime day) => day.Year * 10000 + day.Month * 100 + day.Day;

    private void Awake()
    {
        button = GetComponent<UnityEngine.UI.Button>();
        button.onClick.AddListener(RecordDose);
    }

    public void SetDoseTime(int minutes)
    {
        doseTime = TimeSpan.FromMinutes(Mathf.Clamp(minutes, 0, 1439));
        GetComponentInChildren<TMPro.TMP_Text>(true).text = doseTime.ToString(@"hh\:mm");
    }

    private bool CanRecord(DateTime now)
    {
        var elapsed = now.TimeOfDay - doseTime;
        return elapsed >= TimeSpan.Zero && elapsed < TimeSpan.FromMinutes(30);
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
        if (CanRecord(now) && PlayerPrefs.GetInt(recordKey) != today)
        {
            PlayerPrefs.SetInt(recordKey, today);
            PlayerPrefs.Save();
            HealthScoreData.RefreshMedication(now);
        }
        Refresh(now);
    }

    private void Refresh(DateTime now)
    {
        bool recorded = PlayerPrefs.GetInt(recordKey) == DayKey(now);
        checkmark.enabled = recorded;
        button.interactable = !recorded && CanRecord(now);
    }
}
