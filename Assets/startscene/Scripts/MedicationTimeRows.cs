using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MedicationTimeRows : MonoBehaviour
{
    [SerializeField] private Toggle medicationToggle;
    [SerializeField] private TMP_Dropdown frequency;
    [SerializeField] private GameObject[] timeRows;

    private void OnEnable()
    {
        medicationToggle.onValueChanged.AddListener(OnMedicationChanged);
        frequency.onValueChanged.AddListener(OnFrequencyChanged);
        UpdateRows();
    }

    private void OnDisable()
    {
        medicationToggle.onValueChanged.RemoveListener(OnMedicationChanged);
        frequency.onValueChanged.RemoveListener(OnFrequencyChanged);
    }

    private void OnMedicationChanged(bool isOn) => UpdateRows();

    private void OnFrequencyChanged(int optionIndex) => UpdateRows();

    private void UpdateRows()
    {
        int count = medicationToggle.isOn ? frequency.value + 1 : 0;
        for (int i = 0; i < timeRows.Length; i++)
        {
            timeRows[i].SetActive(i < count);
        }
    }
}
