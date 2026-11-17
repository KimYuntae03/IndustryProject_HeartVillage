using System;
using UnityEngine;

[Serializable]
public sealed class LocalPlayerProfile
{
    private const string ProfileKey = "HeartVillage.StartScene.Profile";

    public string playerName;
    public string heightCm;
    // Minutes since midnight, one entry per daily dose.
    public int[] doseMinutes;

    public static LocalPlayerProfile Load()
    {
        string json = PlayerPrefs.GetString(ProfileKey, "");
        if (string.IsNullOrEmpty(json)) return null;

        try
        {
            return JsonUtility.FromJson<LocalPlayerProfile>(json);
        }
        catch (ArgumentException)
        {
            Debug.LogWarning("저장된 사용자 정보를 읽을 수 없습니다.");
            return null;
        }
    }

    public void Save()
    {
        PlayerPrefs.SetString(ProfileKey, JsonUtility.ToJson(this));
        PlayerPrefs.Save();
    }
}
