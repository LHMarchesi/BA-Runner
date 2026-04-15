using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private Image imageToFade;
    [SerializeField] private string sceneToLoad;
    [SerializeField] private float fadeDuration = 0.5f;

    public void StartTransition()
    {
        if (imageToFade == null)
        {
            Debug.LogWarning("No has asignado ninguna imagen para el Fade Out.");
            return;
        }

        imageToFade.DOFade(1f, fadeDuration).OnComplete(() =>
        {
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogError("El nombre de la escena a cargar está vacío.");
            }
        });
    }
}
