using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "Level_Scriptable", menuName = "Scriptable Objects/Level_Scriptable")]
public class Level_Scriptable : ScriptableObject
{
    [Header("Gameplay")]
    public SpawnPattern[] levelPatterns;
    public int levelIndex;
    public float maxLevelProgession;
    public float timeBetweenWaves;
    public AudioClip levelMusic;
    public Sprite levelBackground;
    public SpeedData speedData;

    [Header("Cinematics")]
    public string introURL;
    public string outroURL;

    [Header("Scenes")]
    public string cinematicScene; 
    public string gameplayScene;
}
