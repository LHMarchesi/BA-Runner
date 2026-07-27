using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelBackgroundController : MonoBehaviour
{
    [Header("Background Images")]
    [SerializeField] private Image backgroundA;
    [SerializeField] private Image backgroundB;

    [Header("Transition")]
    [SerializeField, Min(0f)]
    private float fadeDuration = 0.4f;

    [Tooltip(
        "Muestra inmediatamente el primer fondo seleccionado " +
        "sin realizar una transición desde transparente."
    )]
    [SerializeField]
    private bool showFirstBackgroundImmediately = true;

    private Image currentBackground;
    private Image nextBackground;

    private LevelNodeUI lastSelectedNode;
    private Coroutine fadeRoutine;

    private bool hasShownFirstBackground;

    private void Awake()
    {
        currentBackground = backgroundA;
        nextBackground = backgroundB;

        PrepareImage(backgroundA);
        PrepareImage(backgroundB);

        SetAlpha(backgroundA, 0f);
        SetAlpha(backgroundB, 0f);

        if (backgroundA != null)
            backgroundA.enabled = false;

        if (backgroundB != null)
            backgroundB.enabled = false;
    }

    private void Update()
    {
        if (
            EventSystem.current == null ||
            currentBackground == null ||
            nextBackground == null
        )
        {
            return;
        }

        GameObject selected =
            EventSystem.current.currentSelectedGameObject;

        if (selected == null)
            return;

        /*
         * El Button o MenuButton puede estar en un hijo,
         * por eso buscamos el LevelNodeUI en los padres.
         */
        LevelNodeUI selectedNode =
            selected.GetComponentInParent<LevelNodeUI>();

        // El objeto seleccionado no es un nivel.
        // Por ejemplo, puede ser el botón Back.
        if (selectedNode == null)
            return;

        if (selectedNode == lastSelectedNode)
            return;

        lastSelectedNode = selectedNode;

        ShowBackground(selectedNode.BackgroundSprite);
    }

    private void ShowBackground(Sprite newSprite)
    {
        if (newSprite == null)
            return;

        if (
            currentBackground.sprite == newSprite &&
            currentBackground.enabled
        )
        {
            return;
        }

        if (
            !hasShownFirstBackground &&
            showFirstBackgroundImmediately
        )
        {
            ShowImmediately(newSprite);
            return;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        fadeRoutine =
            StartCoroutine(FadeBackgroundRoutine(newSprite));
    }

    private void ShowImmediately(Sprite sprite)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        currentBackground.sprite = sprite;
        currentBackground.enabled = true;

        SetAlpha(currentBackground, 1f);
        SetAlpha(nextBackground, 0f);

        nextBackground.enabled = false;

        hasShownFirstBackground = true;
    }

    private IEnumerator FadeBackgroundRoutine(
        Sprite newSprite
    )
    {
        nextBackground.sprite = newSprite;
        nextBackground.enabled = true;

        SetAlpha(nextBackground, 0f);

        float currentStartAlpha =
            currentBackground.enabled
                ? currentBackground.color.a
                : 0f;

        currentBackground.enabled = true;

        if (fadeDuration <= 0f)
        {
            SetAlpha(currentBackground, 0f);
            SetAlpha(nextBackground, 1f);

            CompleteTransition();
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float normalizedTime =
                Mathf.Clamp01(
                    elapsedTime / fadeDuration
                );

            float smoothTime =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    normalizedTime
                );

            SetAlpha(
                currentBackground,
                Mathf.Lerp(
                    currentStartAlpha,
                    0f,
                    smoothTime
                )
            );

            SetAlpha(
                nextBackground,
                Mathf.Lerp(
                    0f,
                    1f,
                    smoothTime
                )
            );

            yield return null;
        }

        SetAlpha(currentBackground, 0f);
        SetAlpha(nextBackground, 1f);

        CompleteTransition();
    }

    private void CompleteTransition()
    {
        currentBackground.enabled = false;

        Image previousBackground =
            currentBackground;

        currentBackground =
            nextBackground;

        nextBackground =
            previousBackground;

        hasShownFirstBackground = true;
        fadeRoutine = null;
    }

    private void PrepareImage(Image image)
    {
        if (image == null)
            return;

        // Evita que el fondo bloquee clicks o navegación.
        image.raycastTarget = false;
    }

    private void SetAlpha(
        Image image,
        float alpha
    )
    {
        if (image == null)
            return;

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private void OnDisable()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        lastSelectedNode = null;
    }
}