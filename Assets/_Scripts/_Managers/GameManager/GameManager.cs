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

    public void SaveProgress(int newLevelIndex)
    {
        PlayerPrefs.SetInt("LevelIndex", newLevelIndex);
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
