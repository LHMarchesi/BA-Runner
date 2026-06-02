using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance;

    [SerializeField] private Level_Scriptable startingLevel;
    [SerializeField] private GameData GameData;

    public Level_Scriptable CurrentLevel { get; private set; }

    private readonly HashSet<string> _activeFlags = new HashSet<string>();

    // ── Flags ────────────────────────────────────────────────────────────────

    public void SetFlag(string flag) => _activeFlags.Add(flag);
    public void ClearFlag(string flag) => _activeFlags.Remove(flag);
    public bool HasFlag(string flag) => _activeFlags.Contains(flag);
    public void ClearAllFlags() => _activeFlags.Clear();
    public void ApplyChoice(ChoiceData choice)
    {
        foreach (var flag in choice.FlagsToSet) SetFlag(flag);
        foreach (var flag in choice.FlagsToClear) ClearFlag(flag);
    }

    // ── Progresión ───────────────────────────────────────────────────────────
    public void ResetProgress()
    {
        _activeFlags.Clear();
        CurrentLevel = startingLevel;
        PlayerPrefs.DeleteKey("CurrentLevel");
        PlayerPrefs.DeleteKey("ActiveFlags");
        PlayerPrefs.Save();
    }
    public void AdvanceLevel()
    {
        Level_Scriptable next = EvaluateNextLevel();

        if (next != null)
        {
            CurrentLevel = next;
            SaveProgress();
            GameManager.Instance.IsOutro = false;
            SceneManager.LoadScene("CinematicsScene");
            GameManager.Instance.ChangeState(GameState.Cinematic);
        }
        else
        {
            SceneManager.LoadScene("Credits");
            GameManager.Instance.ChangeState(GameState.Credits);
        }
    }

    private Level_Scriptable EvaluateNextLevel()
    {
        Debug.Log($"[Progression] Evaluando desde: {CurrentLevel.name}");

        foreach (var transition in CurrentLevel.Variants)
        {
            bool match = AllFlagsActive(transition.RequiredFlags);
            Debug.Log($"  Transition flags: [{string.Join(", ", transition.RequiredFlags)}] → match: {match}");
            if (match)
            {
                Debug.Log($"  → Eligiendo variante: {transition.NextLevel.name}");
                return transition.NextLevel;
            }
        }

        Debug.Log($"  → Sin match, usando default: {CurrentLevel.DefaultNextLevel?.name ?? "NULL (Credits)"}");
        return CurrentLevel.DefaultNextLevel;
    }

    private bool AllFlagsActive(List<string> flags)
    {
        foreach (var f in flags)
            if (!_activeFlags.Contains(f)) return false;
        return true;
    }

    // ── Persistencia ─────────────────────────────────────────────────────────

    public void SaveProgress()
    {
        PlayerPrefs.SetString("CurrentLevel", CurrentLevel.name);
        PlayerPrefs.SetString("ActiveFlags", string.Join(",", _activeFlags));
        PlayerPrefs.Save();
    }

    public void LoadProgress()
    {
        _activeFlags.Clear();

        if (PlayerPrefs.HasKey("ActiveFlags"))
        {
            var saved = PlayerPrefs.GetString("ActiveFlags");
            if (!string.IsNullOrEmpty(saved))
                foreach (var f in saved.Split(','))
                    _activeFlags.Add(f);
        }

        // Busca el nivel guardado entre todos los levels del grafo
        if (PlayerPrefs.HasKey("CurrentLevel"))
        {
            string savedName = PlayerPrefs.GetString("CurrentLevel");
            CurrentLevel = FindLevelByName(savedName) ?? startingLevel;
        }
        else
        {
            CurrentLevel = startingLevel;
        }
    }

    // Recorre el grafo desde startingLevel para encontrar el level guardado
    private Level_Scriptable FindLevelByName(string levelName)
    {
        var visited = new HashSet<string>();
        var queue = new Queue<Level_Scriptable>();
        queue.Enqueue(startingLevel);

        while (queue.Count > 0)
        {
            var level = queue.Dequeue();
            if (level == null || visited.Contains(level.name)) continue;
            visited.Add(level.name);

            if (level.name == levelName) return level;

            foreach (var t in level.Variants) queue.Enqueue(t.NextLevel);
            queue.Enqueue(level.DefaultNextLevel);
        }

        return null;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
        }
        else Destroy(gameObject);
    }
}