using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenu_UiHandler : MonoBehaviour
{
    // =========================================================
    // PRESS ANY KEY
    // =========================================================

    [Header("Press Any Key")]
    [SerializeField]
    private Image pressAnyKeyRawImage;


    // =========================================================
    // MAIN MENU VIDEO
    // =========================================================

    [Header("Main Menu Video")]
    [SerializeField]
    private VideoPlayer modeSelectVideo;

    [SerializeField]
    private RawImage modeSelectRawImage;


    // =========================================================
    // VIDEO LOADING IMAGES
    // =========================================================

    [Header("Video Loading Images")]
    [SerializeField]
    private Image firstVideoLoadingImage;
    [SerializeField] private TextMeshProUGUI firstVideoLoadingText;
    [SerializeField] private float loadingDotsInterval = 0.4f;
    [SerializeField]
    private Image modeVideoLoadingImage;


    // =========================================================
    // VIDEO URLS
    // =========================================================

    [Header("Video URLs")]
    [SerializeField]
    private string pressAnyKeyVideoURL;

    [SerializeField]
    private string modeSelectVideoURL;


    // =========================================================
    // MAIN MENU
    // =========================================================

    [Header("Main Menu")]
    [Tooltip(
        "0 = Historia\n" +
        "1 = Survival\n" +
        "2 = Exit\n" +
        "3 = Level Selector"
    )]
    [SerializeField]
    private Button[] buttons;

    [SerializeField]
    private MenuNavigation navigation;

    [SerializeField]
    private GameObject indicator;


    // =========================================================
    // SURVIVAL MENU
    // =========================================================

    [Header("Survival Menu")]
    [SerializeField]
    private CanvasGroup survivalMenuGroup;

    [Tooltip(
        "0 = Solo\n" +
        "1 = Coop\n" +
        "2 = Versus\n" +
        "3 = Back"
    )]
    [SerializeField]
    private Button[] survivalButtons;

    [SerializeField]
    private MenuNavigation survivalNavigation;

    [SerializeField]
    private GameObject survivalIndicator;


    // =========================================================
    // BACKGROUND
    // =========================================================

    [Header("Background")]
    [SerializeField]
    private RawImage firstBackgroundRawImage;

    [SerializeField]
    private VideoPlayer firstBackgroundVideo;


    // =========================================================
    // AUDIO
    // =========================================================

    [Header("Audio")]
    [SerializeField]
    private AudioClip menuAppearSFX;


    // =========================================================
    // TRANSITION
    // =========================================================

    [Header("Scene Transition")]
    [SerializeField]
    private SceneTransition transition;

    [Header("Menu Fade")]
    [SerializeField]
    private float survivalMenuFadeDuration = 0.4f;


    // =========================================================
    // EVENTS
    // =========================================================

    private EventBinding<OnEnterMenuEvent>
        onEnterMenuBinding;


    // =========================================================
    // STATE
    // =========================================================

    private bool isMenuActive;
    private bool isTransitioning;

    private Coroutine menuTransitionRoutine;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        InitializeVisualState();

        PrepareVideos();

        PrepareMainMenu();

        PrepareSurvivalMenu();
    }


    // =========================================================
    // INITIAL STATE
    // =========================================================

    private void InitializeVisualState()
    {
        isMenuActive = false;
        isTransitioning = false;


        // -----------------------------------------------------
        // PRESS ANY KEY
        // -----------------------------------------------------

        if (pressAnyKeyRawImage != null)
        {
            pressAnyKeyRawImage
                .gameObject
                .SetActive(true);
        }


        // -----------------------------------------------------
        // MAIN INDICATOR
        // -----------------------------------------------------

        if (indicator != null)
        {
            indicator.SetActive(false);
        }


        // -----------------------------------------------------
        // SURVIVAL INDICATOR
        // -----------------------------------------------------

        if (survivalIndicator != null)
        {
            survivalIndicator.SetActive(false);
        }


        // -----------------------------------------------------
        // SURVIVAL PANEL
        // -----------------------------------------------------

        if (survivalMenuGroup != null)
        {
            survivalMenuGroup
                .gameObject
                .SetActive(true);

            survivalMenuGroup.alpha = 0f;

            survivalMenuGroup.interactable = false;

            survivalMenuGroup.blocksRaycasts = false;
        }


        // -----------------------------------------------------
        // LOADING IMAGES
        // -----------------------------------------------------

        /*
         * El primer placeholder es visible
         * desde el inicio.
         */
        if (firstVideoLoadingImage != null)
        {
            firstVideoLoadingImage
                .gameObject
                .SetActive(true);

            SetImageAlpha(
                firstVideoLoadingImage,
                1f
            );
        }


        /*
         * El segundo solamente aparece cuando
         * vamos a utilizar el segundo video.
         */
        if (modeVideoLoadingImage != null)
        {
            modeVideoLoadingImage
                .gameObject
                .SetActive(false);

            SetImageAlpha(
                modeVideoLoadingImage,
                1f
            );
        }
    }


    // =========================================================
    // VIDEOS
    // =========================================================
    private IEnumerator LoadingDotsRoutine(
    TextMeshProUGUI text
)
    {
        int dots = 0;

        while (text != null)
        {
            dots++;

            if (dots > 3)
                dots = 1;

            text.text =
                "CARGANDO" +
                new string('.', dots);

            yield return new WaitForSeconds(
                loadingDotsInterval
            );
        }
    }
    private void StartLoadingAnimation(
    TextMeshProUGUI text,
    ref Coroutine coroutine
)
    {
        if (text == null)
            return;

        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        text.gameObject.SetActive(true);

        coroutine =
            StartCoroutine(
                LoadingDotsRoutine(text)
            );
    }
    private void StopLoadingAnimation(
    TextMeshProUGUI text,
    ref Coroutine coroutine
)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        if (text != null)
        {
            text.gameObject.SetActive(false);
        }
    }

    private void PrepareVideos()
    {
        /*
         * IMPORTANTE:
         *
         * Primero asignamos las URLs.
         * Después llamamos Prepare().
         */

        if (firstBackgroundVideo != null)
        {
            firstBackgroundVideo.url =
                pressAnyKeyVideoURL;
        }


        if (modeSelectVideo != null)
        {
            modeSelectVideo.url =
                modeSelectVideoURL;
        }


        PrepareVideo(
            firstBackgroundVideo,
            firstBackgroundRawImage
        );


        PrepareVideo(
            modeSelectVideo,
            modeSelectRawImage
        );


        /*
         * Empezamos a preparar ambos videos
         * inmediatamente.
         */
        if (firstBackgroundVideo != null)
        {
            firstBackgroundVideo.Prepare();
        }


        if (modeSelectVideo != null)
        {
            modeSelectVideo.Prepare();
        }
    }


    private void PrepareVideo(
        VideoPlayer videoPlayer,
        RawImage rawImage
    )
    {
        if (
            videoPlayer == null ||
            rawImage == null
        )
        {
            return;
        }


        rawImage.gameObject.SetActive(false);


        Color color =
            rawImage.color;

        color.a = 0f;

        rawImage.color =
            color;


        videoPlayer.playOnAwake = false;

        videoPlayer.isLooping = true;

        videoPlayer.renderMode =
            VideoRenderMode.RenderTexture;
    }


    // =========================================================
    // VIDEO READY
    // =========================================================

  


    private IEnumerator WaitForVideoPrepared(
        VideoPlayer videoPlayer
    )
    {
        if (videoPlayer == null)
            yield break;


        /*
         * Si por alguna razón todavía no se llamó
         * Prepare(), lo hacemos acá también.
         */
        if (!videoPlayer.isPrepared)
        {
            videoPlayer.Prepare();
        }


        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
    }


    private void SetImageAlpha(
        Image image,
        float alpha
    )
    {
        if (image == null)
            return;

        Color color =
            image.color;

        color.a = alpha;

        image.color =
            color;
    }


    // =========================================================
    // PREPARE MAIN MENU
    // =========================================================

    private void PrepareMainMenu()
    {
        if (
            buttons == null ||
            buttons.Length < 4
        )
        {
            Debug.LogError(
                "[MainMenu] Main Buttons necesita " +
                "al menos 4 botones."
            );

            return;
        }


        // HISTORIA

        PrepareSceneButton(
            buttons[0],
            SceneTransition.transitionTo.Cinematics
        );


        // SURVIVAL

        PrepareSurvivalButton(
            buttons[1]
        );


        // EXIT

        PrepareSceneButton(
            buttons[2],
            SceneTransition.transitionTo.Exit
        );


        // LEVEL SELECTOR

        PrepareSceneButton(
            buttons[3],
            SceneTransition.transitionTo.LevelSelector
        );
    }


    private void PrepareSceneButton(
        Button button,
        SceneTransition.transitionTo target
    )
    {
        if (button == null)
            return;


        button.gameObject.SetActive(false);

        button.interactable = false;


        button.onClick.AddListener(
            () =>
            {
                if (isTransitioning)
                    return;

                isTransitioning = true;

                transition.StartTransition(
                    target
                );
            }
        );
    }


    private void PrepareSurvivalButton(
        Button button
    )
    {
        if (button == null)
            return;


        button.gameObject.SetActive(false);

        button.interactable = false;


        button.onClick.AddListener(
            OpenSurvivalMenu
        );
    }


    // =========================================================
    // PREPARE SURVIVAL MENU
    // =========================================================

    private void PrepareSurvivalMenu()
    {
        if (
            survivalButtons == null ||
            survivalButtons.Length < 4
        )
        {
            Debug.LogError(
                "[MainMenu] Survival Buttons necesita " +
                "4 botones: Solo, Coop, Versus, Back."
            );

            return;
        }


        // SOLO

        survivalButtons[0]
            .onClick
            .AddListener(
                () =>
                    StartSurvival(
                        SurvivalMode.Solo
                    )
            );


        // COOP

        survivalButtons[1]
            .onClick
            .AddListener(
                () =>
                    StartSurvival(
                        SurvivalMode.Coop
                    )
            );


        // VERSUS

        survivalButtons[2]
            .onClick
            .AddListener(
                () =>
                    StartSurvival(
                        SurvivalMode.Versus
                    )
            );


        // BACK

        survivalButtons[3]
            .onClick
            .AddListener(
                CloseSurvivalMenu
            );


        SetSurvivalButtonsInteractable(
            false
        );
    }


    private void SetSurvivalButtonsInteractable(
        bool value
    )
    {
        if (survivalButtons == null)
            return;


        foreach (
            Button button
            in survivalButtons
        )
        {
            if (button != null)
            {
                button.interactable =
                    value;
            }
        }
    }


    // =========================================================
    // EVENTS
    // =========================================================

    private void OnEnable()
    {
        onEnterMenuBinding =
            new EventBinding<OnEnterMenuEvent>(
                OnEnterMenu
            );


        EventBus<OnEnterMenuEvent>
            .Register(
                onEnterMenuBinding
            );
    }


    private void OnDisable()
    {
        EventBus<OnEnterMenuEvent>
            .Deregister(
                onEnterMenuBinding
            );


        if (modeSelectVideo != null)
        {
            modeSelectVideo.Stop();
        }


        if (firstBackgroundVideo != null)
        {
            firstBackgroundVideo.Stop();
        }


        if (menuTransitionRoutine != null)
        {
            StopCoroutine(
                menuTransitionRoutine
            );

            menuTransitionRoutine =
                null;
        }


        DOTween.Kill(this);
    }


    // =========================================================
    // INPUT
    // =========================================================

    private void Update()
    {
        if (
            Keyboard.current == null ||
            isMenuActive ||
            isTransitioning
        )
        {
            return;
        }


        /*
         * Actualmente usás Space como
         * Press Any Key.
         */
        if (
            Keyboard.current
                .spaceKey
                .wasPressedThisFrame
        )
        {
            isTransitioning = true;

            StartCoroutine(
                ModeSelectionSequence()
            );
        }
    }


    // =========================================================
    // ENTER MENU
    // =========================================================

    private void OnEnterMenu(
        OnEnterMenuEvent e
    )
    {
        StartCoroutine(
            MenuSequence()
        );
    }


    private IEnumerator MenuSequence()
    {
        yield return new WaitForSeconds(
            0.75f
        );


        /*
         * AnimateVideo ahora espera automáticamente
         * hasta que el video esté preparado.
         */
        yield return AnimateVideo(
            firstBackgroundVideo,
            firstBackgroundRawImage,
            firstVideoLoadingImage,
            firstVideoLoadingText
        );
    }


    // =========================================================
    // PRESS ANY KEY → MAIN MENU
    // =========================================================

    private IEnumerator ModeSelectionSequence()
    {
        // -----------------------------------------------------
        // FADE PRESS ANY KEY BACKGROUND
        // -----------------------------------------------------

        yield return FadeOutVideo(
            firstBackgroundVideo,
            firstBackgroundRawImage,
            0.35f
        );


        if (pressAnyKeyRawImage != null)
        {
            pressAnyKeyRawImage
                .gameObject
                .SetActive(false);
        }


        if (firstVideoLoadingImage != null)
        {
            firstVideoLoadingImage
                .gameObject
                .SetActive(false);
        }


        // -----------------------------------------------------
        // PREPARE SECOND VIDEO
        // -----------------------------------------------------

        if (modeVideoLoadingImage != null)
        {
            modeVideoLoadingImage
                .gameObject
                .SetActive(true);

            SetImageAlpha(
                modeVideoLoadingImage,
                1f
            );
        }


        // -----------------------------------------------------
        // MAIN MENU VIDEO
        // -----------------------------------------------------

        if (
            modeSelectRawImage != null &&
            modeSelectVideo != null
        )
        {
            /*
             * Esperar al video sin mostrarlo todavía.
             *
             * Mientras esperamos:
             * ModeVideoLoadingImage permanece visible.
             */
            yield return WaitForVideoPrepared(
                modeSelectVideo
            );


            /*
             * Ahora sí ocultamos la imagen
             * estática y mostramos el video.
             */
            if (modeVideoLoadingImage != null)
            {
                modeVideoLoadingImage
                    .gameObject
                    .SetActive(false);
            }


            modeSelectRawImage
                .gameObject
                .SetActive(true);


            modeSelectRawImage.color =
                new Color(
                    modeSelectRawImage.color.r,
                    modeSelectRawImage.color.g,
                    modeSelectRawImage.color.b,
                    0f
                );


            modeSelectVideo.Play();


            yield return modeSelectRawImage
                .DOFade(
                    1f,
                    1.0f
                )
                .SetEase(Ease.OutQuad)
                .WaitForCompletion();
        }


        // -----------------------------------------------------
        // BUTTONS
        // -----------------------------------------------------

        SetMainButtonsVisible(
            true
        );


        SetMainButtonsInteractable(
            true
        );


        // -----------------------------------------------------
        // AUDIO
        // -----------------------------------------------------

        if (
            menuAppearSFX != null &&
            AudioManager.Instance != null
        )
        {
            AudioManager.Instance.PlaySFX(
                menuAppearSFX
            );
        }


        // -----------------------------------------------------
        // SELECT FIRST BUTTON
        // -----------------------------------------------------

        if (
            navigation != null &&
            buttons.Length > 0
        )
        {
            navigation.AssignButton(
                buttons[0].gameObject
            );
        }


        isMenuActive = true;

        isTransitioning = false;
    }


    // =========================================================
    // OPEN SURVIVAL MENU
    // =========================================================

    public void OpenSurvivalMenu()
    {
        if (
            isTransitioning ||
            menuTransitionRoutine != null
        )
        {
            return;
        }


        menuTransitionRoutine =
            StartCoroutine(
                OpenSurvivalMenuSequence()
            );
    }


    private IEnumerator OpenSurvivalMenuSequence()
    {
        isTransitioning = true;


        // -----------------------------------------------------
        // DISABLE MAIN INPUT
        // -----------------------------------------------------

        SetMainButtonsInteractable(
            false
        );


        if (indicator != null)
        {
            indicator.SetActive(false);
        }


        // -----------------------------------------------------
        // HIDE MAIN BUTTONS
        // -----------------------------------------------------

        SetMainButtonsVisible(
            false
        );


        // -----------------------------------------------------
        // SURVIVAL PANEL
        // -----------------------------------------------------

        if (survivalMenuGroup != null)
        {
            survivalMenuGroup.alpha = 0f;

            survivalMenuGroup.interactable =
                false;

            survivalMenuGroup.blocksRaycasts =
                false;
        }


        // -----------------------------------------------------
        // CROSS FADE
        // -----------------------------------------------------

        Sequence sequence =
            DOTween.Sequence();


        if (modeSelectRawImage != null)
        {
            sequence.Join(
                modeSelectRawImage
                    .DOFade(
                        0f,
                        survivalMenuFadeDuration
                    )
            );
        }


        if (survivalMenuGroup != null)
        {
            sequence.Join(
                survivalMenuGroup
                    .DOFade(
                        1f,
                        survivalMenuFadeDuration
                    )
            );
        }


        yield return sequence
            .SetEase(Ease.InOutQuad)
            .WaitForCompletion();


        // -----------------------------------------------------
        // PAUSE MAIN VIDEO
        // -----------------------------------------------------

        if (
            modeSelectVideo != null &&
            modeSelectVideo.isPlaying
        )
        {
            modeSelectVideo.Pause();
        }


        // -----------------------------------------------------
        // ENABLE SURVIVAL UI
        // -----------------------------------------------------

        if (survivalMenuGroup != null)
        {
            survivalMenuGroup.interactable =
                true;

            survivalMenuGroup.blocksRaycasts =
                true;
        }


        SetSurvivalButtonsInteractable(
            true
        );


        yield return null;

        Canvas.ForceUpdateCanvases();


        if (
            survivalNavigation != null &&
            survivalButtons != null &&
            survivalButtons.Length > 0 &&
            survivalButtons[0] != null
        )
        {
            survivalNavigation.AssignButton(
                survivalButtons[0].gameObject
            );


            Debug.Log(
                $"[MainMenu] Survival selected → " +
                $"{survivalButtons[0].gameObject.name}"
            );
        }


        menuTransitionRoutine = null;

        isTransitioning = false;
    }


    // =========================================================
    // CLOSE SURVIVAL MENU
    // =========================================================

    public void CloseSurvivalMenu()
    {
        if (
            isTransitioning ||
            menuTransitionRoutine != null
        )
        {
            return;
        }


        menuTransitionRoutine =
            StartCoroutine(
                CloseSurvivalMenuSequence()
            );
    }


    private IEnumerator CloseSurvivalMenuSequence()
    {
        isTransitioning = true;


        // -----------------------------------------------------
        // DISABLE SURVIVAL
        // -----------------------------------------------------

        SetSurvivalButtonsInteractable(
            false
        );


        if (survivalMenuGroup != null)
        {
            survivalMenuGroup.interactable =
                false;

            survivalMenuGroup.blocksRaycasts =
                false;
        }


        if (survivalIndicator != null)
        {
            survivalIndicator.SetActive(false);
        }


        // -----------------------------------------------------
        // MAIN VIDEO
        // -----------------------------------------------------

        if (modeSelectRawImage != null)
        {
            modeSelectRawImage
                .gameObject
                .SetActive(true);
        }


        if (modeSelectVideo != null)
        {
            /*
             * Ya fue preparado al iniciar el menú.
             */
            if (!modeSelectVideo.isPrepared)
            {
                yield return WaitForVideoPrepared(
                    modeSelectVideo
                );
            }

            modeSelectVideo.Play();
        }


        // -----------------------------------------------------
        // CROSS FADE
        // -----------------------------------------------------

        Sequence sequence =
            DOTween.Sequence();


        if (survivalMenuGroup != null)
        {
            sequence.Join(
                survivalMenuGroup
                    .DOFade(
                        0f,
                        survivalMenuFadeDuration
                    )
            );
        }


        if (modeSelectRawImage != null)
        {
            sequence.Join(
                modeSelectRawImage
                    .DOFade(
                        1f,
                        survivalMenuFadeDuration
                    )
            );
        }


        yield return sequence
            .SetEase(Ease.InOutQuad)
            .WaitForCompletion();


        // -----------------------------------------------------
        // MAIN BUTTONS
        // -----------------------------------------------------

        SetMainButtonsVisible(
            true
        );


        SetMainButtonsInteractable(
            true
        );


        if (
            navigation != null &&
            buttons.Length > 1
        )
        {
            navigation.AssignButton(
                buttons[1].gameObject
            );
        }


        menuTransitionRoutine = null;

        isTransitioning = false;
    }


    // =========================================================
    // START SURVIVAL
    // =========================================================

    private void StartSurvival(
        SurvivalMode selectedMode
    )
    {
        if (isTransitioning)
            return;


        isTransitioning = true;


        SurvivalRunConfig.SetMode(
            selectedMode
        );


        Debug.Log(
            $"[MainMenu] Survival Mode → " +
            $"{selectedMode}"
        );


        switch (selectedMode)
        {
            case SurvivalMode.Solo:

                transition.StartTransition(
                    SceneTransition.transitionTo.Survival
                );

                break;


            case SurvivalMode.Coop:

                transition.StartTransition(
                    SceneTransition.transitionTo.SurvivalCoop
                );

                break;


            case SurvivalMode.Versus:

                transition.StartTransition(
                    SceneTransition.transitionTo.SurvivalCoop
                );

                break;
        }
    }


    // =========================================================
    // MAIN BUTTON HELPERS
    // =========================================================

    private void SetMainButtonsVisible(
        bool value
    )
    {
        if (buttons == null)
            return;


        foreach (
            Button button
            in buttons
        )
        {
            if (button != null)
            {
                button.gameObject.SetActive(
                    value
                );
            }
        }
    }


    private void SetMainButtonsInteractable(
        bool value
    )
    {
        if (buttons == null)
            return;


        foreach (
            Button button
            in buttons
        )
        {
            if (button != null)
            {
                button.interactable =
                    value;
            }
        }
    }


    // =========================================================
    // VIDEO APPEAR
    // =========================================================
    private Coroutine firstLoadingTextCoroutine;
    private IEnumerator AnimateVideo(
        VideoPlayer videoPlayer,
        RawImage rawImage,
        Image loadingImage, 
        TextMeshProUGUI loadingText = null
    )
    {
        if (
         videoPlayer == null ||
         rawImage == null
     )
        {
            yield break;
        }

        if (loadingImage != null)
        {
            loadingImage.gameObject.SetActive(true);
            SetImageAlpha(loadingImage, 1f);
        }

        StartLoadingAnimation(
            loadingText,
            ref firstLoadingTextCoroutine
        );

        yield return WaitForVideoPrepared(
            videoPlayer
        );

        StopLoadingAnimation(
            loadingText,
            ref firstLoadingTextCoroutine
        );

        if (loadingImage != null)
        {
            loadingImage.gameObject.SetActive(false);
        }

        rawImage.gameObject.SetActive(true);

        Color color = rawImage.color;
        color.a = 0f;
        rawImage.color = color;

        videoPlayer.Play();

        yield return rawImage
            .DOFade(1f, 0.35f)
            .SetEase(Ease.OutQuad)
            .WaitForCompletion();
    }


    // =========================================================
    // VIDEO FADE OUT
    // =========================================================

    private IEnumerator FadeOutVideo(
        VideoPlayer videoPlayer,
        RawImage rawImage,
        float duration
    )
    {
        if (
            videoPlayer == null ||
            rawImage == null
        )
        {
            yield break;
        }


        yield return rawImage
            .DOFade(
                0f,
                duration
            )
            .SetEase(Ease.InQuad)
            .WaitForCompletion();


        rawImage
            .gameObject
            .SetActive(false);


        videoPlayer.Stop();
    }
}