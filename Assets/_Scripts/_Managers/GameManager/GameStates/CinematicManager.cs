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

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = url;

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
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