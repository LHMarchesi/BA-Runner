using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource MusicSource;
    [SerializeField] private AudioMixerGroup ControlSFX;

    [SerializeField] public AudioClip menuMusicClip;
    [SerializeField] public AudioClip cinematicsSong;
    [SerializeField] public AudioClip survivalSong;

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

            if (ControlSFX != null)
            {
                audioSource.outputAudioMixerGroup = ControlSFX;
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
            audioSource.pitch = Random.Range(0.95f, 1.3f);
            audioSource.PlayOneShot(clip);
        }
    }

    public void StopMusic()
    {
        if (MusicSource != null && MusicSource.isPlaying)
        {
            MusicSource.Stop();
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip != null && MusicSource != null)
        {
            MusicSource.clip = clip;
            MusicSource.time = 0f;
            MusicSource.Play();
        }
    }
}