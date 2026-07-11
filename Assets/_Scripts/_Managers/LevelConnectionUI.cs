using UnityEngine;
using UnityEngine.UI;

public class LevelConnectionUI : MonoBehaviour
{
    [Header("Connection")]
    [SerializeField] private Level_Scriptable fromLevel;
    [SerializeField] private Level_Scriptable toLevel;

    [Header("UI")]
    [SerializeField] private Image lineImage;

    private void Reset()
    {
        lineImage = GetComponent<Image>();
    }

    public void Refresh()
    {
        if (ProgressionManager.Instance == null)
            return;

        bool shouldShow =
            ProgressionManager.Instance.IsLevelDiscovered(fromLevel) &&
            ProgressionManager.Instance.IsLevelDiscovered(toLevel);

        gameObject.SetActive(shouldShow);

        if (lineImage != null)
            lineImage.enabled = shouldShow;
    }
}