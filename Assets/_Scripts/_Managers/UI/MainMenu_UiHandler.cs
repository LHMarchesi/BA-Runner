using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenu_UiHandler : MonoBehaviour
{
    [SerializeField] private Image pressAnyKeyRawImage;

    [SerializeField] private VideoPlayer modeSelectVideo;
    [SerializeField] private RawImage modeSelectRawImage;

    [Header("Video URLs")]
    [SerializeField] private string pressAnyKeyVideoURL;
    [SerializeField] private string modeSelectVideoURL;

    [Header("Menu")]
    [SerializeField] private Button[] buttons;
    [SerializeField] private MenuNavigation navigation;
    [SerializeField] private GameObject indicator;

    [Header("Background")]
    [SerializeField] private RawImage firstBackgroundRawImage;
    [SerializeField] private VideoPlayer firstBackgroundVideo;

    [Header("Audio")]
    [SerializeField] private AudioClip menuAppearSFX;

    [Header("Transition")]
    [SerializeField] private SceneTransition transition;

    [Header("Input")]
    private EventBinding<OnEnterMenuEvent> onEnterMenuBinding;

    private bool isMenuActive = false;


    private void Awake()
    {
        // Estado inicial
        indicator.SetActive(false);
        navigation.gameObject.SetActive(false);
        pressAnyKeyRawImage.gameObject.SetActive(true);
        // Preparar videos
        PrepareVideo(firstBackgroundVideo, firstBackgroundRawImage);
        PrepareVideo(modeSelectVideo, modeSelectRawImage);


        // Preparar botones
        PrepareButton(
            buttons[0].gameObject,
            SceneTransition.transitionTo.Cinematics
        );

        PrepareButton(
            buttons[1].gameObject,
            SceneTransition.transitionTo.Survival
        );

        PrepareButton(
            buttons[2].gameObject,
            SceneTransition.transitionTo.Exit
        );

        PrepareButton(
            buttons[3].gameObject,
            SceneTransition.transitionTo.LevelSelector
        );

        // Cargar URLs
        if (firstBackgroundVideo != null)
        {
            firstBackgroundVideo.url = pressAnyKeyVideoURL;
        }

        if (modeSelectVideo != null)
        {
            modeSelectVideo.url = modeSelectVideoURL;
        }
    }


    private void OnEnable()
    {
        onEnterMenuBinding =
            new EventBinding<OnEnterMenuEvent>(OnEnterMenu);

        EventBus<OnEnterMenuEvent>.Register(onEnterMenuBinding);
    }


    private void OnDisable()
    {
        EventBus<OnEnterMenuEvent>.Deregister(onEnterMenuBinding);

        if (modeSelectVideo != null)
            modeSelectVideo.Stop();

        if (firstBackgroundVideo != null)
            firstBackgroundVideo.Stop();
    }


    private void Update()
    {
        if (Keyboard.current != null && !isMenuActive)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                StartCoroutine(ModeSelectionSequence());
            }
        }
    }


    // ============================================================
    // BUTTONS
    // ============================================================

    private void PrepareButton(
        GameObject obj,
        SceneTransition.transitionTo transitionTo)
    {
        obj.SetActive(false);

        Button button = obj.GetComponent<Button>();

        button.interactable = false;

        button.onClick.AddListener(() =>
        {
            transition.StartTransition(transitionTo);
        });
    }


    // ============================================================
    // VIDEOS
    // ============================================================

    private void PrepareVideo(
        VideoPlayer videoPlayer,
        RawImage rawImage)
    {
        if (videoPlayer == null || rawImage == null)
            return;

        // Ocultar inicialmente
        rawImage.gameObject.SetActive(false);

        // Transparente
        Color color = rawImage.color;
        color.a = 0f;
        rawImage.color = color;

        // Configuración del VideoPlayer
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = true;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        // Cuando termina de preparar el video
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }


    private void OnVideoPrepared(VideoPlayer videoPlayer)
    {
        // No hacemos Play automáticamente acá.
        // Cada secuencia decide cuándo reproducirlo.
    }


    // ============================================================
    // ENTER MENU
    // ============================================================

    private void OnEnterMenu(OnEnterMenuEvent e)
    {
        StartCoroutine(MenuSequence());
    }


    private IEnumerator MenuSequence()
    {
        yield return new WaitForSeconds(.75f);

        yield return AnimateVideo(
            firstBackgroundVideo,
            firstBackgroundRawImage
        );
    }


    // ============================================================
    // PRESS ANY KEY -> MODE SELECTION
    // ============================================================

    private IEnumerator ModeSelectionSequence()
    {
        // Ocultar Press Any Key
        yield return FadeOutVideo(
            firstBackgroundVideo,
            firstBackgroundRawImage,
            0.35f
        );


        // ========================================================
        // FADE DEL BACKGROUND
        // ========================================================

        if (firstBackgroundRawImage != null)
        {
            Sequence backgroundSequence = DOTween.Sequence();

            backgroundSequence.Append(
                firstBackgroundRawImage.DOFade(0f, 1.35f)
            );

            yield return backgroundSequence.WaitForCompletion();
        }


        // ========================================================
        // MOSTRAR VIDEO DE SELECCIÓN
        // ========================================================
        pressAnyKeyRawImage.gameObject.SetActive(false);
        modeSelectRawImage.gameObject.SetActive(true);

        modeSelectVideo.Play();

        Sequence videoSequence = DOTween.Sequence();

        videoSequence.Append(
            modeSelectRawImage.DOFade(1f, 1.35f)
        );
        yield return videoSequence.WaitForCompletion();

        // ========================================================
        // BOTONES
        // ========================================================

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(true);
            buttons[i].interactable = true;
        }


        // Mostrar navegación
        navigation.gameObject.SetActive(true);

        // Mostrar indicador
        indicator.SetActive(true);


        // Seleccionar primer botón
        buttons[0].Select();

        EventSystem.current.SetSelectedGameObject(
            buttons[0].gameObject
        );


        isMenuActive = true;
       
    }


    // ============================================================
    // VIDEO APPEAR
    // ============================================================

    private IEnumerator AnimateVideo(
        VideoPlayer videoPlayer,
        RawImage rawImage)
    {
        rawImage.gameObject.SetActive(true);

        // Reproducir video
        videoPlayer.Play();

        // Fade in
        Sequence sequence = DOTween.Sequence();

        sequence.Append(
            rawImage.DOFade(1f, 0.35f)
        );

        yield return sequence.WaitForCompletion();

    }


    // ============================================================
    // VIDEO FADE OUT
    // ============================================================

    private IEnumerator FadeOutVideo(
        VideoPlayer videoPlayer,
        RawImage rawImage,
        float duration)
    {
        if (videoPlayer == null || rawImage == null)
            yield break;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(
            rawImage.DOFade(0f, duration)
        );

        yield return sequence.WaitForCompletion();

        rawImage.gameObject.SetActive(false);
        videoPlayer.Stop();

    }
}

