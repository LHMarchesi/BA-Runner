using UnityEngine;
using UnityEngine.Audio;

public class ControlMusica : MonoBehaviour
{
  [SerializeField] private AudioMixer audioMixer;

  public void ControldeMusica (float Slider)
  {
     audioMixer.SetFloat ("VolumenMusica", Mathf.Log10 (Slider) * 20);
  }
}
