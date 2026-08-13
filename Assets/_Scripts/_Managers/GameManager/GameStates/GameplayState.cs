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
        LevelManager levelManager =
            Object.FindFirstObjectByType<LevelManager>();

        if (levelManager != null)
        {
            gameplayInitialized = true;

            return;
        }


        gameplayInitialized = false;

        SceneManager.sceneLoaded -=
            OnSceneLoaded;

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


        Level_Scriptable level = null;


        if (ProgressionManager.Instance != null)
        {
            level =
                ProgressionManager.Instance.CurrentLevel;
        }

        if (
            AudioManager.Instance != null &&
            level != null &&
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
            gm.IsPausing = false;

            return;
        }

        gameplayInitialized = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }
    }
}