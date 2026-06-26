using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform[] frontLanes;
    [SerializeField] private Transform[] rearLanes;

    [SerializeField] WorldSpeed worldSpeed;
    
    private WaveConfig currentWaveConfig;
    private MultiTypePool pool;
    private Coroutine spawnCoroutine;

    private int sequentialPatternIndex;

    private EventBinding<OnLevelStartEvent> onLevelStart;
    private EventBinding<OnLevelCompletedEvent> onLevelCompleted;
    private EventBinding<OnRoadStageChanged> onStageChanged;
    private EventBinding<OnRoadSectionChanged> onSectionChanged;

    private void OnEnable()
    {
        onLevelStart = new EventBinding<OnLevelStartEvent>(HandleLevelStart);
        onLevelCompleted = new EventBinding<OnLevelCompletedEvent>(HandleLevelCompleted);
        onStageChanged = new EventBinding<OnRoadStageChanged>(HandleStageChanged);
        onSectionChanged = new EventBinding<OnRoadSectionChanged>(HandleRoadSectionChanged);

        EventBus<OnLevelStartEvent>.Register(onLevelStart);
        EventBus<OnLevelCompletedEvent>.Register(onLevelCompleted);
        EventBus<OnRoadSectionChanged>.Register(onSectionChanged);
        EventBus<OnRoadStageChanged>.Register(onStageChanged);
    }

    // ── Handlers de evento ──────────────────────────────────────────────

    private void HandleLevelStart(OnLevelStartEvent e)
    {
        StopSpawning();
        sequentialPatternIndex = 0;
        StartSpawning();
    }

    private void Start()
    {
        StopSpawning();
        sequentialPatternIndex = 0;
        StartSpawning();
    }


    private void HandleRoadSectionChanged(OnRoadSectionChanged e)
    {
        sequentialPatternIndex = 0;
        currentWaveConfig = e.roadSection.waveConfig;

        Prewarm(currentWaveConfig);
    }

    private void HandleStageChanged(OnRoadStageChanged e)
    {
        sequentialPatternIndex = 0;
        currentWaveConfig = e.stage.waveConfig;
        Prewarm(currentWaveConfig);
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

    private void Prewarm(WaveConfig waveConfig)
    {
        if (waveConfig == null)
            return;

        foreach (var pattern in waveConfig.patterns)
        {
            if (pattern == null)
                continue;

            foreach (var entry in pattern.spawns)
            {
                if (entry.obstacleConfig != null)
                {
                    pool.Prewarm(
                        entry.obstacleConfig,
                        worldSpeed);
                }
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
            if (currentWaveConfig == null ||
                currentWaveConfig.patterns == null ||
                currentWaveConfig.patterns.Length == 0)
            {
                yield return null;
                continue;
            }

            SpawnPattern pattern =
                PickPattern(currentWaveConfig);

            yield return ExecutePattern(pattern);

            yield return new WaitForSeconds(
                currentWaveConfig.timeBetweenWaves);
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
        Transform[] lanes = entry.obstacleConfig.spawnSide == ObstacleConfig.SpawnSide.Front ? frontLanes : rearLanes;

        if (entry.laneIndex < 0 || entry.laneIndex >= lanes.Length)
        {
            Debug.LogError(
                $"LaneIndex inválido.\n" +
                $"Side: {entry.obstacleConfig.spawnSide}\n" +
                $"Index: {entry.laneIndex}\n" +
                $"Lanes: {lanes.Length}"
            );
            return;
        }
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
        EventBus<OnRoadSectionChanged>.Deregister(onSectionChanged);
        EventBus<OnRoadStageChanged>.Deregister(onStageChanged);
        StopSpawning();
    }
}
