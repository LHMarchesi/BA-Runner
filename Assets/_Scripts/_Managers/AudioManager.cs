using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource dialogueSFXSource;

    [Header("Mixer")]
    [SerializeField] private AudioMixerGroup controlSFX;
    [SerializeField] private AudioMixerGroup controlMusic;

    [Header("Default Music")]
    [SerializeField] public AudioClip menuMusicClip;
    [SerializeField] public AudioClip survivalSong;
    [SerializeField] public AudioClip creditsMusicClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ConfigureAudioSources();
    }

    private void ConfigureAudioSources()
    {
        if (musicSource == null)
        {
            Debug.LogError(
                "[AudioManager] Music Source no est� asignado."
            );
        }
        else
        {
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.mute = false;

            if (controlMusic != null)
            {
                musicSource.outputAudioMixerGroup =
                    controlMusic;
            }
        }

        if (sfxSource == null)
        {
            Debug.LogError(
                "[AudioManager] SFX Source no est� asignado."
            );
        }
        else
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.mute = false;

            if (controlSFX != null)
            {
                sfxSource.outputAudioMixerGroup =
                    controlSFX;
            }
        }
    }

    public void PlayDialogueSFX(
    AudioClip clip,
    float volume = 1f
)
    {
        if (clip == null)
            return;

        if (dialogueSFXSource == null)
        {
            Debug.LogWarning(
                "[AudioManager] Dialogue SFX Source no est� asignado."
            );

            return;
        }


        dialogueSFXSource.Stop();

        dialogueSFXSource.pitch =
            Random.Range(0.95f, 1.05f);

        dialogueSFXSource.PlayOneShot(
            clip,
            Mathf.Clamp01(volume)
        );
    }

    public void StopDialogueSFX()
    {
        if (dialogueSFXSource == null)
            return;

        dialogueSFXSource.Stop();
    }
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
            return;

        if (sfxSource == null)
        {
            return;
        }

        sfxSource.pitch =
            Random.Range(0.95f, 1.05f);

        sfxSource.PlayOneShot(
            clip
        );
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null)
            return;


        if (musicSource.clip == clip &&
            musicSource.isPlaying)
        {
            return;
        }

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.time = 0f;
        musicSource.loop = true;
        musicSource.pitch = 1f;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null)
            return;

        musicSource.Stop();
        musicSource.clip = null;
    }
}