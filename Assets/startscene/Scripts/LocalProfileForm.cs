using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LocalProfileForm : MonoBehaviour
{
    [Serializable]
    private struct TimeInput
    {
        public TMP_Dropdown hour;
        public TMP_Dropdown minute;
    }

    [SerializeField] private GameObject popupRoot;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField heightInput;
    [SerializeField] private Toggle medicationToggle;
    [SerializeField] private TMP_Dropdown frequency;
    [SerializeField] private TimeInput[] timeInputs;

    public void SaveProfile()
    {
        int count = medicationToggle.isOn ? frequency.value + 1 : 0;
        var profile = new LocalPlayerProfile
        {
            playerName = nameInput.text.Trim(),
            heightCm = heightInput.text.Trim(),
            doseMinutes = new int[count]
        };

        for (int i = 0; i < count; i++)
        {
            profile.doseMinutes[i] = timeInputs[i].hour.value * 60 + timeInputs[i].minute.value;
        }

        profile.Save();
        popupRoot.SetActive(false);
    }

    private void OnEnable()
    {
        var profile = LocalPlayerProfile.Load();

        nameInput.SetTextWithoutNotify(profile?.playerName ?? "");
        heightInput.SetTextWithoutNotify(profile?.heightCm ?? "");
        int[] doses = profile?.doseMinutes ?? Array.Empty<int>();
        int count = Mathf.Min(doses.Length, timeInputs.Length);
        for (int i = 0; i < timeInputs.Length; i++)
        {
            int minutes = i < count ? Mathf.Clamp(doses[i], 0, 1439) : 0;
            timeInputs[i].hour.SetValueWithoutNotify(minutes / 60);
            timeInputs[i].minute.SetValueWithoutNotify(minutes % 60);
        }

        frequency.value = Mathf.Max(0, count - 1);
        medicationToggle.isOn = count > 0;
    }
}
