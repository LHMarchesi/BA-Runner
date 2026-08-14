using UnityEngine;

public class PauseState : IState
{
    GameManager gm;
    public PauseState(GameManager gm)
    {
        this.gm = gm;
    }
    public void Awake()
    {
        Time.timeScale = 0f;
        AudioListener.pause = true;
        EventBus<OnPauseEvent>.Raise(new OnPauseEvent { isPaused = true });
    }
    public void Execute()
    {
    }
    public void Sleep()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        EventBus<OnPauseEvent>.Raise(new OnPauseEvent { isPaused = false });
    }
}