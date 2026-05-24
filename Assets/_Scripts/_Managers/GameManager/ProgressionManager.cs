using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance;

    [SerializeField] public List<Level_Scriptable> levels;
    public int CurrentLevelIndex { get; private set; }
    public Level_Scriptable CurrentLevel => levels[CurrentLevelIndex];

    [SerializeField] private GameData GameData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AdvanceLevel()
    {
        int nextIndex = CurrentLevelIndex + 1;

        if (nextIndex < levels.Count)
        {
            SaveProgress(nextIndex);

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

    public void SaveProgress(int newLevelIndex)
    {
        GameData.currentLevelIndex = newLevelIndex;
        PlayerPrefs.SetInt("LevelIndex", newLevelIndex);
        PlayerPrefs.Save();
    }


    public void LoadProgress()
    {
        if (PlayerPrefs.HasKey("LevelIndex"))
        {
            GameData.currentLevelIndex = PlayerPrefs.GetInt("LevelIndex");
            CurrentLevelIndex = GameData.currentLevelIndex;
        }
        else
        {
            GameData.currentLevelIndex = 0; // default
            CurrentLevelIndex = 0;
        }
    }
}