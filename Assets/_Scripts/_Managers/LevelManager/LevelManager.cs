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
        if (levelProgession < CurrentLevel.maxLevelProgession)
        {
            levelProgession += Time.deltaTime;
            progessBar.UpdateProgess(levelProgession, CurrentLevel.maxLevelProgession);

            float normalized = levelProgession / CurrentLevel.maxLevelProgession;
            SpeedData.currentProgressionMultiplier = Mathf.Lerp(SpeedData.minProgressionMultiplier, SpeedData.maxProgressionMultiplier, normalized);
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
