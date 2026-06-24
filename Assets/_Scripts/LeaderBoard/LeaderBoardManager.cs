using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class LeaderboardEntry
{
    public string playerName;
    public float score;
}

[System.Serializable]
public class LeaderboardData
{
    public List<LeaderboardEntry> entries = new();
}

public static class LeaderBoardManager
{
    private const string SaveKey = "Leaderboard";

    private static List<LeaderboardEntry> entries;

    public static IReadOnlyList<LeaderboardEntry> Entries
    {
        get
        {
            if (entries == null)
                Load();

            return entries;
        }
    }

    public static bool IsHighScore(float score)
    {
        if (entries == null)
            Load();

        if (entries.Count < 10)
            return true;

        return score > entries[^1].score;
    }

    public static void AddScore(string playerName, float score)
    {
        if (entries == null)
            Load();

        entries.Add(new LeaderboardEntry
        {
            playerName = playerName,
            score = score
        });

        entries = entries
            .OrderByDescending(e => e.score)
            .Take(10)
            .ToList();

        Save();
    }

    public static void ClearLeaderboard()
    {
        entries = new List<LeaderboardEntry>();
        PlayerPrefs.DeleteKey(SaveKey);
    }

    private static void Save()
    {
        LeaderboardData data = new()
        {
            entries = entries
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    private static void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            entries = new List<LeaderboardEntry>();
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey);

        LeaderboardData data =
            JsonUtility.FromJson<LeaderboardData>(json);

        entries = data?.entries ?? new List<LeaderboardEntry>();
    }
}
