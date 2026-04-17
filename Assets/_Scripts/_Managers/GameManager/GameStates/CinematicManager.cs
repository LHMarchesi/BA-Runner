using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CinematicManager : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject continueButton;

    private Level_Scriptable currentLevel;
    private bool isOutro;

    private void Start()
    {
        continueButton.SetActive(false);

        int index = GameManager.Instance.gameData.currentLevelIndex;
        currentLevel = LevelManager.instance.levels[index];

        isOutro = GameManager.Instance.IsOutro;

        PlayCinematic();
    }

    void PlayCinematic()
    {
        string url = isOutro ? currentLevel.outroURL : currentLevel.introURL;

        if (string.IsNullOrEmpty(url))
        {
            OnVideoFinished();
            return;
        }

        // Es buena práctica asegurarse de que la URL esté correctamente codificada (los espacios pueden dar problemas en WebGL)
        url = url.Replace(" ", "%20");

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = url;

        // En WebGL es crucial esperar a que el video se prepare antes de reproducirlo
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.loopPointReached += OnVideoFinished;
        
        videoPlayer.Prepare();
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();
    }

    void OnVideoError(VideoPlayer vp, string message)
    {
        // En Unity, un try-catch no funciona para VideoPlayer porque el error ocurre en un hilo interno asíncrono.
        // La mejor solución nativa es exactamente este evento 'errorReceived' (es el equivalente al "catch").
        
        // Usamos LogWarning en lugar de LogError para evitar que la consola de Unity pause el juego (Error Pause)
        Debug.LogWarning("Saltando video por error (Ignorable en Editor). Info: " + message);
        
        // En vez de mostrar el botón sobre una pantalla rota o negra, forzamos de inmediato el avance del juego
        OnContinuePressed();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        OnVideoFinished();
    }

    void OnVideoFinished()
    {
        continueButton.SetActive(true);
    }

    public void OnContinuePressed()
    {
        if (isOutro)
        {
           LevelManager.instance.GoToNextLevel();
        }
        else
        {
            LoadGameplay();
        }
    }

    void LoadGameplay()
    {
        SceneManager.LoadScene("SampleScene"); // tu escena real
        GameManager.Instance.ChangeState(GameState.Playing);
    }
}