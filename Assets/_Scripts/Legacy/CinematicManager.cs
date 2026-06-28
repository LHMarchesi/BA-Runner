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

        currentLevel = ProgressionManager.Instance.CurrentLevel;

        isOutro = GameManager.Instance.IsOutro;
    }


    public void OnContinuePressed()
    {
        if (isOutro)
        {
           ProgressionManager.Instance.AdvanceLevel();
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