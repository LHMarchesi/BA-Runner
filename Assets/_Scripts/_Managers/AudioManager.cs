using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource MusicSource;
    [SerializeField] public AudioClip menuMusicClip;
    [SerializeField] public AudioClip cinematicsSong;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.3f); // Agrega una ligera variación de tono para evitar la repetición monótona
            audioSource.PlayOneShot(clip);
        }
    }

    public void StopMusic()
    {
        if (MusicSource.isPlaying)
        {
            MusicSource.Stop();
        }
    }


    public void PlayMusic(AudioClip clip)
    {
        if (clip != null)
        {
            MusicSource.clip = clip;
            MusicSource.time = 0f;
            MusicSource.Play();
        }
    }
}
