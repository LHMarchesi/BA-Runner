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
     public float changeDistance;
    public float laneChangeSpeed;
}
