using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Level_Scriptable", menuName = "Scriptable Objects/Level_Scriptable")]
public class Level_Scriptable : ScriptableObject
{
    [Header("Flag Variants")]
    public List<LevelVariant> Variants = new List<LevelVariant>();
    public Level_Scriptable DefaultNextLevel;

    [Header("Gameplay")]
    public float maxLevelProgession;
    public AudioClip levelMusic;
    public Sprite levelBackground;
    public Sprite winLevelImage;

    [Header("Level Stages")]
    public List<LevelStage> stages = new();

    [Header("Cinematics")]
    public RuntimeDialogueGraph introDialogueGraph;
    public RuntimeDialogueGraph outroDialogueGraph;
}

[System.Serializable]
public class LevelVariant
{
    [Tooltip("Todas deben estar activas (AND)")]
    public List<string> RequiredFlags = new List<string>();
    public Level_Scriptable NextLevel;
}

[System.Serializable]
public class LevelStage
{
    [Header("Stage Info")]
    public string stageName;

    [Header("Stage SpeedData")]
    public SpeedData speedData;

    [Header("Stage Patterns")]
    public WaveConfig waveConfig;

    [Range(0, 1)]
    public float progressionRequired;

    [Header("Sign")]
    public int displayedSpeed;
}


[System.Serializable]
public class RoadSection
{
    public string stageName;

    public float progressionRequired;

    public SpeedData worldSpeedData;

    public WaveConfig waveConfig;

    public EnvironmentPreset environment;

    public int displayedSpeed;
}


[System.Serializable]
public class EnvironmentPreset
{
    public Sprite background;
}
