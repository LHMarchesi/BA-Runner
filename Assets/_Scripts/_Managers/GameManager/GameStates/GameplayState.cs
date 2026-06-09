using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayState : IState
{
    GameManager gm;
    bool sceneLoaded = false;

    public GameplayState(GameManager gm)
    {
        this.gm = gm;
    }

    public void Awake()
    {
        if (sceneLoaded) return; 

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneLoaded = true;

        var level = ProgressionManager.Instance.CurrentLevel;
        AudioManager.Instance.PlayMusic(level.levelMusic);

        EventBus<OnLevelStartEvent>.Raise(new OnLevelStartEvent { });

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Execute()
    {
        if (!sceneLoaded) return;
        EventBus<OnLevelUpdateEvent>.Raise(new OnLevelUpdateEvent());
    }

    public void Sleep()
    {
        if (gm.IsPausing)
        {
            gm.IsPausing = false; 
            return; 
        }

        sceneLoaded = false; 
        
        AudioManager.Instance.StopMusic();
    }
}
