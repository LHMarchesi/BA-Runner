using UnityEngine;
using UnityEngine.Audio;

public class ControlSFX : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    public void ControldeSFX (float Slider2)
    {
        audioMixer.SetFloat ("VolumenSFX", Mathf.Log10(Slider2) * 20);
    }
}
