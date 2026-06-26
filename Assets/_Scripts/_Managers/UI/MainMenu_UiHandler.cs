using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenu_UiHandler : MonoBehaviour
{
    [SerializeField] Image pressAnyKeyimage;
    [SerializeField] private Button[] buttons;
    [SerializeField] private RectTransform indicator;
    [SerializeField] RawImage modeSelectImage;
    [SerializeField] Image firstBackgroundImage;
    private int currentButtonIndex = 0;
    [SerializeField] AudioClip menuAppearSFX;
    [SerializeField] SceneTransition transition;
    [Header("Input")]
    [SerializeField] private InputActionReference navigateAction;
    [SerializeField] private InputActionReference submitAction;
    EventBinding<OnEnterMenuEvent> onEnterMenuBinding;
    private Image overlayNegro;
    bool isMenuActive = false;

    private void Awake()
    {
        PrepareImage(pressAnyKeyimage);
        PrepareButton(buttons[0].gameObject, SceneTransition.transitionTo.Cinematics);
        PrepareButton(buttons[1].gameObject, SceneTransition.transitionTo.Survival);
        PrepareButton(buttons[2].gameObject, SceneTransition.transitionTo.Exit);

    }

    private void OnEnable()
    {
        navigateAction.action.Enable();
        submitAction.action.Enable();

        navigateAction.action.performed += OnNavigate;
        submitAction.action.performed += OnSubmit;

        onEnterMenuBinding =
            new EventBinding<OnEnterMenuEvent>(OnEnterMenu);

        EventBus<OnEnterMenuEvent>.Register(onEnterMenuBinding);
    }
    private void OnNavigate(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        if (input.y > 0.5f)
        {
            currentButtonIndex--;

            if (currentButtonIndex < 0)
                currentButtonIndex = buttons.Length - 1;

            UpdateButtonSelection();
        }
        else if (input.y < -0.5f)
        {
            currentButtonIndex++;

            if (currentButtonIndex >= buttons.Length)
                currentButtonIndex = 0;

            UpdateButtonSelection();
        }
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (!isMenuActive) return;
        buttons[currentButtonIndex].Select();
        buttons[currentButtonIndex].onClick.Invoke();
    }

    private void OnDisable()
    {
        EventBus<OnEnterMenuEvent>.Deregister(onEnterMenuBinding);
    }

    private void Update()
    {
        if (Keyboard.current != null && !isMenuActive)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                pressAnyKeyimage.gameObject.SetActive(false);
                StartCoroutine(ModeSelectionSequence());
            }
        }
    }


    void PrepareButton(GameObject obj, SceneTransition.transitionTo transitionTo)
    {
        obj.SetActive(false);


        Button button = obj.GetComponent<Button>();
        button.interactable = false;
        button.onClick.AddListener(() =>
        {
            transition.StartTransition(transitionTo);
        });
    }
    private void UpdateButtonSelection()
    {
        if (!indicator.gameObject.activeSelf)
            indicator.gameObject.SetActive(true);

        indicator.anchoredPosition =
            buttons[currentButtonIndex]
            .GetComponent<RectTransform>()
            .anchoredPosition;
    }
    void PrepareImage(Image image)
    {
        image.gameObject.SetActive(false);

        image.transform.localScale = Vector3.zero;


        Color color = image.color;
        color.a = 0;

        image.color = color;
    }

    void OnEnterMenu(OnEnterMenuEvent e)
    {
        StartCoroutine(MenuSequence());
    }

    IEnumerator MenuSequence()
    {
        yield return new WaitForSeconds(.75f);
        yield return AnimateImage(pressAnyKeyimage);
    }

    IEnumerator ModeSelectionSequence()
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(firstBackgroundImage.DOFade(0f, 1.35f));

        yield return seq.WaitForCompletion();

        Sequence seqq = DOTween.Sequence();
        modeSelectImage.gameObject.SetActive(true);
        Color c = modeSelectImage.color;
        c.a = 0f;
        modeSelectImage.color = c;
        seqq.Append(modeSelectImage.DOFade(1f, 1.35f));
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(true);
            buttons[i].interactable = true;
        }
        UpdateButtonSelection();
        isMenuActive = true;

        yield return seqq.WaitForCompletion();
       
    }


    IEnumerator AnimateImage(Image image)  // animacion de aparacion podria ser con Animator
    {
        image.gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();

        seq.Append(
            image.DOFade(1f, 0.35f)
        );

        yield return seq.WaitForCompletion();
        StartPulse(image);
    }


    void StartPulse(Image image)
    {
        // Infinite Yoyo Pulse (Scale 1.0 <-> 1.2)
        image.transform.DOScale(1f, 1.2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetAutoKill(false);

        // Optional: Color Pulse (Neon Arcade Style)
        image.DOColor(Color.red, 1.1f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetAutoKill(false);
    }
}