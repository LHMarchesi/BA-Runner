using UnityEngine;
public enum GameState
{
    MainMenu,
    Cinematic,
    Playing,
    Win,
    Lose,
    Credits,
    Survival,
    Pause,
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private StateMachine<GameState> stateMachine = new();
    public bool IsOutro { get; set; }
    public bool IsPausing { get; set; }

    EventBinding<OnLevelCompletedEvent> levelResultBinding;
    EventBinding<OnPlayerDeathEvent> playerDeathBinding;

    private void OnEnable()
    {
        levelResultBinding = new EventBinding<OnLevelCompletedEvent>(OnLevelCompleted);
        EventBus<OnLevelCompletedEvent>.Register(levelResultBinding);

        playerDeathBinding = new EventBinding<OnPlayerDeathEvent>(OnPlayerDeath);
        EventBus<OnPlayerDeathEvent>.Register(playerDeathBinding);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

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
        stateMachine.AddState(new CreditsState(this), GameState.Credits);
        stateMachine.AddState(new CreditsState(this), GameState.Survival);
        stateMachine.AddState(new PauseState(this), GameState.Pause);

        stateMachine.ChangeState(GameState.MainMenu);
    }

    private void Update()
    {
        stateMachine.Update();
    }

    void OnLevelCompleted(OnLevelCompletedEvent e)
    {
        ChangeState(GameState.Win);
        IsOutro = true;
    }

    void OnPlayerDeath(OnPlayerDeathEvent e)
    {
        ChangeState(GameState.Lose);
    }

    public void ChangeState(GameState newState)
    {
        stateMachine.ChangeState(newState);
    }


    private void OnDisable()
    {
        EventBus<OnLevelCompletedEvent>.Deregister(levelResultBinding);
    }
}
