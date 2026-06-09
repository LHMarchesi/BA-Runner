using UnityEngine;

[CreateAssetMenu(fileName = "ObstacleConfig", menuName = "Game/Obstacle Config")]
public class ObstacleConfig : ScriptableObject
{
    public Obstacle prefab;
    public int defaultPoolSize;
    public float despawnXThreshold;
}