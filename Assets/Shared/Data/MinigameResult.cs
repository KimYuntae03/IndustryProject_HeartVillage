using System;

[Serializable]
public class MinigameResult
{
    public string gameId;
    public int zoneId;
    public bool success;
    public float score;
    public float playSeconds;
}

public class SaveOutcome
{
    public int baseReward;
    public int bonus;
    public int total;
    public bool isNewRecord;
    public float bestScore;
}