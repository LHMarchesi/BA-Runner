using System.Collections;
using UnityEngine;

public class WinState : IState
{
    GameManager gm;

    public WinState(GameManager gm)
    {
        this.gm = gm;
    }

    public void Awake()
    {
        gm.NextLevel();
        UIManager.Instance.WinTranstion();
    }

    public void Execute()
    {
    }

    public void Sleep()
    {
    }
   
}
