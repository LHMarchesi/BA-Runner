using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;


public class CreditsSequence : MonoBehaviour
{
    [System.Serializable]
    public class LocalizedCreditImages
    {
        [Tooltip("Código de locale, por ejemplo: en, es-AR, pt-BR")]
        public string localeCode;

        public Image[] images;
    }

    [Header("Imágenes (idioma por defecto / fallback)")]
    [SerializeField]
    private Image[] creditImages;

    [Header("Overrides por idioma (opcional)")]
    [Tooltip(
        "Si el idioma activo coincide con un localeCode de " +
        "acá, se usan estas imágenes en vez de las de arriba."
    )]
    [SerializeField]
    private LocalizedCreditImages[] localizedOverrides;

    [Header("Tiempos (segundos)")]
    [SerializeField]
    private float fadeInDuration = 1f;

    [SerializeField]
    private float holdDuration = 2f;

    [SerializeField]
    private float fadeOutDuration = 1f;

    [Header("Al terminar")]
    [SerializeField]
    private bool autoTransitionToMenuOnFinish = true;

    [SerializeField]
    private SceneTransition sceneTransition;

    private void Awake()
    {

        ResetImages(creditImages);

        if (localizedOverrides != null)
        {
            foreach (LocalizedCreditImages set in localizedOverrides)
            {
                if (set != null)
                    ResetImages(set.images);
            }
        }
    }

    private void Start()
    {
        StartCoroutine(PlaySequenceRoutine());
    }

    private IEnumerator PlaySequenceRoutine()
    {

        yield return LocalizationSettings.InitializationOperation;

        Image[] imagesToPlay = ResolveImagesForCurrentLocale();

        if (imagesToPlay == null || imagesToPlay.Length == 0)
        {
            Debug.LogWarning(
                "[CreditsSequence] No hay imágenes asignadas " +
                "para el idioma actual."
            );

            yield break;
        }

        yield return PlaySequence(imagesToPlay);
    }

    private Image[] ResolveImagesForCurrentLocale()
    {
        string currentLocaleCode =
            LocalizationSettings.SelectedLocale != null
                ? LocalizationSettings.SelectedLocale.Identifier.Code
                : null;

        if (
            !string.IsNullOrEmpty(currentLocaleCode) &&
            localizedOverrides != null
        )
        {
            foreach (LocalizedCreditImages set in localizedOverrides)
            {
                if (
                    set != null &&
                    set.images != null &&
                    set.images.Length > 0 &&
                    string.Equals(
                        set.localeCode,
                        currentLocaleCode,
                        System.StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return set.images;
                }
            }
        }

        return creditImages;
    }

    private IEnumerator PlaySequence(Image[] images)
    {
        foreach (Image image in images)
        {
            if (image == null)
                continue;

            image.gameObject.SetActive(true);

            yield return image
                .DOFade(1f, fadeInDuration)
                .WaitForCompletion();

            yield return new WaitForSeconds(holdDuration);

            yield return image
                .DOFade(0f, fadeOutDuration)
                .WaitForCompletion();

            image.gameObject.SetActive(false);
        }

        if (autoTransitionToMenuOnFinish)
        {
            if (sceneTransition != null)
            {
                sceneTransition.StartTransition(
                    SceneTransition.transitionTo.MainMenu
                );
            }
            else
            {
                Debug.LogWarning(
                    "[CreditsSequence] Auto Transition To " +
                    "Menu On Finish está activado pero no " +
                    "hay un Scene Transition asignado."
                );
            }
        }
    }

    private void ResetImages(Image[] images)
    {
        if (images == null)
            return;

        foreach (Image image in images)
        {
            if (image == null)
                continue;

            SetAlpha(image, 0f);
            image.gameObject.SetActive(false);
        }
    }

    private void SetAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}