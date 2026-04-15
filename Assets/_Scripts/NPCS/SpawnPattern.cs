using UnityEngine;

[CreateAssetMenu(fileName = "SpawnPattern", menuName = "Game/Spawn Pattern")]
public class SpawnPattern : ScriptableObject
{
    [System.Serializable]
    public struct SpawnData
    {
        public int laneIndex;
        public float delay;
    }

    public SpawnData[] spawns;
}
