using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "Game/GameData")]
public class GameData : ScriptableObject
{
    public int currentLevelIndex;
    public Level_Scriptable currentLevel;
}
