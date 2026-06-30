using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenu_UiHandler : MonoBehaviour
{
    [SerializeField] Image pressAnyKeyimage;
    [SerializeField] private Button[] buttons;
    [SerializeField] private MenuNavigation navigation;
    [SerializeField] private GameObject indicator;
    [SerializeField] Image modeSelectImage;
    [SerializeField] Image firstBackgroundImage;
    [SerializeField] AudioClip menuAppearSFX;
    [SerializeField] SceneTransition transition;
    [Header("Input")]
    EventBinding<OnEnterMenuEvent> onEnterMenuBinding;
    bool isMenuActive = false;

    private void Awake()
    {
        indicator.SetActive(false);
        PrepareImage(pressAnyKeyimage);
        PrepareButton(buttons[0].gameObject, SceneTransition.transitionTo.Cinematics);
        PrepareButton(buttons[1].gameObject, SceneTransition.transitionTo.Survival);
        PrepareButton(buttons[2].gameObject, SceneTransition.transitionTo.Exit);

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
        navigation.gameObject.SetActive(true);
        seqq.Append(modeSelectImage.DOFade(1f, 1.35f));
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(true);
            buttons[i].interactable = true;
        }
        indicator.SetActive(true);
        buttons[0].Select();
        EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
       

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
