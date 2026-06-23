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
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneLoaded = true;

        AudioManager.Instance.PlayMusic(AudioManager.Instance.survivalSong);

        SceneManager.sceneLoaded -= OnSceneLoaded;
        EventBus<OnLevelStartEvent>.Raise(new OnLevelStartEvent { });

        Debug.WriteLine("enter survival State");
    }

    public void Execute()
    {
      
    }

    public void Sleep()
    {
    }
}
