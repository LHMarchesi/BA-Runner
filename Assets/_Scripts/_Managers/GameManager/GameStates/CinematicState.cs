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
    }

    public void Execute() { }

    public void Sleep() { }
}


