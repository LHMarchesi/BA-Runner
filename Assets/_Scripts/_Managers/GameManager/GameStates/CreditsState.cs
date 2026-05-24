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

        AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusicClip);

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
