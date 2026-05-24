using System.Collections;
using UnityEngine;

public class LoseState : IState
{
    GameManager gm;
  
    public LoseState(GameManager gm)
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
