using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private Image imageToFade;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private bool clearNarrativeFlags;
    public enum transitionTo { MainMenu, Gameplay, Cinematics, Credits, Survival, Exit }
    public transitionTo transitionTarget;


    public void StartTransition(transitionTo transition)
    {
        if (imageToFade == null)
        {
            Debug.LogWarning("No has asignado ninguna imagen para el Fade Out.");
            return;
        }

        imageToFade.DOFade(1f, fadeDuration).OnComplete(() =>
        {
            string sceneToLoad = "";

            switch (transition)
            {
                case transitionTo.MainMenu:
                    sceneToLoad = "Menu";
                    GameManager.Instance.ChangeState(GameState.MainMenu);
                    break;

                case transitionTo.Gameplay:
                    sceneToLoad = "SampleScene";
                    GameManager.Instance.ChangeState(GameState.Playing);
                    break;

                case transitionTo.Cinematics:
                    if (clearNarrativeFlags)
                        ProgressionManager.Instance.ResetProgress();

                    sceneToLoad = "CinematicsScene";
                    GameManager.Instance.ChangeState(GameState.Cinematic);
                    break;

                case transitionTo.Credits:
                    sceneToLoad = "CreditsScene";
                    break;

                case transitionTo.Survival:
                    sceneToLoad = "SurvivalScene";
                    GameManager.Instance.ChangeState(GameState.Survival);
                    break;

                case transitionTo.Exit:
                    Application.Quit();
                    return;
            }

            SceneManager.LoadScene(sceneToLoad);
        });
    }
    public void StartTransition( )
    {
        if (imageToFade == null)
        {
            Debug.LogWarning("No has asignado ninguna imagen para el Fade Out.");
            return;
        }

        imageToFade.DOFade(1f, fadeDuration).OnComplete(() =>
        {
            string sceneToLoad = "";

            switch (transitionTarget)
            {
                case transitionTo.MainMenu:
                    sceneToLoad = "Menu";
                    GameManager.Instance.ChangeState(GameState.MainMenu);
                    break;

                case transitionTo.Gameplay:
                    sceneToLoad = "SampleScene";
                    GameManager.Instance.ChangeState(GameState.Playing);
                    break;

                case transitionTo.Cinematics:
                    if (clearNarrativeFlags)
                        ProgressionManager.Instance.ResetProgress();

                    sceneToLoad = "CinematicsScene";
                    GameManager.Instance.ChangeState(GameState.Cinematic);
                    break;

                case transitionTo.Credits:
                    sceneToLoad = "CreditsScene";
                    break;

                case transitionTo.Survival:
                    sceneToLoad = "SurvivalScene";
                    GameManager.Instance.ChangeState(GameState.Survival);
                    break;

                case transitionTo.Exit:
                    Application.Quit();
                    return;
            }

            SceneManager.LoadScene(sceneToLoad);
        });
    }
}
