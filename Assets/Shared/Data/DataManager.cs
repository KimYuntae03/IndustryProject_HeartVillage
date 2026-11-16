using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [Serializable]
    private class PlayLogEntry
    {
        public string gameId;
        public int zoneId;
        public bool success;
        public float score;
        public float playSeconds;
        public string playedAt;
    }

    [Serializable]
    private class SaveData
    {
        public int heartPoint;
        public List<PlayLogEntry> playLogs = new List<PlayLogEntry>();
    }

    private const string SaveKey = "HV_SaveData";
    private static DataManager instance;
    private SaveData data;

    [SerializeField] private int baseReward = 100;
    [SerializeField] private int newRecordBonus = 50;

    public event Action OnDataChanged;

    public static DataManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("DataManager");
                instance = go.AddComponent<DataManager>();
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    void Load()
    {
        string json = PlayerPrefs.GetString(SaveKey, "");
        data = string.IsNullOrEmpty(json) ? new SaveData() : JsonUtility.FromJson<SaveData>(json);
        if (data == null) data = new SaveData();
        if (data.playLogs == null) data.playLogs = new List<PlayLogEntry>();
    }

    void Save()
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public SaveOutcome SaveResult(MinigameResult result)
    {
        float previousBest = GetBestScore(result.gameId);
        bool isNewRecord = result.score > previousBest;
        bool eligible = result.success || result.rewardOnFail;
        int baseAmount = eligible ? baseReward : 0;
        int bonus = (eligible && result.allowBonus && isNewRecord) ? newRecordBonus : 0;
        int total = baseAmount + bonus;

        data.playLogs.Add(new PlayLogEntry
        {
            gameId = result.gameId,
            zoneId = result.zoneId,
            success = result.success,
            score = result.score,
            playSeconds = result.playSeconds,
            playedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
        });
        data.heartPoint += total;
        Save();
        OnDataChanged?.Invoke();

        return new SaveOutcome
        {
            baseReward = baseAmount,
            bonus = bonus,
            total = total,
            isNewRecord = isNewRecord,
            bestScore = Mathf.Max(previousBest, result.score)
        };
    }

    public int GetHeartPoint()
    {
        return data.heartPoint;
    }

    public float GetBestScore(string gameId)
    {
        float best = 0f;
        foreach (PlayLogEntry log in data.playLogs)
        {
            if (log.gameId == gameId && log.score > best) best = log.score;
        }
        return best;
    }

    public int GetTodaySuccessCount(string gameId)
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        int count = 0;
        foreach (PlayLogEntry log in data.playLogs)
        {
            if (log.gameId == gameId && log.success && log.playedAt.StartsWith(today)) count++;
        }
        return count;
    }

    public void ResetAll()
    {
        data = new SaveData();
        Save();
        OnDataChanged?.Invoke();
    }
}