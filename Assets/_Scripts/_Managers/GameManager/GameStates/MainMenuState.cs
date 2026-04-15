public class MainMenuState : IState
{
    GameManager gm;

    public MainMenuState(GameManager gm)
    {
        this.gm = gm;
    }

    public void Awake()
    {
    }

    public void Execute()
    {
        // gameplay corre solo
    }

    public void Sleep()
    {
    }
}
