using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance;

    [Header("Levels")]
    [SerializeField] private Level_Scriptable startingLevel;
    [SerializeField] private GameData GameData;

    [Header("Scenes")]
    [SerializeField] private string cinematicsSceneName = "CinematicsScene";
    [SerializeField] private string creditsSceneName = "Credits";

    public Level_Scriptable CurrentLevel { get; private set; }

    private readonly HashSet<string> _activeFlags = new HashSet<string>();
    private readonly HashSet<string> _discoveredLevelIDs = new HashSet<string>();
    private readonly HashSet<string> _completedLevelIDs = new HashSet<string>();

    private readonly Dictionary<string, int> _bestStarsByLevelID = new Dictionary<string, int>();
    private readonly Dictionary<string, float> _bestTimeByLevelID = new Dictionary<string, float>();

    private float _pendingCompletionTime = -1f;

    private const string CurrentLevelKey = "CurrentLevel";
    private const string ActiveFlagsKey = "ActiveFlags";
    private const string DiscoveredLevelsKey = "DiscoveredLevels";
    private const string CompletedLevelsKey = "CompletedLevels";
    private const string BestStarsKey = "BestStars";
    private const string BestTimesKey = "BestTimes";

    // ── Unity ────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ── Flags ────────────────────────────────────────────────────────────────

    public void SetFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag))
            return;

        _activeFlags.Add(flag);
        SaveProgress();
    }

    public void ClearFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag))
            return;

        _activeFlags.Remove(flag);
        SaveProgress();
    }

    public bool HasFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag))
            return false;

        return _activeFlags.Contains(flag);
    }

    public void ClearAllFlags()
    {
        _activeFlags.Clear();
        SaveProgress();
    }

    public void ApplyChoice(ChoiceData choice)
    {
        if (choice == null)
            return;

        if (choice.FlagsToSet != null)
        {
            foreach (string flag in choice.FlagsToSet)
            {
                SetFlag(flag);
            }
        }

        if (choice.FlagsToClear != null)
        {
            foreach (string flag in choice.FlagsToClear)
            {
                ClearFlag(flag);
            }
        }

        SaveProgress();
    }

    // ── Descubrimiento ───────────────────────────────────────────────────────

    public bool IsLevelDiscovered(Level_Scriptable level)
    {
        if (level == null)
            return false;

        return _discoveredLevelIDs.Contains(level.LevelID);
    }

    public void DiscoverLevel(Level_Scriptable level)
    {
        if (level == null)
            return;

        _discoveredLevelIDs.Add(level.LevelID);
        SaveProgress();
    }

    private void DiscoverStartingLevelIfNeeded()
    {
        if (startingLevel == null)
            return;

        _discoveredLevelIDs.Add(startingLevel.LevelID);
    }

    // ── Completado y estrellas ───────────────────────────────────────────────

    public bool IsLevelCompleted(Level_Scriptable level)
    {
        if (level == null)
            return false;

        return _completedLevelIDs.Contains(level.LevelID);
    }

    public int GetStarsForLevel(Level_Scriptable level)
    {
        if (level == null)
            return 0;

        if (_bestStarsByLevelID.TryGetValue(level.LevelID, out int stars))
            return stars;

        return 0;
    }

    public float GetBestTimeForLevel(Level_Scriptable level)
    {
        if (level == null)
            return Mathf.Infinity;

        if (_bestTimeByLevelID.TryGetValue(level.LevelID, out float time))
            return time;

        return Mathf.Infinity;
    }

    public void RegisterLevelCompletionTime(float completionTime)
    {
        _pendingCompletionTime = Mathf.Max(0f, completionTime);
    }

    public void CompleteLevel(Level_Scriptable level, float completionTime)
    {
        if (level == null)
            return;

        string levelID = level.LevelID;

        _completedLevelIDs.Add(levelID);
        _discoveredLevelIDs.Add(levelID);

        int earnedStars = level.GetStarsForTime(completionTime);

        if (!_bestStarsByLevelID.ContainsKey(levelID))
        {
            _bestStarsByLevelID[levelID] = earnedStars;
        }
        else
        {
            _bestStarsByLevelID[levelID] = Mathf.Max(_bestStarsByLevelID[levelID], earnedStars);
        }

        if (!_bestTimeByLevelID.ContainsKey(levelID))
        {
            _bestTimeByLevelID[levelID] = completionTime;
        }
        else
        {
            _bestTimeByLevelID[levelID] = Mathf.Min(_bestTimeByLevelID[levelID], completionTime);
        }

        SaveProgress();

        Debug.Log($"[Progression] Level completed: {level.name} | Time: {completionTime:F2} | Stars: {earnedStars}");
    }

    // ── Progresión ───────────────────────────────────────────────────────────

    public void ResetProgress()
    {
        _activeFlags.Clear();
        _discoveredLevelIDs.Clear();
        _completedLevelIDs.Clear();
        _bestStarsByLevelID.Clear();
        _bestTimeByLevelID.Clear();

        _pendingCompletionTime = -1f;

        CurrentLevel = startingLevel;

        DiscoverStartingLevelIfNeeded();

        PlayerPrefs.DeleteKey(CurrentLevelKey);
        PlayerPrefs.DeleteKey(ActiveFlagsKey);
        PlayerPrefs.DeleteKey(DiscoveredLevelsKey);
        PlayerPrefs.DeleteKey(CompletedLevelsKey);
        PlayerPrefs.DeleteKey(BestStarsKey);
        PlayerPrefs.DeleteKey(BestTimesKey);

        SaveProgress();
    }

    public void AdvanceLevel()
    {
        if (CurrentLevel == null)
        {
            CurrentLevel = startingLevel;
            DiscoverStartingLevelIfNeeded();
        }

        float completionTime = _pendingCompletionTime >= 0f
            ? _pendingCompletionTime
            : Mathf.Infinity;

        _pendingCompletionTime = -1f;

        CompleteLevel(CurrentLevel, completionTime);

        Level_Scriptable next = EvaluateNextLevel();

        if (next == null)
        {
            SaveProgress();

            if (GameManager.Instance != null)
                GameManager.Instance.ChangeState(GameState.Credits);

            SceneManager.LoadScene(creditsSceneName);
            return;
        }

        DiscoverLevel(next);

        CurrentLevel = next;
        SaveProgress();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.IsOutro = false;
            GameManager.Instance.ChangeState(GameState.Cinematic);
        }

        SceneManager.LoadScene(cinematicsSceneName);
    }

    public void AdvanceLevel(float completionTime)
    {
        RegisterLevelCompletionTime(completionTime);
        AdvanceLevel();
    }

    private Level_Scriptable EvaluateNextLevel()
    {
        if (CurrentLevel == null)
            return null;

        Debug.Log($"[Progression] Evaluando desde: {CurrentLevel.name}");

        if (CurrentLevel.Variants != null)
        {
            foreach (LevelVariant transition in CurrentLevel.Variants)
            {
                if (transition == null)
                    continue;

                bool match = AllFlagsActive(transition.RequiredFlags);

                Debug.Log($"  Transition flags: [{string.Join(", ", transition.RequiredFlags)}] → match: {match}");

                if (match)
                {
                    Debug.Log($"  → Eligiendo variante: {transition.NextLevel?.name ?? "NULL"}");
                    return transition.NextLevel;
                }
            }
        }

        Debug.Log($"  → Sin match, usando default: {CurrentLevel.DefaultNextLevel?.name ?? "NULL (Credits)"}");

        return CurrentLevel.DefaultNextLevel;
    }

    public void LoadLevel(string levelIDOrName)
    {
        Level_Scriptable level = FindLevelByIDOrName(levelIDOrName);

        if (level == null)
        {
            Debug.LogError($"Nivel '{levelIDOrName}' no encontrado.");
            return;
        }

        LoadLevel(level);
    }

    public void LoadLevel(Level_Scriptable level)
    {
        if (level == null)
            return;

        if (!IsLevelDiscovered(level))
        {
            Debug.LogWarning($"Intentaste cargar un nivel no descubierto: {level.name}");
            return;
        }

        CurrentLevel = level;
        SaveProgress();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.IsOutro = false;
            GameManager.Instance.ChangeState(GameState.Cinematic);
        }

        SceneManager.LoadScene(cinematicsSceneName);
    }

    private bool AllFlagsActive(List<string> flags)
    {
        if (flags == null || flags.Count == 0)
            return true;

        foreach (string flag in flags)
        {
            if (!_activeFlags.Contains(flag))
                return false;
        }

        return true;
    }

    // ── Persistencia ─────────────────────────────────────────────────────────

    public void SaveProgress()
    {
        if (CurrentLevel != null)
            PlayerPrefs.SetString(CurrentLevelKey, CurrentLevel.LevelID);

        PlayerPrefs.SetString(ActiveFlagsKey, SerializeHashSet(_activeFlags));
        PlayerPrefs.SetString(DiscoveredLevelsKey, SerializeHashSet(_discoveredLevelIDs));
        PlayerPrefs.SetString(CompletedLevelsKey, SerializeHashSet(_completedLevelIDs));
        PlayerPrefs.SetString(BestStarsKey, SerializeIntDictionary(_bestStarsByLevelID));
        PlayerPrefs.SetString(BestTimesKey, SerializeFloatDictionary(_bestTimeByLevelID));

        PlayerPrefs.Save();
    }

    public void LoadProgress()
    {
        _activeFlags.Clear();
        _discoveredLevelIDs.Clear();
        _completedLevelIDs.Clear();
        _bestStarsByLevelID.Clear();
        _bestTimeByLevelID.Clear();

        DeserializeHashSet(PlayerPrefs.GetString(ActiveFlagsKey, ""), _activeFlags);
        DeserializeHashSet(PlayerPrefs.GetString(DiscoveredLevelsKey, ""), _discoveredLevelIDs);
        DeserializeHashSet(PlayerPrefs.GetString(CompletedLevelsKey, ""), _completedLevelIDs);
        DeserializeIntDictionary(PlayerPrefs.GetString(BestStarsKey, ""), _bestStarsByLevelID);
        DeserializeFloatDictionary(PlayerPrefs.GetString(BestTimesKey, ""), _bestTimeByLevelID);

        if (PlayerPrefs.HasKey(CurrentLevelKey))
        {
            string savedLevelID = PlayerPrefs.GetString(CurrentLevelKey);
            CurrentLevel = FindLevelByIDOrName(savedLevelID);
        }

        if (CurrentLevel == null)
            CurrentLevel = startingLevel;

        DiscoverStartingLevelIfNeeded();

        if (CurrentLevel != null)
            _discoveredLevelIDs.Add(CurrentLevel.LevelID);

        SaveProgress();
    }

    private string SerializeHashSet(HashSet<string> set)
    {
        return string.Join(",", set);
    }

    private void DeserializeHashSet(string data, HashSet<string> target)
    {
        if (string.IsNullOrEmpty(data))
            return;

        string[] values = data.Split(',');

        foreach (string value in values)
        {
            if (!string.IsNullOrEmpty(value))
                target.Add(value);
        }
    }

    private string SerializeIntDictionary(Dictionary<string, int> dictionary)
    {
        List<string> entries = new List<string>();

        foreach (KeyValuePair<string, int> pair in dictionary)
        {
            entries.Add(pair.Key + "=" + pair.Value);
        }

        return string.Join(",", entries);
    }

    private void DeserializeIntDictionary(string data, Dictionary<string, int> target)
    {
        if (string.IsNullOrEmpty(data))
            return;

        string[] entries = data.Split(',');

        foreach (string entry in entries)
        {
            string[] parts = entry.Split('=');

            if (parts.Length != 2)
                continue;

            if (int.TryParse(parts[1], out int value))
            {
                target[parts[0]] = value;
            }
        }
    }

    private string SerializeFloatDictionary(Dictionary<string, float> dictionary)
    {
        List<string> entries = new List<string>();

        foreach (KeyValuePair<string, float> pair in dictionary)
        {
            string value = pair.Value.ToString(CultureInfo.InvariantCulture);
            entries.Add(pair.Key + "=" + value);
        }

        return string.Join(",", entries);
    }

    private void DeserializeFloatDictionary(string data, Dictionary<string, float> target)
    {
        if (string.IsNullOrEmpty(data))
            return;

        string[] entries = data.Split(',');

        foreach (string entry in entries)
        {
            string[] parts = entry.Split('=');

            if (parts.Length != 2)
                continue;

            if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                target[parts[0]] = value;
            }
        }
    }

    // ── Buscar niveles en el grafo ───────────────────────────────────────────

    private Level_Scriptable FindLevelByIDOrName(string levelIDOrName)
    {
        if (string.IsNullOrEmpty(levelIDOrName))
            return null;

        HashSet<string> visited = new HashSet<string>();
        Queue<Level_Scriptable> queue = new Queue<Level_Scriptable>();

        if (startingLevel != null)
            queue.Enqueue(startingLevel);

        while (queue.Count > 0)
        {
            Level_Scriptable level = queue.Dequeue();

            if (level == null)
                continue;

            if (visited.Contains(level.LevelID))
                continue;

            visited.Add(level.LevelID);

            if (level.LevelID == levelIDOrName || level.name == levelIDOrName)
                return level;

            if (level.Variants != null)
            {
                foreach (LevelVariant transition in level.Variants)
                {
                    if (transition != null && transition.NextLevel != null)
                        queue.Enqueue(transition.NextLevel);
                }
            }

            if (level.DefaultNextLevel != null)
                queue.Enqueue(level.DefaultNextLevel);
        }

        return null;
    }
}


