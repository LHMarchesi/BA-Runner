using UnityEngine;

[CreateAssetMenu(fileName = "ObstacleConfig", menuName = "Obstacles/Obstacle Config")]
public class ObstacleConfig : ScriptableObject
{
    public Obstacle prefab;
    public int defaultPoolSize;
    public float despawnXThreshold;
    public enum SpawnSide
    {
        Front,
        Rear
    }

    public SpawnSide spawnSide;
}
