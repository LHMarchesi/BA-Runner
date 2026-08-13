using System;
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

    public GameState CurrentState
    {
        get;
        private set;
    }

    private EventBinding<OnLevelCompletedEvent>
        levelResultBinding;

    private EventBinding<OnPlayerDeathEvent>
        playerDeathBinding;

    private bool eventsRegistered;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(
            gameObject
        );

        InitializeStates();

        RegisterEvents();
    }


    // =========================================================
    // EVENTS
    // =========================================================

    private void RegisterEvents()
    {
        if (eventsRegistered)
            return;

        levelResultBinding =
            new EventBinding<OnLevelCompletedEvent>(
                OnLevelCompleted
            );

        EventBus<OnLevelCompletedEvent>
            .Register(
                levelResultBinding
            );


        playerDeathBinding =
            new EventBinding<OnPlayerDeathEvent>(
                OnPlayerDeath
            );

        EventBus<OnPlayerDeathEvent>
            .Register(
                playerDeathBinding
            );

        eventsRegistered = true;
    }


    private void UnregisterEvents()
    {
        if (!eventsRegistered)
            return;

        EventBus<OnLevelCompletedEvent>
            .Deregister(
                levelResultBinding
            );

        EventBus<OnPlayerDeathEvent>
            .Deregister(
                playerDeathBinding
            );

        eventsRegistered = false;
    }


    private void OnDestroy()
    {
        /*
         * Solo la instancia real debe limpiar
         * sus bindings.
         */
        if (Instance != this)
            return;

        UnregisterEvents();

        Instance = null;
    }


    // =========================================================
    // STATES
    // =========================================================

    private void InitializeStates()
    {
        stateMachine.AddState(
            new MainMenuState(this),
            GameState.MainMenu
        );

        stateMachine.AddState(
            new CinematicState(this),
            GameState.Cinematic
        );

        stateMachine.AddState(
            new GameplayState(this),
            GameState.Playing
        );

        stateMachine.AddState(
            new WinState(this),
            GameState.Win
        );

        stateMachine.AddState(
            new LoseState(this),
            GameState.Lose
        );

        stateMachine.AddState(
            new CreditsState(this),
            GameState.Credits
        );

        stateMachine.AddState(
            new SurvivalState(this),
            GameState.Survival
        );

        stateMachine.AddState(
            new PauseState(this),
            GameState.Pause
        );

        ChangeState(
            GameState.MainMenu
        );
    }


    private void Update()
    {
        stateMachine.Update();
    }


    // =========================================================
    // LEVEL COMPLETED
    // =========================================================

    private void OnLevelCompleted(
        OnLevelCompletedEvent e
    )
    {
        /*
         * Survival no debería utilizar el flujo
         * Win del modo Historia.
         */
        if (
            CurrentState ==
            GameState.Survival
        )
        {
            return;
        }

        IsOutro = true;

        ChangeState(
            GameState.Win
        );
    }


    // =========================================================
    // PLAYER DEATH
    // =========================================================

    private void OnPlayerDeath(
        OnPlayerDeathEvent e
    )
    {
        /*
         * En Survival las muertes son manejadas
         * por SurvivalManager.
         */
        if (
            CurrentState ==
            GameState.Survival
        )
        {
            return;
        }

        ChangeState(
            GameState.Lose
        );
    }


    // =========================================================
    // CHANGE STATE
    // =========================================================

    public void ChangeState(
        GameState newState
    )
    {
        CurrentState =
            newState;

        stateMachine.ChangeState(
            newState
        );
    }
}