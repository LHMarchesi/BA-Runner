using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Gameplay_UIHandler : MonoBehaviour
{
    [SerializeField] GameObject loseScreen;
    [SerializeField] GameObject winBtnContinue;
    [SerializeField] GameObject restartBtn;
    [SerializeField] MeshRenderer BackgroundImage;
    [SerializeField] Image despedidoImage;
    [SerializeField] GameObject pausePanel;
    [SerializeField] Image countDown;
    [SerializeField] Image WinImage;
    [SerializeField] Image[] stars;
    [SerializeField] TextMeshProUGUI countDownText;
    [SerializeField] SceneTransition transitionToCinematics;

    [SerializeField] AudioClip countdownBeep;
    [SerializeField] private AudioClip starCollectSFX;
    [SerializeField] private AudioClip lvlCompleted;

    EventBinding<OnLevelCompletedEvent> levelResultBinding;
    EventBinding<OnPlayerDeathEvent> playerDeathBinding;
    private EventBinding<OnRoadEnvironmentChanged> envBinding;
    private EventBinding<OnPauseEvent> pauseEventBinding;
    private void OnEnable()
    {
        InitializeEvents();

    }

    private void OnPauseEventTriggered(OnPauseEvent @event)
    {
        if (@event.isPaused)
        {
            pausePanel.SetActive(true);
        }
        else
        {
            pausePanel.SetActive(false);
        }
    }

    private void Start()
    {
        loseScreen.gameObject.SetActive(false);

        var currentLevel = ProgressionManager.Instance.CurrentLevel;
    }

    public void ResumeGame()
    {
        GameManager.Instance.ChangeState(GameState.Playing);
    }
    IEnumerator LoseSequence()
    {
        loseScreen.gameObject.SetActive(true);
        despedidoImage.gameObject.SetActive(true);
        despedidoImage.color = new Color(1, 1, 1, 0);

        yield return despedidoImage.DOFade(1, 0.5f).WaitForCompletion();

        yield return new WaitForSeconds(3f);

        // Mostrar countdown
        countDown.gameObject.SetActive(true);
        countDownText.gameObject.SetActive(true);

        countDown.color = new Color(1, 1, 1, 0);
        countDownText.alpha = 0;

        yield return countDown.DOFade(1, 0.5f).WaitForCompletion();
        yield return countDownText.DOFade(1, 0.5f).WaitForCompletion();

        //  Countdown retro
        for (int i = 10; i >= 0; i--)
        {
            countDownText.text = "CONTINUE " + i.ToString() + "?";
            AudioManager.Instance.PlaySFX(countdownBeep);
            // punch (retro feel)
            countDownText.transform.localScale = Vector3.one * 1.5f;
            countDownText.transform.DOScale(1f, 0.2f);

            yield return new WaitForSeconds(1f);
        }

        SceneManager.LoadScene("SampleScene");
        GameManager.Instance.ChangeState(GameState.Playing);
    }

    IEnumerator WinSequence(int starsEarned)
    {
        foreach (var star in stars)
        {
            star.gameObject.SetActive(false);
        }
        winBtnContinue.gameObject.SetActive(false);
        restartBtn.gameObject.SetActive(false);
        WinImage.sprite = ProgressionManager.Instance.CurrentLevel.winLevelImage;
        WinImage.gameObject.SetActive(true);
        WinImage.color = new Color(1, 1, 1, 0);

        yield return WinImage.DOFade(1, 0.5f).WaitForCompletion();

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < starsEarned; i++)
        {
            var star = stars[i];
            star.gameObject.SetActive(true);
            star.transform.localScale = Vector3.zero;
            star.color = new Color(1, 1, 1, 0);

        }

        for (int i = 0; i < starsEarned; i++)
        {
            var star = stars[i];
            star.DOFade(1, 0.5f).WaitForCompletion();
            DG.Tweening.Sequence seq = DOTween.Sequence();

            seq.Append(star.transform.DOScale(1.3f, 0.2f).SetEase(Ease.OutBack)); // crece con rebote
            seq.Append(star.transform.DOScale(1.4f, 0.1f)); // vuelve a tamaño normal

            // pequeño stretch (feeling extra)
            seq.Join(star.transform.DOScaleY(0.8f, 0.1f).SetLoops(2, LoopType.Yoyo));

            yield return seq.WaitForCompletion();
            AudioManager.Instance.PlaySFX(starCollectSFX);
            yield return new WaitForSeconds(0.5f); // delay entre estrellas
        }

        AudioManager.Instance.PlaySFX(lvlCompleted);
        yield return new WaitForSeconds(.2f);
        winBtnContinue.gameObject.SetActive(true);
        yield return new WaitForSeconds(.5f);
        restartBtn.gameObject.SetActive(true);

    }

    private void OnEnvironmentChanged(OnRoadEnvironmentChanged e)
    {
        StartCoroutine(TransitionBackground(e.environmentPreset));
    }

    private IEnumerator TransitionBackground(EnvironmentPreset env)
    {
        if (env == null || env.background == null)
            yield break;


        BackgroundImage.material = env.background;

    }

    void InitializeEvents()
    {
        envBinding = new EventBinding<OnRoadEnvironmentChanged>(OnEnvironmentChanged);
        EventBus<OnRoadEnvironmentChanged>.Register(envBinding);

        levelResultBinding = new EventBinding<OnLevelCompletedEvent>(OnLevelResult);
        EventBus<OnLevelCompletedEvent>.Register(levelResultBinding);

        playerDeathBinding = new EventBinding<OnPlayerDeathEvent>(OnPlayerDeath);
        EventBus<OnPlayerDeathEvent>.Register(playerDeathBinding);

        pauseEventBinding = new EventBinding<OnPauseEvent>(OnPauseEventTriggered);
        EventBus<OnPauseEvent>.Register(pauseEventBinding);
    }

    void OnLevelResult(OnLevelCompletedEvent e)
    {
        StartCoroutine(WinSequence(e.stars));
    }

    void OnPlayerDeath(OnPlayerDeathEvent e)
    {
        StartCoroutine(LoseSequence());
    }


    private void OnDisable()
    {
        EventBus<OnLevelCompletedEvent>.Deregister(levelResultBinding);
        EventBus<OnRoadEnvironmentChanged>.Deregister(envBinding);
        EventBus<OnPlayerDeathEvent>.Deregister(playerDeathBinding);
    }
}
