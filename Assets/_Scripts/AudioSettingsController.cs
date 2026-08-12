using UnityEngine;
using UnityEngine.Audio;

public class AudioSettingsController : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    private const string MUSIC_VOLUME = "VolumenMusica";
    private const string SFX_VOLUME = "VolumenSFX";

    public void SetMusicVolume(float value)
    {
        float volumeDb = ConvertToDecibels(value);

        bool success =
            audioMixer.SetFloat(
                MUSIC_VOLUME,
                volumeDb
            );

        if (!success)
        {
            Debug.LogError(
                $"No se encontró el parámetro '{MUSIC_VOLUME}' " +
                "en el AudioMixer."
            );
        }
    }

    public void SetSFXVolume(float value)
    {
        float volumeDb = ConvertToDecibels(value);

        bool success =
            audioMixer.SetFloat(
                SFX_VOLUME,
                volumeDb
            );

        if (!success)
        {
            Debug.LogError(
                $"No se encontró el parámetro '{SFX_VOLUME}' " +
                "en el AudioMixer."
            );
        }
    }

    private float ConvertToDecibels(float value)
    {
        /*
         * Evita Log10(0), que daría -Infinity.
         */
        if (value <= 0.0001f)
            return -80f;

        return Mathf.Log10(value) * 20f;
    }
}