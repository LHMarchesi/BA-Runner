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
        var level = gm.gameData.currentLevel;
        AudioManager.Instance.PlayMusic(level.levelMusic);
        SceneManager.LoadScene(level.gameplayScene);
    }

    public void Execute()
    {
        // gameplay corre solo
    }

    public void Sleep()
    {
        AudioManager.Instance.StopMusic();
    }
}
