using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    [Header("Score")]
    public int Score { get; private set; }
    public float Distance { get; private set; }
    public float Stage { get; private set; }

    private bool isRunning;

    private EventBinding<OnRoadSectionChanged> sectionBinding;

    EventBinding<OnPlayerDeathEvent> playerDeathBinding;

    [SerializeField]
    private bool useGlobalEvents = true;

    [SerializeField]
    private bool broadcastLevelUpdate = true;

    private void OnEnable()
    {
        isRunning = true;

        if (!useGlobalEvents)
            return;

        sectionBinding =
            new EventBinding<OnRoadSectionChanged>(
                OnRoadSectionChanged
            );

        EventBus<OnRoadSectionChanged>
            .Register(sectionBinding);

        playerDeathBinding =
            new EventBinding<OnPlayerDeathEvent>(
                HandlePLayerDeath
            );

        EventBus<OnPlayerDeathEvent>
            .Register(playerDeathBinding);
    }

    private void HandlePLayerDeath(OnPlayerDeathEvent e)
    {
        isRunning = false;
    }
    private void OnDisable()
    {
        if (!useGlobalEvents)
            return;

        EventBus<OnRoadSectionChanged>
            .Deregister(sectionBinding);

        EventBus<OnPlayerDeathEvent>
            .Deregister(playerDeathBinding);
    }


    private void Update()
    {
        if (!broadcastLevelUpdate)
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
        Stage++;
    }

    public void ResetScore()
    {
        Score = 0;
        Distance = 0f;
        Stage = 0f;
        isRunning = true;
    }

    public void SetRunnig(bool value)
    {
        isRunning = value;
    }

    private void OnRoadSectionChanged(OnRoadSectionChanged e)
    {
        Stage++;
    }


    public void AddDistance(float value)
    {
        if (!isRunning)
            return;
        Distance += value;
        Score = Mathf.FloorToInt(Distance * 10f);
    }

}
