using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform[] lanes; // asignar 4 en inspector

    [SerializeField] WorldSpeed worldSpeed;
    [SerializeField] LevelManager levelManager;

    private MultiTypePool pool;
    private Coroutine spawnCoroutine;

    private int sequentialPatternIndex;

    private EventBinding<OnLevelStartEvent> onLevelStart;
    private EventBinding<OnLevelCompletedEvent> onLevelCompleted;
    private EventBinding<OnRoadStageChanged> onStageChanged;

    private void OnEnable()
    {
        onLevelStart = new EventBinding<OnLevelStartEvent>(HandleLevelStart);
        onLevelCompleted = new EventBinding<OnLevelCompletedEvent>(HandleLevelCompleted);
        onStageChanged = new EventBinding<OnRoadStageChanged>(HandleStageChanged);

        EventBus<OnLevelStartEvent>.Register(onLevelStart);
        EventBus<OnLevelCompletedEvent>.Register(onLevelCompleted);
        EventBus<OnRoadStageChanged>.Register(onStageChanged);
    }

    // ── Handlers de evento ──────────────────────────────────────────────

    private void HandleLevelStart(OnLevelStartEvent e)
    {
        StopSpawning();
        sequentialPatternIndex = 0;
        PrewarmForStage(levelManager.CurrentStage);
        StartSpawning();
    }

    private void HandleStageChanged(OnRoadStageChanged e)
    {
        sequentialPatternIndex = 0;
        PrewarmForStage(e.stage);
    }

    private void HandleLevelCompleted(OnLevelCompletedEvent e)
    {
        StopSpawning();
    }

    private void Awake()
    {
        pool = GetComponent<MultiTypePool>();
    }

    // ── Prewarming ──────────────────────────────────────────────────────

    private void PrewarmForStage(LevelStage stage)
    {
        var waveConfig = stage?.waveConfig;
        if (waveConfig == null) return;

        foreach (var pattern in waveConfig.patterns)
        {
            if (pattern == null) continue;

            foreach (var entry in pattern.spawns)
            {
                if (entry.obstacleConfig != null)
                    pool.Prewarm(entry.obstacleConfig, worldSpeed);
            }
        }
    }

    // ── Spawn loop ──────────────────────────────────────────────────────

    private void StartSpawning()
    {
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    private void StopSpawning()
    {
        if (spawnCoroutine == null) return;
        StopCoroutine(spawnCoroutine);
        spawnCoroutine = null;
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            // Leer la stage activa en cada iteración para capturar cambios.
            var waveConfig = levelManager.CurrentStage?.waveConfig;

            if (waveConfig == null || waveConfig.patterns == null || waveConfig.patterns.Length == 0)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            SpawnPattern pattern = PickPattern(waveConfig);
            yield return StartCoroutine(ExecutePattern(pattern));

            yield return new WaitForSeconds(waveConfig.timeBetweenWaves);
        }
    }

    /// <summary>
    /// Selecciona el siguiente patrón según el modo configurado en WaveConfig.
    /// </summary>
    private SpawnPattern PickPattern(WaveConfig waveConfig)
    {
        if (waveConfig.randomizeOrder)
            return waveConfig.patterns[Random.Range(0, waveConfig.patterns.Length)];

        // Modo secuencial: avanza en orden y hace wrap al final.
        SpawnPattern pattern = waveConfig.patterns[sequentialPatternIndex % waveConfig.patterns.Length];
        sequentialPatternIndex++;
        return pattern;
    }

    /// <summary>
    /// Ejecuta un patrón completo: recorre sus SpawnEntry y espera el delay de cada uno.
    /// </summary>
    private IEnumerator ExecutePattern(SpawnPattern pattern)
    {
        foreach (var entry in pattern.spawns)
        {
            if (entry.delay > 0f)
                yield return new WaitForSeconds(entry.delay);

            SpawnObstacle(entry);
        }
    }

    // ── Spawn de un obstacle individual ────────────────────────────────

    private void SpawnObstacle(SpawnPattern.SpawnEntry entry)
    {
        Obstacle obstacle = pool.Get(entry.obstacleConfig, worldSpeed);
        Transform lane = lanes[entry.laneIndex];

        obstacle.transform.SetParent(lane, worldPositionStays: false);
        obstacle.transform.localPosition = Vector3.zero;

        ObstacleConfig capturedConfig = entry.obstacleConfig;
        obstacle.OnDespawn = () => pool.Return(capturedConfig, obstacle);
      
        obstacle.gameObject.SetActive(true);
    }
    private void OnDisable()
    {
        EventBus<OnLevelStartEvent>.Deregister(onLevelStart);
        EventBus<OnLevelCompletedEvent>.Deregister(onLevelCompleted);
        EventBus<OnRoadStageChanged>.Deregister(onStageChanged);
        StopSpawning();
    }
}
