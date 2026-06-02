using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu_UiHandler : MonoBehaviour
{
    [SerializeField] GameObject playButton;
    [SerializeField] GameObject survivalButton;

    [SerializeField] AudioClip menuAppearSFX;

    EventBinding<OnEnterMenuEvent> onEnterMenuBinding;

    private void Awake()
    {
        PrepareButton(playButton);
        PrepareButton(survivalButton);
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

    void PrepareButton(GameObject button)
    {
        button.SetActive(false);

        button.transform.localScale = Vector3.zero;

        Image image = button.GetComponent<Image>();

        Color color = image.color;
        color.a = 0;

        image.color = color;

        button.GetComponent<Button>().interactable = false;
    }

    void OnEnterMenu(OnEnterMenuEvent e)
    {
        StartCoroutine(MenuSequence());
    }

    IEnumerator MenuSequence()
    {
        yield return new WaitForSeconds(.75f);
        yield return AnimateButton(playButton);

        yield return new WaitForSeconds(0.55f);

        yield return AnimateButton(survivalButton);
    }

    IEnumerator AnimateButton(GameObject button)  // animacion de aparacion podria ser con Animator
    {
        button.SetActive(true);

        Image image = button.GetComponent<Image>();

        Vector3 originalScale = Vector3.one;

        button.transform.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();

        seq.Append(
            image.DOFade(1f, 0.35f)
        );

        seq.Join(
            button.transform
                .DOScale(1.3f, 0.45f)
                .SetEase(Ease.OutBack)
        );

        seq.Append(
            button.transform
                .DOScale(originalScale, 0.4f)
        );

        seq.Append(
            button.transform
                .DOPunchScale(
                    new Vector3(0f, -0.15f, 0f),
                    0.15f,
                    5,
                    0.5f
                )
        );

        yield return seq.WaitForCompletion();

        button.transform.localScale = originalScale;

        button.GetComponent<Button>().interactable = true;
    }
}