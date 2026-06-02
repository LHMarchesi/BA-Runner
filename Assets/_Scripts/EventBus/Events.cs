public interface IEvent { }

public struct OnLevelCompletedEvent : IEvent
{
    public int stars;
    public float completionTime;
}

public struct OnPlayerDeathEvent : IEvent
{
}

public struct OnLevelStartEvent : IEvent
{
    public SpeedData levelSpeedData;
    public int levelIndex;
}

public struct OnLevelUpdateEvent : IEvent
{
    public float levelProgession;
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