using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

[CreateAssetMenu(fileName = "Level_Scriptable", menuName = "Scriptable Objects/Level_Scriptable")]
public class Level_Scriptable : ScriptableObject
{
    public SpawnPattern[] levelPatterns;
    public int levelIndex;
    public float maxLevelProgession;
    public float timeBetweenWaves;

    public string cinematicScene; 
    public string gameplayScene;
    public AudioClip levelMusic;
}
