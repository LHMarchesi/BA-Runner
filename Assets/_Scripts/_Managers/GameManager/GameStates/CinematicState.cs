using UnityEngine.SceneManagement;

public class CinematicState : IState
{
    GameManager gm;

    public CinematicState(GameManager gm)
    {
        this.gm = gm;
    }

    public void Awake()
    {
        //var level = gm.gameData.currentLevel;

       // SceneManager.LoadScene(level.cinematicScene);
    }

    public void Execute() { }

    public void Sleep() { }
}