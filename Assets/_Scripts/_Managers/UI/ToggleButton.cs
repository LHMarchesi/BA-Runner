using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ToggleButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image targetImage;

    [Header("Sprites")]
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;

    [Header("Default")]
    [SerializeField] private bool startOn = true;

    public UnityEvent<bool> OnValueChanged;

    private Button button;

    public bool IsOn { get; private set; }

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Toggle);

        SetValue(startOn, true);
    }

    public void Toggle()
    {
        SetValue(!IsOn);
    }

    public void SetValue(bool value, bool force = false)
    {
        if (!force && IsOn == value)
            return;

        IsOn = value;

        if (targetImage != null)
            targetImage.sprite = IsOn ? onSprite : offSprite;
        OnValueChanged?.Invoke(IsOn);
    }

    public void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
    }
}

