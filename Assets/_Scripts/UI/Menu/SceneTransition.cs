using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    // =========================================================
    // DESTINATIONS
    // =========================================================
    [Header("Default Destination")]
    [SerializeField]
    private transitionTo defaultTarget;
    public enum transitionTo
    {
        Cinematics,
        Gameplay,

        /*
         * Legacy.
         * Lo dejamos para no romper referencias antiguas.
         */
        SurvivalCoop,

        Exit,

        LevelSelector,

        /*
         * Nuevo destino genérico.
         */
        Survival,

        MainMenu
    }


    // =========================================================
    // SCENES
    // =========================================================

    [Header("Scene Names")]
    [SerializeField]
    private string cinematicsSceneName =
        "CinematicsScene";

    [SerializeField]
    private string gameplaySceneName = "SampleScene";

    [SerializeField]
    private string survivalSceneName =
        "SurvivalScene";
    
    [SerializeField]
    private string survivalCoopVersusSceneName =
        "SurvivalSceneCoop";

    [SerializeField]
    private string levelSelectorSceneName =
        "LevelSelector";

    [SerializeField]
    private string mainMenuSceneName =
        "Menu";


    // =========================================================
    // FADE
    // =========================================================

    [Header("Fade")]
    [SerializeField]
    private CanvasGroup fadeCanvasGroup;

    [SerializeField]
    private float fadeDuration = 0.5f;


    // =========================================================
    // STATE
    // =========================================================

    private bool transitioning;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (fadeCanvasGroup == null)
            return;


        fadeCanvasGroup.gameObject.SetActive(
            true
        );

        fadeCanvasGroup.alpha = 0f;

        fadeCanvasGroup.interactable = false;

        fadeCanvasGroup.blocksRaycasts = false;
    }


    // =========================================================
    // START TRANSITION
    // =========================================================

    public void StartTransition(
        transitionTo target
    )
    {
        if (transitioning)
            return;


        transitioning = true;


        StartCoroutine(
            TransitionRoutine(
                target
            )
        );
    }

    public void StartTransition()
    {
        StartTransition(defaultTarget);
    }


    // =========================================================
    // ROUTINE
    // =========================================================

    private IEnumerator TransitionRoutine(
        transitionTo target
    )
    {
        // -----------------------------------------------------
        // BLOCK INPUT
        // -----------------------------------------------------

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts =
                true;

            fadeCanvasGroup.interactable =
                true;
        }


        // -----------------------------------------------------
        // FADE TO BLACK
        // -----------------------------------------------------

        yield return Fade(
            0f,
            1f
        );


        // -----------------------------------------------------
        // EXIT
        // -----------------------------------------------------

        if (
            target ==
            transitionTo.Exit
        )
        {
            QuitGame();

            yield break;
        }


        // -----------------------------------------------------
        // GAME STATE
        // -----------------------------------------------------

        SetGameState(
            target
        );


        // -----------------------------------------------------
        // GET SCENE
        // -----------------------------------------------------

        string sceneName =
            GetSceneName(
                target
            );


        if (
            string.IsNullOrWhiteSpace(
                sceneName
            )
        )
        {
            Debug.LogError(
                $"[SceneTransition] No existe " +
                $"una escena configurada para " +
                $"'{target}'."
            );

            transitioning = false;

            yield return Fade(
                1f,
                0f
            );

            yield break;
        }


        // -----------------------------------------------------
        // LOAD
        // -----------------------------------------------------

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(
                sceneName
            );


        if (operation == null)
        {
            Debug.LogError(
                $"[SceneTransition] No se pudo " +
                $"cargar '{sceneName}'."
            );

            transitioning = false;

            yield break;
        }


        while (!operation.isDone)
        {
            yield return null;
        }
    }


    // =========================================================
    // GAME STATE
    // =========================================================

    private void SetGameState(
        transitionTo target
    )
    {
        if (GameManager.Instance == null)
            return;


        switch (target)
        {
            // -------------------------------------------------
            // HISTORIA
            // -------------------------------------------------
            case transitionTo.Gameplay:

                GameManager.Instance.ChangeState(
                    GameState.Playing
                );

                break;

            case transitionTo.Cinematics:

                GameManager.Instance.IsOutro =
                    false;

                GameManager.Instance.ChangeState(
                    GameState.Cinematic
                );

                break;


            // -------------------------------------------------
            // SURVIVAL
            // -------------------------------------------------

            case transitionTo.Survival:

            case transitionTo.SurvivalCoop:

                GameManager.Instance.ChangeState(
                    GameState.Survival
                );

                break;


            // -------------------------------------------------
            // MAIN MENU
            // -------------------------------------------------

            case transitionTo.MainMenu:

                GameManager.Instance.ChangeState(
                    GameState.MainMenu
                );

                break;


            // -------------------------------------------------
            // LEVEL SELECTOR
            // -------------------------------------------------

            case transitionTo.LevelSelector:

                GameManager.Instance.ChangeState(
                    GameState.MainMenu
                );

                break;
        }
    }


    // =========================================================
    // SCENE NAME
    // =========================================================

    private string GetSceneName(
        transitionTo target
    )
    {
        switch (target)
        {
            case transitionTo.Gameplay:

                return gameplaySceneName;

            case transitionTo.Cinematics:

                return cinematicsSceneName;


            case transitionTo.Survival:

                return survivalSceneName;

            case transitionTo.SurvivalCoop:

                return survivalCoopVersusSceneName;


            case transitionTo.LevelSelector:

                return levelSelectorSceneName;


            case transitionTo.MainMenu:

                return mainMenuSceneName;
        }


        return null;
    }


    // =========================================================
    // FADE
    // =========================================================

    private IEnumerator Fade(
        float from,
        float to
    )
    {
        if (fadeCanvasGroup == null)
            yield break;


        float elapsed = 0f;

        fadeCanvasGroup.alpha =
            from;


        while (
            elapsed <
            fadeDuration
        )
        {
            elapsed +=
                Time.unscaledDeltaTime;


            float normalized =
                fadeDuration <= 0f
                    ? 1f
                    : elapsed /
                      fadeDuration;


            fadeCanvasGroup.alpha =
                Mathf.Lerp(
                    from,
                    to,
                    normalized
                );


            yield return null;
        }


        fadeCanvasGroup.alpha =
            to;
    }


    // =========================================================
    // EXIT
    // =========================================================

    private void QuitGame()
    {
#if UNITY_EDITOR

        Debug.Log(
            "[SceneTransition] EXIT"
        );

        UnityEditor.EditorApplication
            .isPlaying = false;

#else

        Application.Quit();

#endif
    }
}