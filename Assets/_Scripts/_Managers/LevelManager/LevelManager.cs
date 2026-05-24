using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;


    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    [SerializeField] public List<Level_Scriptable> levels;
    [SerializeField] private ProgessBar progessBar;

    private float levelProgession;
    private float completionTime;
    private bool levelCompleted;
    public Level_Scriptable CurrentLevel
    {
        get
        {
            if (levels == null || levels.Count == 0) return null;

            int index = GameManager.Instance.GameData.currentLevelIndex;

            if (index < 0 || index >= levels.Count)
            {
                index = 0;
                GameManager.Instance.GameData.currentLevelIndex = 0;
            }

            return levels[index];
        }
    }


    private void Start()
    {
        EventBus<OnLevelStartEvent>.Raise(new OnLevelStartEvent { levelSpeedData = CurrentLevel.speedData });
        levelProgession = 0;
        completionTime = 0;
        levelCompleted = false;
    }


    public void IncreaseLevelProgession()
    {
        if (levelCompleted) return;
        var currentLevel = CurrentLevel;
        if (currentLevel == null) return;
        completionTime += Time.deltaTime;

        if (levelProgession < currentLevel.maxLevelProgession)
        {
            var speedData = currentLevel.speedData;
            float boostImpact = Mathf.Pow(speedData.playerBoostMultiplier, 1.5f);
            levelProgession += Time.deltaTime * boostImpact;

            if (speedData != null)
            {
                float normalized = levelProgession / currentLevel.maxLevelProgession;

                speedData.currentProgressionMultiplier =
                    Mathf.Lerp(speedData.minProgressionMultiplier,
                               speedData.maxProgressionMultiplier,
                               normalized);
            }

            if (progessBar != null)
                progessBar.UpdateProgess(levelProgession, currentLevel.maxLevelProgession);
        }
        else
        {
            levelCompleted = true;
            EventBus<OnLevelCompletedEvent>.Raise(new OnLevelCompletedEvent{stars = CalculateStars(completionTime), completionTime = this.completionTime});
        }
    }

    int CalculateStars(float time)
    {
        if (time <= 30) return 5;
        if (time <= 45) return 4;
        if (time <= 60) return 3;
        if (time <= 90) return 2;

        return 1;
    }

    // ESTE método lo llama el CinematicManager cuando termina el OUTRO
    public void GoToNextLevel()
    {
        int nextIndex = GameManager.Instance.GameData.currentLevelIndex + 1;

        if (nextIndex < levels.Count)
        {
            GameManager.Instance.GameData.currentLevelIndex = nextIndex;
            GameManager.Instance.SaveProgress(nextIndex);

            GameManager.Instance.IsOutro = false;
            SceneManager.LoadScene("CinematicsScene");
            GameManager.Instance.ChangeState(GameState.Cinematic);
        }
        else
        {
            SceneManager.LoadScene("Credits");
            GameManager.Instance.ChangeState(GameState.Credits);
        }
    }
}