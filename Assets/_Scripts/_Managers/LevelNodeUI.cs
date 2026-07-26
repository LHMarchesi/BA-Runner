using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum LevelNodeState
{
    Hidden,
    Unlocked,
    Completed,
    Current
}

public class LevelNodeUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private Level_Scriptable level;

    public Level_Scriptable Level => level;

    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI levelNameText;

    [Header("State Visuals")]
    [Tooltip("Imagen que indica que este es el nivel actual.")]
    [SerializeField] private Image currentVisual;

    [Tooltip("Tilde que aparece encima cuando el nivel está completado.")]
    [SerializeField] private Image completedVisual;

    [Header("Stars")]
    [SerializeField] private GameObject starsRoot;
    [SerializeField] private List<Image> starImages = new List<Image>();
    [SerializeField] private Sprite fullStarSprite;
    [SerializeField] private Sprite emptyStarSprite;

    private LevelSelectManager manager;

    private void Reset()
    {
        button = GetComponent<Button>();

        if (button == null)
            button = GetComponentInChildren<Button>();
    }

    public void Initialize(LevelSelectManager levelSelectManager)
    {
        manager = levelSelectManager;

        if (button == null)
            button = GetComponent<Button>();

        if (button == null)
            button = GetComponentInChildren<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnPressed);
        }

        if (levelNameText != null && level != null)
            levelNameText.text = level.LevelID;
    }

    public void SetState(LevelNodeState state)
    {
        bool isHidden = state == LevelNodeState.Hidden;

        gameObject.SetActive(!isHidden);

        if (isHidden)
            return;

        if (button != null)
        {
            button.interactable =
                state == LevelNodeState.Unlocked ||
                state == LevelNodeState.Completed ||
                state == LevelNodeState.Current;
        }

        // Imagen que marca el nivel actual.
        if (currentVisual != null)
        {
            currentVisual.enabled =
                state == LevelNodeState.Current;
        }

        // Tilde que aparece encima del botón.
        if (completedVisual != null)
        {
            completedVisual.enabled =
                state == LevelNodeState.Completed;
        }

        int stars = ProgressionManager.Instance.GetStarsForLevel(level);
        SetStars(stars);
    }

    private void SetStars(int starCount)
    {
        if (starsRoot != null)
            starsRoot.SetActive(true);

        for (int i = 0; i < starImages.Count; i++)
        {
            Image starImage = starImages[i];

            if (starImage == null)
                continue;

            starImage.gameObject.SetActive(true);

            if (fullStarSprite != null && emptyStarSprite != null)
            {
                starImage.sprite =
                    i < starCount
                        ? fullStarSprite
                        : emptyStarSprite;
            }
            else
            {
                starImage.gameObject.SetActive(i < starCount);
            }
        }
    }

    private void OnPressed()
    {
        if (level == null || manager == null)
            return;

        manager.SelectLevel(level);
    }
}