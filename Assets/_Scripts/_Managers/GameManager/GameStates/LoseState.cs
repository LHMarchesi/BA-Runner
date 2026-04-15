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
        gm.StartCoroutine(LoseRoutine());
    }

    public void Execute()
    {
        
    }

    public void Sleep()
    {
        UIManager.Instance.ToggleLoseScreen(false);
    }

    IEnumerator LoseRoutine()
    {
        // mostrar pantalla de derrota
        yield return new WaitForSeconds(1.5f);
        UIManager.Instance.ToggleLoseScreen(true);
    }
}
