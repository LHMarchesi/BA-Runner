using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level_Scriptable", menuName = "Scriptable Objects/Level_Scriptable")]
public class Level_Scriptable : ScriptableObject
{
    [Header("Flag Variants")]
    public List<LevelVariant> Variants = new List<LevelVariant>();
    public Level_Scriptable DefaultNextLevel;

    [Header("Gameplay")]
    public SpawnPattern[] levelPatterns;
    public float maxLevelProgession;
    public float timeBetweenWaves;
    public AudioClip levelMusic;
    public Sprite levelBackground;
    public Sprite winLevelImage;
    public SpeedData speedData;

    [Header("Cinematics")]
    public RuntimeDialogueGraph introDialogueGraph;
    public RuntimeDialogueGraph outroDialogueGraph;

    [Header("Scenes")]
    public string cinematicScene; 
    public string gameplayScene;
}

[System.Serializable]
public class LevelVariant
{
    [Tooltip("Todas deben estar activas (AND)")]
    public List<string> RequiredFlags = new List<string>();
    public Level_Scriptable NextLevel;
}