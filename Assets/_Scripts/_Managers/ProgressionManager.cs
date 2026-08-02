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

    public bool RegisterLevelCompletionTime(
        Level_Scriptable level,
        float completionTime)
    {
        if (level == null || completionTime <= 0f)
            return false;

        _pendingCompletionTime = completionTime;

        string key = GetBestTimeKey(level);

        int newTimeMilliseconds =
            Mathf.RoundToInt(completionTime * 1000f);

        bool hasPreviousTime =
            PlayerPrefs.HasKey(key);

        bool isNewRecord = true;

        if (hasPreviousTime)
        {
            int previousTimeMilliseconds =
                PlayerPrefs.GetInt(key);

            isNewRecord =
                newTimeMilliseconds <
                previousTimeMilliseconds;
        }

        if (isNewRecord)
        {
            PlayerPrefs.SetInt(
                key,
                newTimeMilliseconds
            );

            PlayerPrefs.Save();
        }

        return isNewRecord;
    }

    public bool TryGetBestLevelTime(
    Level_Scriptable level,
    out float bestTime
)
    {
        bestTime = 0f;

        if (level == null)
            return false;

        string key = GetBestTimeKey(level);

        if (!PlayerPrefs.HasKey(key))
            return false;

        int milliseconds =
            PlayerPrefs.GetInt(key);

        bestTime =
            milliseconds / 1000f;

        return true;
    }

    private string GetBestTimeKey(
        Level_Scriptable level
    )
    {
        return BestTimesKey + level.LevelID;
    }
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

    public void ApplyChoice(
     ChoiceData selectedChoice,
     IReadOnlyList<ChoiceData> siblingChoices = null,
     bool choicesAreExclusive = false
 )
    {
        if (selectedChoice == null)
            return;

        /*
         * Primero eliminamos las flags que pertenecen a las demás
         * opciones de esta misma decisión.
         */
        if (choicesAreExclusive && siblingChoices != null)
        {
            foreach (ChoiceData siblingChoice in siblingChoices)
            {
                if (siblingChoice == null || siblingChoice.FlagsToSet == null)
                    continue;

                foreach (string rawFlag in siblingChoice.FlagsToSet)
                {
                    string flag = NormalizeFlag(rawFlag);

                    if (string.IsNullOrEmpty(flag))
                        continue;

                    bool removed = _activeFlags.Remove(flag);

                    if (removed)
                    {
                        Debug.Log(
                            $"[Flags] Eliminada por elección exclusiva: {flag}"
                        );
                    }
                }
            }
        }

        /*
         * Después aplicamos las eliminaciones específicas configuradas
         * manualmente en FlagsToClear.
         */
        if (selectedChoice.FlagsToClear != null)
        {
            foreach (string rawFlag in selectedChoice.FlagsToClear)
            {
                string flag = NormalizeFlag(rawFlag);

                if (string.IsNullOrEmpty(flag))
                    continue;

                _activeFlags.Remove(flag);
            }
        }

        /*
         * Finalmente activamos las flags de la opción seleccionada.
         * Se hace al final para garantizar que la elección actual gane.
         */
        if (selectedChoice.FlagsToSet != null)
        {
            foreach (string rawFlag in selectedChoice.FlagsToSet)
            {
                string flag = NormalizeFlag(rawFlag);

                if (string.IsNullOrEmpty(flag))
                    continue;

                _activeFlags.Add(flag);

                Debug.Log($"[Flags] Activada: {flag}");
            }
        }

        DiscoverSelectedVariant();
        SaveProgress();

        Debug.Log(
            $"[Flags] Flags activas después de elegir " +
            $"'{selectedChoice.ChoiceText}': " +
            $"{(_activeFlags.Count > 0 ? string.Join(", ", _activeFlags) : "(ninguna)")}"
        );
    }

    private string NormalizeFlag(string flag)
    {
        return string.IsNullOrWhiteSpace(flag)
            ? string.Empty
            : flag.Trim();
    }

    private void DiscoverSelectedVariant()
    {
        if (CurrentLevel == null || CurrentLevel.Variants == null)
            return;

        foreach (LevelVariant variant in CurrentLevel.Variants)
        {
            if (variant == null || variant.NextLevel == null)
                continue;

            // No descubrimos variantes sin requisitos desde una elección.
            // Esto evita desbloquear accidentalmente una transición genérica.
            if (variant.RequiredFlags == null ||
                variant.RequiredFlags.Count == 0)
            {
                continue;
            }

            if (!AllFlagsActive(variant.RequiredFlags))
                continue;

            DiscoverLevel(variant.NextLevel);

            return;
        }
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
        if (level == null || string.IsNullOrEmpty(level.LevelID))
            return;

        bool wasAdded = _discoveredLevelIDs.Add(level.LevelID);

        if (!wasAdded)
            return;
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

    public void CompleteLevel(
       Level_Scriptable level,
       float completionTime
   )
    {
        if (level == null)
        {
            Debug.LogError(
                "[Progression] No se puede completar un nivel null."
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(level.LevelID))
        {
            Debug.LogError(
                $"[Progression] El nivel {level.name} no tiene LevelID."
            );

            return;
        }

        string levelID = level.LevelID;

        /*
         * Aunque sea un replay sin tiempo, conservamos el nivel
         * como completado y descubierto.
         */
        _completedLevelIDs.Add(levelID);
        _discoveredLevelIDs.Add(levelID);

        bool hasValidCompletionTime =
            completionTime > 0f &&
            !float.IsNaN(completionTime) &&
            !float.IsInfinity(completionTime);

        if (!hasValidCompletionTime)
        {
            Debug.Log(
                $"[Progression] {level.name} se está avanzando sin " +
                $"registrar un tiempo nuevo. Valor: {completionTime}. " +
                "Se conservarán el tiempo y las estrellas anteriores."
            );

            SaveProgress();
            return;
        }

        int earnedStars =
            level.GetStarsForTime(completionTime);

        if (!_bestStarsByLevelID.TryGetValue(
                levelID,
                out int previousStars
            ) ||
            earnedStars > previousStars)
        {
            _bestStarsByLevelID[levelID] =
                earnedStars;
        }

        if (!_bestTimeByLevelID.TryGetValue(
                levelID,
                out float previousTime
            ) ||
            completionTime < previousTime)
        {
            _bestTimeByLevelID[levelID] =
                completionTime;
        }

        SaveProgress();

        Debug.Log(
            $"[Progression] Nivel completado: {level.name} | " +
            $"Tiempo: {completionTime:F2} | " +
            $"Estrellas: {earnedStars}"
        );
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
        Debug.Log(
            $"[Progression] AdvanceLevel iniciado | " +
            $"CurrentLevel: {CurrentLevel?.name ?? "NULL"} | " +
            $"PendingTime: {_pendingCompletionTime}"
        );

        if (CurrentLevel == null)
        {
            CurrentLevel = startingLevel;
            DiscoverStartingLevelIfNeeded();
        }

        if (CurrentLevel == null)
        {
            Debug.LogError(
                "[Progression] No existe CurrentLevel ni startingLevel."
            );

            return;
        }

        Level_Scriptable completedLevel = CurrentLevel;

        /*
         * En un replay puede no existir un tiempo pendiente.
         * Eso no debe impedir evaluar la elección y avanzar.
         */
        float completionTime = _pendingCompletionTime;
        _pendingCompletionTime = -1f;

        CompleteLevel(
            completedLevel,
            completionTime
        );

        Debug.Log(
            $"[Progression] Evaluando el siguiente nivel desde " +
            $"{completedLevel.name}."
        );

        Level_Scriptable next =
            EvaluateNextLevel();

        Debug.Log(
            $"[Progression] Resultado de EvaluateNextLevel: " +
            $"{next?.name ?? "NULL"}"
        );

        if (next == null)
        {
            SaveProgress();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(
                    GameState.Credits
                );
            }

            if (string.IsNullOrWhiteSpace(creditsSceneName))
            {
                Debug.LogError(
                    "[Progression] creditsSceneName está vacío."
                );

                return;
            }

            SceneManager.LoadScene(
                creditsSceneName
            );

            return;
        }

        DiscoverLevel(next);

        CurrentLevel = next;
        SaveProgress();

        Debug.Log(
            $"[Progression] CurrentLevel actualizado a: " +
            $"{CurrentLevel.name}"
        );

        if (GameManager.Instance != null)
        {
            GameManager.Instance.IsOutro = false;

            GameManager.Instance.ChangeState(
                GameState.Cinematic
            );
        }

        if (string.IsNullOrWhiteSpace(cinematicsSceneName))
        {
            Debug.LogError(
                "[Progression] cinematicsSceneName está vacío."
            );

            return;
        }

        Debug.Log(
            $"[Progression] Cargando escena: " +
            $"{cinematicsSceneName}"
        );

        SceneManager.LoadScene(
            cinematicsSceneName
        );
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
        /*
         * Una variante sin requisitos no debe considerarse una coincidencia.
         * Para una ruta sin condiciones se debe usar DefaultNextLevel.
         */
        if (flags == null || flags.Count == 0)
            return false;

        foreach (string rawFlag in flags)
        {
            /*
             * NormalizeFlag elimina espacios al principio y al final.
             * Por ejemplo:
             * " NuncaMeDetuveAPensarlo "
             * pasa a:
             * "NuncaMeDetuveAPensarlo"
             */
            string flag = NormalizeFlag(rawFlag);

            /*
             * Una entrada vacía dentro de RequiredFlags indica
             * una configuración inválida.
             */
            if (string.IsNullOrEmpty(flag))
            {
                Debug.LogWarning(
                    "[Progression] Se encontró una RequiredFlag vacía."
                );

                return false;
            }

            if (!_activeFlags.Contains(flag))
            {
                return false;
            }
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


