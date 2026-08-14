using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class StarRequirement
{
    [Range(1, 5)]
    public int stars;

    public float maxTime;
}
[CreateAssetMenu(fileName = "Level_Scriptable", menuName = "Scriptable Objects/Level_Scriptable")]
public class Level_Scriptable : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string levelID;
    public string LevelID
    {
        get
        {
            return string.IsNullOrEmpty(levelID) ? name : levelID;
        }
    }

    [Header("Flag Variants")]
    public List<LevelVariant> Variants = new List<LevelVariant>();
    public Level_Scriptable DefaultNextLevel;
    [Header("Gameplay")]
    public float maxLevelProgession;
    public AudioClip levelMusic;
    public Sprite winLevelImage;

    [Header("Level Stages")]
    public List<LevelStage> stages = new();

    [Header("Star Requirement")]
    public StarRequirement[] starRequirements;
    public int GetStarsForTime(float completionTime)
    {
        if (starRequirements == null || starRequirements.Length == 0)
            return 1;

        int bestStars = 0;

        foreach (StarRequirement requirement in starRequirements)
        {
            if (requirement == null)
                continue;

            if (completionTime <= requirement.maxTime)
            {
                bestStars = Mathf.Max(bestStars, requirement.stars);
            }
        }

        if (bestStars <= 0)
            bestStars = 1;

        return Mathf.Clamp(bestStars, 1, 5);
    }
    [Header("Cinematics")]
    public RuntimeDialogueGraph introDialogueGraph;
    public RuntimeDialogueGraph outroDialogueGraph;

    [Header("Audio")]
    public AudioClip introMusic;
    public AudioClip outroMusic;
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

    public EnvironmentPreset environment;
}


[System.Serializable]
public class RoadSection
{
    public string stageName;

    public float progressionRequired;

    public SpeedData worldSpeedData;

    public WaveConfig[] waveConfig;

    public EnvironmentPreset environment;

    public int displayedSpeed;
}


[System.Serializable]
public class EnvironmentPreset
{
    public Material background;
    public Material parallaxLayer1;
    public Material parallaxLayer2;
    public Material parallaxLayer3;
}
