using UnityEngine.SceneManagement;

public class CinematicState : IState
{
    GameManager gm;
    bool sceneLoaded = false;   
    public CinematicState(GameManager gm)
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

        AudioManager.Instance.PlayMusic(AudioManager.Instance.cinematicsSong);

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    public void Execute() { }

    public void Sleep() { }
}


