using UnityEngine;
using UnityEngine.UI;

public class FullscreenToggle : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;

    [Header("Icons")]
    [SerializeField] private Sprite fullscreenIcon;
    [SerializeField] private Sprite windowedIcon;
    private bool isFullscreen;

    private void Start()
    {
        isFullscreen =
            Screen.fullScreenMode != FullScreenMode.Windowed;

        UpdateIcon();
    }

    public void ToggleFullscreen()
    {
        isFullscreen = !isFullscreen;

        if (isFullscreen)
        {
            Screen.fullScreenMode =
                FullScreenMode.FullScreenWindow;

            Screen.fullScreen = true;
        }
        else
        {
            Screen.fullScreenMode =
                FullScreenMode.Windowed;

            Screen.fullScreen = false;
        }

        UpdateIcon();
    }

    private void UpdateIcon()
    {
        if (iconImage == null)
            return;

        iconImage.sprite =
            isFullscreen
                ? windowedIcon
                : fullscreenIcon;
    }
}