using System.Collections;
using Unity.VisualScripting.InputSystem;
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
    }

    public void Execute()
    {
    }

    public void Sleep()
    {
    }
   
}
