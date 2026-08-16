using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsState : IState
{
    GameManager gm;
    bool sceneLoaded = false;

    public CreditsState(GameManager gm)
    {
        this.gm = gm;
    }

    public void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneLoaded = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(
                AudioManager.Instance.creditsMusicClip
            );
        }
        else
        {
            Debug.LogWarning(
                "[CreditsState] AudioManager.Instance " +
                "es null al cargar los créditos. No se " +
                "reprodujo la música."
            );
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Execute()
    {
        // gameplay corre solo
    }

    public void Sleep()
    {
    }
}