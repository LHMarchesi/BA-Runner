using System.Diagnostics;
using UnityEngine.SceneManagement;

public class SurvivalState : IState
{
    GameManager gm;
    bool sceneLoaded = false;

    public SurvivalState(GameManager gm)
    {
        this.gm = gm;
    }

    public void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        EventBus<OnLevelStartEvent>.Raise(new OnLevelStartEvent { });

    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneLoaded = true;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        AudioManager.Instance.PlayMusic(AudioManager.Instance.survivalSong);
    }

    public void Execute()
    {
      
    }

    public void Sleep()
    {
    }
}
