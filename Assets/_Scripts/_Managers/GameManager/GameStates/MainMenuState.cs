using UnityEngine.SceneManagement;

public class MainMenuState : IState
{
    GameManager gm;
    bool sceneLoaded = false;

    public MainMenuState(GameManager gm)
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

        AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusicClip);

        SceneManager.sceneLoaded -= OnSceneLoaded;
        EventBus<OnEnterMenuEvent>.Raise(new OnEnterMenuEvent());
    }


    public void Execute()
    {
        // gameplay corre solo
    }

    public void Sleep()
    {
    }
}
