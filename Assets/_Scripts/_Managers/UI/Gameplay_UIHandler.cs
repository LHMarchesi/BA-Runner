using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Gameplay_UIHandler : MonoBehaviour
{
    [SerializeField] GameObject loseScreen;
    [SerializeField] GameObject indicator;
    [SerializeField] Button winBtnContinue;
    [SerializeField] Button winRestartBtn;
    [SerializeField] Button loseRestartBtn;
    [SerializeField] MeshRenderer BackgroundImage;
    [SerializeField] Image despedidoImage;
    [SerializeField] GameObject pausePanel;
    [SerializeField] Image countDown;
    [SerializeField] Image WinImage;
    [SerializeField] Image[] stars;
    [SerializeField] TextMeshProUGUI countDownText;
    [SerializeField] TextMeshProUGUI completionTime;
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
            Button button = pausePanel.GetComponentInChildren<Button>();
            button.Select();
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
        else
        {
            pausePanel.SetActive(false);
        }
    }

    private void Start()
    {
        loseScreen.gameObject.SetActive(false);
        indicator.SetActive(false);
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
        loseRestartBtn.Select();
        EventSystem.current.SetSelectedGameObject(loseRestartBtn.gameObject);
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

    IEnumerator WinSequence(int starsEarned, float completionTimeValue)
    {
        foreach (var star in stars)
        {
            star.gameObject.SetActive(false);
        }
        winBtnContinue.gameObject.SetActive(false);
        winRestartBtn.gameObject.SetActive(false);
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
        TimeSpan time = TimeSpan.FromSeconds(completionTimeValue);
        completionTime.text += $"{time.Minutes:00}:{time.Seconds:00}.{time.Milliseconds / 10:00}";
        yield return new WaitForSeconds(.2f);
        winBtnContinue.gameObject.SetActive(true);
        winBtnContinue.Select();
        EventSystem.current.SetSelectedGameObject(winBtnContinue.gameObject);
        indicator.SetActive(true);

        yield return new WaitForSeconds(.5f);
        winRestartBtn.gameObject.SetActive(true);

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
        StartCoroutine(WinSequence(e.stars, e.completionTime));
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
        EventBus<OnPauseEvent>.Deregister(pauseEventBinding);
    }
}
