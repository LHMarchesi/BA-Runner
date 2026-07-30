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

    [Header("Background")]
    [Tooltip("Imagen de fondo que se muestra al seleccionar este nivel.")]
    [SerializeField] private Sprite backgroundSprite;

    public Sprite BackgroundSprite => backgroundSprite;

    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI levelNameText;

    [Header("State Visuals")]
    [Tooltip("Imagen que indica que este es el nivel actual.")]
    [SerializeField] private Image currentVisual;

    [Tooltip("Tilde que aparece encima cuando el nivel está completado.")]
    [SerializeField] private Image completedVisual;

    [Header("Completion Time")]
    [SerializeField] private GameObject bestTimeRoot;
    [SerializeField] private TextMeshProUGUI bestTimeText;

    [Tooltip("Texto que aparece antes del tiempo.")]
    [SerializeField] private string bestTimePrefix = "BEST ";

    [Header("Stars")]
    [SerializeField] private GameObject starsRoot;
    [SerializeField]
    private List<Image> starImages =
        new List<Image>();

    [SerializeField] private Sprite fullStarSprite;
    [SerializeField] private Sprite emptyStarSprite;

    private LevelSelectManager manager;
    private LevelNodeState currentState;

    private void Reset()
    {
        button = GetComponent<Button>();

        if (button == null)
            button = GetComponentInChildren<Button>(true);
    }

    public void Initialize(
        LevelSelectManager levelSelectManager)
    {
        manager = levelSelectManager;

        if (button == null)
            button = GetComponent<Button>();

        if (button == null)
            button = GetComponentInChildren<Button>(true);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnPressed);
        }

        if (levelNameText != null && level != null)
        {
            levelNameText.text = level.LevelID;
        }

        /*
         * Actualizamos la información guardada inmediatamente.
         * SetState volverá a actualizarla después cuando el
         * LevelSelectManager determine el estado del nivel.
         */
        RefreshProgress();
    }

    public void SetState(LevelNodeState state)
    {
        currentState = state;

        bool isHidden =
            state == LevelNodeState.Hidden;

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

        /*
         * Mantenemos la lógica visual que ya tenías.
         *
         * Actualmente esta imagen aparece en todos los
         * estados visibles.
         */
        if (currentVisual != null)
        {
            currentVisual.enabled =
                state == LevelNodeState.Current ||
                state == LevelNodeState.Completed ||
                state == LevelNodeState.Unlocked;
        }

        if (completedVisual != null)
        {
            completedVisual.enabled =
                state == LevelNodeState.Completed;
        }

        /*
         * Aunque el estado siga siendo Completed,
         * las estrellas y el mejor tiempo pueden haber cambiado.
         */
        RefreshProgress();
    }

    public void RefreshProgress()
    {
        if (level == null ||
            ProgressionManager.Instance == null)
        {
            SetStars(0);
            SetBestTimeVisible(false);
            return;
        }

        int bestStars =
            ProgressionManager.Instance
                .GetStarsForLevel(level);

        SetStars(bestStars);
        RefreshBestTime();
    }

    private void SetStars(int starCount)
    {
        int clampedStarCount =
            Mathf.Clamp(
                starCount,
                0,
                starImages.Count
            );

        if (starsRoot != null)
        {
            starsRoot.SetActive(true);
        }

        for (int i = 0; i < starImages.Count; i++)
        {
            Image starImage = starImages[i];

            if (starImage == null)
                continue;

            /*
             * Cuando hay sprites llenos y vacíos,
             * todas las estrellas permanecen visibles.
             */
            if (fullStarSprite != null &&
                emptyStarSprite != null)
            {
                starImage.gameObject.SetActive(true);

                starImage.sprite =
                    i < clampedStarCount
                        ? fullStarSprite
                        : emptyStarSprite;
            }
            else
            {
                /*
                 * Si no se configuraron ambos sprites,
                 * usamos el sistema alternativo de
                 * activar solamente las estrellas obtenidas.
                 */
                starImage.gameObject.SetActive(
                    i < clampedStarCount
                );
            }
        }
    }

    private void RefreshBestTime()
    {
        if (level == null ||
            ProgressionManager.Instance == null)
        {
            SetBestTimeVisible(false);
            return;
        }

        bool hasBestTime =
            ProgressionManager.Instance
                .TryGetBestLevelTime(
                    level,
                    out float bestTime
                );

        if (!hasBestTime)
        {
            SetBestTimeVisible(false);
            return;
        }

        SetBestTimeVisible(true);

        if (bestTimeText != null)
        {
            bestTimeText.text =
                bestTimePrefix +
                FormatTime(bestTime);
        }
    }

    private void SetBestTimeVisible(bool visible)
    {
        if (bestTimeRoot != null)
        {
            bestTimeRoot.SetActive(visible);
            return;
        }

        if (bestTimeText != null)
        {
            bestTimeText.gameObject.SetActive(
                visible
            );
        }
    }

    private string FormatTime(float totalSeconds)
    {
        totalSeconds =
            Mathf.Max(0f, totalSeconds);

        int minutes =
            Mathf.FloorToInt(
                totalSeconds / 60f
            );

        int seconds =
            Mathf.FloorToInt(
                totalSeconds % 60f
            );

        int centiseconds =
            Mathf.FloorToInt(
                totalSeconds * 100f
            ) % 100;

        return string.Format(
            "{0:00}:{1:00}.{2:00}",
            minutes,
            seconds,
            centiseconds
        );
    }

    private void OnPressed()
    {
        if (level == null || manager == null)
            return;

        manager.SelectLevel(level);
    }
}