using UnityEngine;
using UnityEngine.SceneManagement;

public class SurvivalState : IState
{
    private GameManager gm;


    public SurvivalState(GameManager gm)
    {
        this.gm = gm;
    }


    // =========================================================
    // ENTER
    // =========================================================

    public void Awake()
    {
        /*
         * Evitamos listeners duplicados.
         */
        SceneManager.sceneLoaded -=
            OnSceneLoaded;

        SceneManager.sceneLoaded +=
            OnSceneLoaded;


        Debug.Log(
            "[SurvivalState] Esperando Survival Scene..."
        );
    }


    // =========================================================
    // SCENE LOADED
    // =========================================================

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        SceneManager.sceneLoaded -=
            OnSceneLoaded;


        Debug.Log(
            $"[SurvivalState] Scene Loaded: {scene.name}"
        );


        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(
                AudioManager.Instance.survivalSong
            );
        }


        /*
         * NO OnLevelStartEvent.
         *
         * SurvivalManager inicializa su propio gameplay.
         */
    }


    // =========================================================
    // UPDATE
    // =========================================================

    public void Execute()
    {
    }


    // =========================================================
    // EXIT
    // =========================================================

    public void Sleep()
    {
        /*
         * Por seguridad, nunca dejamos un sceneLoaded
         * pendiente al abandonar Survival.
         */
        SceneManager.sceneLoaded -=
            OnSceneLoaded;


        Debug.Log(
            "[SurvivalState] Exit Survival."
        );
    }
}