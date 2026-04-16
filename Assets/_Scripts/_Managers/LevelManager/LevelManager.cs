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
    [SerializeField] private ProgessBar progessBar;
    private float levelProgession;

    public static Action OnLevelComplete;
    private Level_Scriptable CurrentLevel => GameManager.Instance.CurrentLevel;
    private SpeedData SpeedData => GameManager.Instance.CurrentLevel.speedData;

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

            var speedData = SpeedData;
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
        if (CurrentLevel.levelIndex < GameManager.Instance.levels.Count)
        {
            LoadLevel(CurrentLevel.levelIndex);
        }
        else
        {
            Debug.Log("Juego completado");
            //GameManager.Instance.ChangeState(GameState.Credits);
        }
    }
}
