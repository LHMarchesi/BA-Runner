using UnityEngine;

[CreateAssetMenu(fileName = "ChangingLaneObstacleConfig", menuName = "Obstacles/Changing Lane")]

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
