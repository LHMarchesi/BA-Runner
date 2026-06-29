using UnityEngine;

[CreateAssetMenu(fileName = "WaveConfig", menuName = "Scriptable Objects/WaveConfig")]
public class WaveConfig : ScriptableObject
{
    public SpawnPattern[] patterns;
    public bool randomizeOrder;
    public float distanceBetweenWaves;
}
