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
}