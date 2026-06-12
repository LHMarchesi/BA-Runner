using UnityEngine;

[CreateAssetMenu(fileName = "SpawnPattern", menuName = "Game/Spawn Pattern")]
public class SpawnPattern : ScriptableObject
{
    [System.Serializable]
    public struct SpawnEntry
    {
        public ObstacleConfig obstacleConfig;

        public int laneIndex;
        public float delay;

        public Vector2 positionOffset;
    }
   

    public SpawnEntry[] spawns;
}