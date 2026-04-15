using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private Image imageToFade;
    [SerializeField] private float fadeDuration = 0.5f;
    public enum transitionTo { MainMenu, Gameplay, Cinematics, Credits }
    public transitionTo transitionTarget;
    public void StartTransition()
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
                    sceneToLoad = "MainMenu";
                    GameManager.Instance.ChangeState(GameState.MainMenu);
                    break;
                case transitionTo.Gameplay:
                    sceneToLoad = "SampleScene";
                    GameManager.Instance.ChangeState(GameState.Playing);
                    break;
                case transitionTo.Cinematics:
                    sceneToLoad = "CinematicsScene";
                    GameManager.Instance.ChangeState(GameState.Cinematic);
                    break;
                case transitionTo.Credits:
                    sceneToLoad = "CreditsScene";
                    //GameManager.Instance.ChangeState(GameState.Credits);
                    break;
                default:
                    break;
            }
            SceneManager.LoadScene(sceneToLoad);
        });
    }
}
