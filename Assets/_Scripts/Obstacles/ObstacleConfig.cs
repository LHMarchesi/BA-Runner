using UnityEngine;

[CreateAssetMenu(fileName = "ObstacleConfig", menuName = "Game/Obstacle Config")]
public class ObstacleConfig : ScriptableObject
{
    public Obstacle prefab;
    public int defaultPoolSize;
    public float despawnXThreshold;
}

[CreateAssetMenu(fileName = "ChangingLaneObstacleConfig", menuName = "Game/Obstacle Config/Changing Lane")]

public class ChangingLaneObstacleConfig : ObstacleConfig
{
    public enum LaneChangeDirection
    {
        Up,
        Down,
    }
    [Header("Lane Change")]
    public LaneChangeDirection changeDirection;          
    [Range(0, 5)] public float changeDelay;
    public float laneChangeSpeed;
}   