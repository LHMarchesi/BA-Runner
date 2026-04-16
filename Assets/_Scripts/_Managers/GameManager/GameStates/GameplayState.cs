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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneLoaded = true;

        var level = LevelManager.instance.CurrentLevel;
        AudioManager.Instance.PlayMusic(level.levelMusic);

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Execute()
    {
        if (!sceneLoaded) return;
        if (LevelManager.instance == null) return;

        LevelManager.instance.IncreaseLevelProgession();
    }

    public void Sleep()
    {
        AudioManager.Instance.StopMusic();
    }
}
