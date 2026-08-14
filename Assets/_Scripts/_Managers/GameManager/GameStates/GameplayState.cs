using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayState : IState
{
    private GameManager gm;

    private bool gameplayInitialized;

    public GameplayState(GameManager gm)
    {
        this.gm = gm;
    }


  
    public void Awake()
    {
        SceneManager.sceneLoaded -=
            OnSceneLoaded;

        if (gameplayInitialized)
        {
            Debug.Log(
                "[GameplayState] Resume Gameplay."
            );

            return;
        }


        SceneManager.sceneLoaded +=
            OnSceneLoaded;

    }


    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        SceneManager.sceneLoaded -=
            OnSceneLoaded;


        gameplayInitialized = true;


        if (ProgressionManager.Instance == null)
        {
            Debug.LogError(
                "[GameplayState] ProgressionManager es NULL."
            );

            return;
        }


        Level_Scriptable level =
            ProgressionManager.Instance.CurrentLevel;


        if (level == null)
        {

            return;
        }

        if (
            AudioManager.Instance != null &&
            level.levelMusic != null
        )
        {
            AudioManager.Instance.PlayMusic(
                level.levelMusic
            );
        }


        EventBus<OnLevelStartEvent>.Raise(
            new OnLevelStartEvent()
        );
    }


    public void Execute()
    {
        if (!gameplayInitialized)
            return;


        EventBus<OnLevelUpdateEvent>.Raise(
            new OnLevelUpdateEvent()
        );
    }

    public void Sleep()
    {
        SceneManager.sceneLoaded -=
            OnSceneLoaded;

        if (gm.IsPausing)
        {
            return;
        }

        gameplayInitialized = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }
    }
}