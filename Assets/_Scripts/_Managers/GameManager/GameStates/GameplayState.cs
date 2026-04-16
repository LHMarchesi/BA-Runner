using UnityEngine.SceneManagement;

public class GameplayState : IState
{
    GameManager gm;

    public GameplayState(GameManager gm)
    {
        this.gm = gm;
    }

    public void Awake()
    {
        var level = gm.CurrentLevel;
        AudioManager.Instance.PlayMusic(level.levelMusic);
        SceneManager.LoadScene(level.gameplayScene);
    }

    public void Execute()
    {
        LevelManager.instance.IncreaseLevelProgession();
    }

    public void Sleep()
    {
        AudioManager.Instance.StopMusic();
    }
}
