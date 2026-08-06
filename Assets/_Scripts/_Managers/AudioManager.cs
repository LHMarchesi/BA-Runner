using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Mixer")]
    [SerializeField] private AudioMixerGroup controlSFX;
    [SerializeField] private AudioMixerGroup controlMusic;

    [Header("Default Music")]
    [SerializeField] public AudioClip menuMusicClip;
    [SerializeField] public AudioClip cinematicsSong;
    [SerializeField] public AudioClip survivalSong;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                $"[AudioManager] Se destruyó un AudioManager duplicado: " +
                $"{gameObject.scene.name}/{gameObject.name}"
            );

            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ConfigureAudioSources();
    }

    private void ConfigureAudioSources()
    {
        /*
         * No usamos GetComponent<AudioSource>() porque el objeto
         * puede tener más de un AudioSource.
         */

        if (musicSource == null)
        {
            Debug.LogError(
                "[AudioManager] Music Source no está asignado."
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
                "[AudioManager] SFX Source no está asignado."
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

        if (musicSource != null &&
            sfxSource != null &&
            musicSource == sfxSource)
        {
            Debug.LogError(
                "[AudioManager] Music Source y SFX Source apuntan " +
                "al mismo AudioSource. Deben ser componentes distintos."
            );
        }
    }
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
            return;

        if (sfxSource == null)
        {
            Debug.LogWarning(
                "[AudioManager] SFX AudioSource no está asignado."
            );

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
        /*
         * Un nodo sin música conserva la música actual.
         */
        if (clip == null)
            return;

        if (musicSource == null)
        {
            Debug.LogWarning(
                "[AudioManager] MusicSource no está asignado."
            );

            return;
        }

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