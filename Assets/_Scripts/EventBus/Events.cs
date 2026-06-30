public interface IEvent { }

public struct OnLevelCompletedEvent : IEvent
{
    public int stars;
    public float completionTime;
}

public struct OnPlayerDeathEvent : IEvent
{
    public float score;
    public float distance;
    public float loops;
}

public struct OnLevelStartEvent : IEvent
{
    public SpeedData levelSpeedData;
    public int levelIndex;
}

public struct OnLevelUpdateEvent : IEvent
{
    public float levelProgession;
    public float score;
    public float distance;
    public float loops;
}

public struct OnEnterMenuEvent : IEvent
{
}

public struct OnEnterCinematics : IEvent
{
}

public struct OnRoadStageChanged : IEvent
{
    public LevelStage stage;
}

public struct OnRoadEnvironmentChanged : IEvent
{
    public EnvironmentPreset environmentPreset;
}


public struct OnRoadSectionChanged : IEvent
{
    public RoadSection roadSection;
}

public struct OnWaveConfigChanged : IEvent
{
    public WaveConfig[] waveConfigs;
}