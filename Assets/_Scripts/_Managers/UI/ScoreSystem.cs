using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    [Header("Score")]
    public int Score { get; private set; }
    public float Distance { get; private set; }
    public float Stage { get; private set; }

    private bool isRunning;

    private EventBinding<OnRoadSectionChanged> sectionBinding;
    private EventBinding<OnPlayerDeathEvent> playerDeathBinding;

    [SerializeField]
    private bool useGlobalEvents = true;

    [SerializeField]
    private bool broadcastLevelUpdate = true;

    public bool IsRunning => isRunning;

    private void OnEnable()
    {
        isRunning = true;

        if (!useGlobalEvents)
            return;

        sectionBinding = new EventBinding<OnRoadSectionChanged>(OnRoadSectionChanged);
        EventBus<OnRoadSectionChanged>.Register(sectionBinding);

        playerDeathBinding = new EventBinding<OnPlayerDeathEvent>(HandlePlayerDeath);
        EventBus<OnPlayerDeathEvent>.Register(playerDeathBinding);
    }

    private void HandlePlayerDeath(OnPlayerDeathEvent e)
    {
        SetRunning(false);
    }

    private void OnDisable()
    {
        SetRunning(false);

        if (!useGlobalEvents)
            return;

        if (sectionBinding != null)
            EventBus<OnRoadSectionChanged>.Deregister(sectionBinding);

        if (playerDeathBinding != null)
            EventBus<OnPlayerDeathEvent>.Deregister(playerDeathBinding);
    }

    private void Update()
    {
        // Important: do not keep broadcasting stale/zero values after the run stops.
        // SurvivalUIManager listens to this global event and otherwise can overwrite
        // the final result that SurvivalManager has just displayed.
        if (!broadcastLevelUpdate || !isRunning)
            return;

        EventBus<OnLevelUpdateEvent>.Raise(
            new OnLevelUpdateEvent
            {
                score = Score,
                distance = Distance,
                loops = Stage
            }
        );
    }

    public void AddStage()
    {
        if (!isRunning)
            return;

        Stage++;
    }

    public void ResetScore()
    {
        Score = 0;
        Distance = 0f;
        Stage = 0f;
        isRunning = true;
    }

    /// <summary>Stops or resumes score accumulation and HUD event broadcasting.</summary>
    public void SetRunning(bool value)
    {
        isRunning = value;
    }

    /// <summary>Backward-compatible spelling used by older project scripts.</summary>
    public void SetRunnig(bool value)
    {
        SetRunning(value);
    }

    private void OnRoadSectionChanged(OnRoadSectionChanged e)
    {
        if (!isRunning)
            return;

        Stage++;
    }

    public void AddDistance(float value)
    {
        if (!isRunning || value <= 0f)
            return;

        Distance += value;
        Score = Mathf.FloorToInt(Distance * 10f);
    }
}
