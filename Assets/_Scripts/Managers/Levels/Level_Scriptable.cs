using UnityEngine;

[CreateAssetMenu(fileName = "Level_Scriptable", menuName = "Scriptable Objects/Level_Scriptable")]
public class Level_Scriptable : ScriptableObject
{
    public SpawnPattern[] levelPatterns;
    public int levelIndex;
    public float maxLevelProgession;
    public float timeBetweenWaves;

    public AudioClip levelMusic;
}
