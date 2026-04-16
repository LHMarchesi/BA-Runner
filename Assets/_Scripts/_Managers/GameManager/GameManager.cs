using System.Collections.Generic;
using UnityEngine;
public enum GameState
{
    MainMenu,
    Cinematic,
    Playing,
    Win,
    Lose
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameData gameData;
    [SerializeField] public List<Level_Scriptable> levels;

    public Level_Scriptable CurrentLevel
    {
        get
        {
            if (levels == null || levels.Count == 0) return null;
            if (gameData.currentLevelIndex < 0 || gameData.currentLevelIndex >= levels.Count)
                gameData.currentLevelIndex = 0; // Reset to 0 if out of bounds
            return levels[gameData.currentLevelIndex];
        }
    }

    private StateMachine<GameState> stateMachine = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
          //  LoadProgress();
            InitializeStates();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetSpeedData(SpeedData speedData)
    {
        CurrentLevel.speedData = speedData;
    }
    public void NextLevel()
    {
        gameData.currentLevelIndex++;
        if (gameData.currentLevelIndex >= levels.Count)
        {
            gameData.currentLevelIndex = levels.Count - 1;
        }
        SaveProgress();
    }

    void InitializeStates()
    {
        stateMachine.AddState(new MainMenuState(this), GameState.MainMenu);
        stateMachine.AddState(new CinematicState(this), GameState.Cinematic);
        stateMachine.AddState(new GameplayState(this), GameState.Playing);
        stateMachine.AddState(new WinState(this), GameState.Win);
        stateMachine.AddState(new LoseState(this), GameState.Lose);

        stateMachine.ChangeState(GameState.MainMenu);
    }

    private void Update()
    {
        stateMachine.Update();
    }

    public void ChangeState(GameState newState)
    {
        stateMachine.ChangeState(newState);
    }

    public void SaveProgress()
    {
        PlayerPrefs.SetInt("LevelIndex", gameData.currentLevelIndex);
        PlayerPrefs.Save();
    }

    public void LoadProgress()
    {
        if (PlayerPrefs.HasKey("LevelIndex"))
        {
            gameData.currentLevelIndex = PlayerPrefs.GetInt("LevelIndex");
        }
        else
        {
            gameData.currentLevelIndex = 0; // default
        }
    }
}
