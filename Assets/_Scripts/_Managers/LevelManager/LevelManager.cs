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

    public static Action OnLevelComplete;

    public Level_Scriptable CurrentLevel
    {
        get
        {
            if (levels == null || levels.Count == 0) return null;

            int index = GameManager.Instance.gameData.currentLevelIndex;

            if (index < 0 || index >= levels.Count)
            {
                index = 0;
                GameManager.Instance.gameData.currentLevelIndex = 0;
            }

            return levels[index];
        }
    }

    private void Start()
    {
        levelProgession = 0;
    }

    public void LoadLevel()
    {
        levelProgession = 0;

        if (progessBar != null)
            progessBar.UpdateProgess(levelProgession, CurrentLevel.maxLevelProgession);
    }

    public void IncreaseLevelProgession()
    {
        var currentLevel = CurrentLevel;
        if (currentLevel == null) return;

        if (levelProgession < currentLevel.maxLevelProgession)
        {
            levelProgession += Time.deltaTime;

            if (progessBar != null)
                progessBar.UpdateProgess(levelProgession, currentLevel.maxLevelProgession);

            var speedData = currentLevel.speedData;
            if (speedData != null)
            {
                float normalized = levelProgession / currentLevel.maxLevelProgession;

                speedData.currentProgressionMultiplier =
                    Mathf.Lerp(speedData.minProgressionMultiplier,
                               speedData.maxProgressionMultiplier,
                               normalized);
            }
        }
        else
        {
            LevelCompleted();
        }
    }

    void LevelCompleted()
    {
        Debug.Log("Nivel completado");

        OnLevelComplete?.Invoke();

        GameManager.Instance.IsOutro = true;

        // Ir a cinemática de salida
        SceneManager.LoadScene("CinematicsScene");
        GameManager.Instance.ChangeState(GameState.Cinematic);
    }

    // ESTE método lo llama el CinematicManager cuando termina el OUTRO
    public void GoToNextLevel()
    {
        int nextIndex = GameManager.Instance.gameData.currentLevelIndex + 1;

        if (nextIndex < levels.Count)
        {
            GameManager.Instance.gameData.currentLevelIndex = nextIndex;
            GameManager.Instance.SaveProgress(nextIndex);

            GameManager.Instance.IsOutro = false;
            SceneManager.LoadScene("CinematicsScene");
            GameManager.Instance.ChangeState(GameState.Cinematic);
        }
        else
        {
            Debug.Log("Juego completado");
        }
    }
}