using UnityEngine;
using UnityEngine.SceneManagement;

public class CinematicState : IState
{
    private GameManager gm;


    public CinematicState(GameManager gm)
    {
        this.gm = gm;
    }


    public void Awake()
    {
        SceneManager.sceneLoaded -=
            OnSceneLoaded;

        SceneManager.sceneLoaded +=
            OnSceneLoaded;
    }


    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        SceneManager.sceneLoaded -=
            OnSceneLoaded;

        Debug.Log(
            $"[CinematicState] Loaded: {scene.name}"
        );
    }


    public void Execute()
    {
    }


    public void Sleep()
    {
        SceneManager.sceneLoaded -=
            OnSceneLoaded;
    }
}