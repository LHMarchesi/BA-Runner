using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    private void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }
    }
    [SerializeField] public List<Level_Scriptable> levels;
    [SerializeField] private ProgessBar progessBar;
    private float levelProgession;
    public static Action OnLevelComplete;

    public Level_Scriptable CurrentLevel
    {
        get
        {
            if (levels == null || levels.Count == 0) return null;
            if (GameManager.Instance.gameData.currentLevelIndex < 0 || GameManager.Instance.gameData.currentLevelIndex >= levels.Count)
                GameManager.Instance.gameData.currentLevelIndex = 0; // Reset to 0 if out of bounds
            return levels[GameManager.Instance.gameData.currentLevelIndex];
        }
    }
    private void Start()
    {
        levelProgession = 0;
    }

    void LoadLevel(int index)
    {
        levelProgession = 0;
        progessBar.UpdateProgess(levelProgession, CurrentLevel.maxLevelProgession);
    }

    public void IncreaseLevelProgession()
    {
        var currentLevel = CurrentLevel;
        if (currentLevel == null) return;

        if (levelProgession < currentLevel.maxLevelProgession)
        {
            levelProgession += Time.deltaTime;

            if (progessBar != null) progessBar.UpdateProgess(levelProgession, currentLevel.maxLevelProgession);

            var speedData = currentLevel.speedData;
            if (speedData != null)
            {
                float normalized = levelProgession / currentLevel.maxLevelProgession;
                speedData.currentProgressionMultiplier = Mathf.Lerp(speedData.minProgressionMultiplier, speedData.maxProgressionMultiplier, normalized);
            }
        }
        else
        {
            GameManager.Instance.ChangeState(GameState.Win);
            OnLevelComplete?.Invoke();
            NextLevel();
        }
    }
   

    void NextLevel()
    {
        int nextIndex = GameManager.Instance.gameData.currentLevelIndex + 1;

        if (nextIndex < levels.Count)
        {
            GameManager.Instance.gameData.currentLevelIndex = nextIndex;
            LoadLevel(nextIndex);
            GameManager.Instance.SaveProgress(nextIndex);
        }
        else
        {
            Debug.Log("Juego completado");
        }
    }
}
