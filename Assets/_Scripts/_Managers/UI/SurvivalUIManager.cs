using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class SurvivalUIManager : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] private MeshRenderer backgroundImage;

    [Header("Run UI")]
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI loopText;
    [SerializeField] private SceneTransition sceneTransition;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button menuButton;

    [Header("End Screen")]
    [SerializeField] private GameObject endScreen;
    [SerializeField] private InputField enterNameField;
    [SerializeField] private GameObject PausePanel;

    private EventBinding<OnRoadEnvironmentChanged> envBinding;
    private EventBinding<OnPlayerDeathEvent> onDeathBinding;
    private EventBinding<OnRoadSectionChanged> sectionBinding;
    private EventBinding<OnLevelUpdateEvent> updateEventBinding;
    private EventBinding<OnPauseEvent> pauseEventBinding;

    private void OnEnable()
    {
        envBinding = new EventBinding<OnRoadEnvironmentChanged>(OnEnvironmentChanged);
        EventBus<OnRoadEnvironmentChanged>.Register(envBinding);

        onDeathBinding = new EventBinding<OnPlayerDeathEvent>(OnSurvivalEnded);
        EventBus<OnPlayerDeathEvent>.Register(onDeathBinding);

        updateEventBinding = new EventBinding<OnLevelUpdateEvent>(OnUpdateEvent);
        EventBus<OnLevelUpdateEvent>.Register(updateEventBinding);

        pauseEventBinding = new EventBinding<OnPauseEvent>(OnPauseEventTriggered);
        EventBus<OnPauseEvent>.Register(pauseEventBinding);

    }

    public void ResumeGame()
    {
        GameManager.Instance.ChangeState(GameState.Playing);
    }

    private void OnPauseEventTriggered(OnPauseEvent @event)
    {
        if (@event.isPaused)
        {
            PausePanel.SetActive(true);
            Button button = PausePanel.GetComponentInChildren<Button>();
            button.Select();
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
        else
        {
            PausePanel.SetActive(false);
        }
    }

    private void Start()
    {
        endScreen.SetActive(false);
        SetDistance(0);
        SetLoop(0);
        SetScore(0);
    }

    #region ENVIRONMENT (BACKGROUND)

    private void OnEnvironmentChanged(OnRoadEnvironmentChanged e)
    {
        StartCoroutine(TransitionBackground(e.environmentPreset));
    }

    private IEnumerator TransitionBackground(EnvironmentPreset env)
    {
        if (env == null || env.background == null)
            yield break;

        backgroundImage.material = env.background;
    }


    #endregion


    #region RUN UI (external updates)

    public void SetDistance(float value)
    {
        distanceText.text = $"{value:0.0} m";
    }

    public void SetScore(int value)
    {
        scoreText.text = value.ToString();
    }

    public void SetLoop(int loop)
    {
        loopText.text = $"LOOP {loop}";
        loopText.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f);
    }

    #endregion

    #region END RUN

    private void OnSurvivalEnded(OnPlayerDeathEvent e)
    {
        StartCoroutine(LoseSequence());

        scoreText.text = $"SCORE: {e.score}";
        distanceText.text = $"DISTANCE: {e.distance:0.0} m";
        loopText.text = $"STAGE: {e.loops}";
    } 
    
    private void OnUpdateEvent(OnLevelUpdateEvent e)
    {
        lastScore = e.score;
        scoreText.text = $"SCORE: {e.score}";
        distanceText.text = $"DISTANCE: {e.distance:0.0} m";
        loopText.text = $"STAGE: {e.loops}";
    }

    [SerializeField] Image despedidoImage;
    [SerializeField] TextMeshProUGUI countDownText;
    [SerializeField] Image countDown;
    [SerializeField] AudioClip countdownBeep;
    [SerializeField] LeaderboardUI leaderboardUI;
    private bool scoreSaved;
    private float lastScore;
    IEnumerator LoseSequence()
    {
        endScreen.gameObject.SetActive(true);
        despedidoImage.gameObject.SetActive(true);
        enterNameField.gameObject.SetActive(false);
        despedidoImage.color = new Color(1, 1, 1, 0);

        yield return despedidoImage.DOFade(1, 0.5f).WaitForCompletion();

        yield return new WaitForSeconds(3f);


        bool isHighScore =
    LeaderBoardManager.IsHighScore(lastScore);

        enterNameField.gameObject.SetActive(isHighScore);

        // Mostrar countdown


        countDown.gameObject.SetActive(true);
        retryButton.Select();
        EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
        countDown.color = new Color(1, 1, 1, 0);
        countDownText.alpha = 0;

        yield return countDown.DOFade(1, 0.5f).WaitForCompletion();
        yield return countDownText.DOFade(1, 0.5f).WaitForCompletion();
        retryButton.onClick.AddListener(() =>
        {
            sceneTransition.StartTransition(SceneTransition.transitionTo.Survival);   
        });
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
        sceneTransition.StartTransition(SceneTransition.transitionTo.Survival);
    }
    public void SaveScore()
    {
        if (scoreSaved)
            return;

        string playerName = enterNameField.text.Trim();

        if (string.IsNullOrEmpty(playerName))
            playerName = "PLAYER";

        LeaderBoardManager.AddScore(playerName, lastScore);

        scoreSaved = true;

        enterNameField.gameObject.SetActive(false);

        leaderboardUI.Refresh();
    }
    #endregion

    private void OnDisable()
    {
        EventBus<OnRoadEnvironmentChanged>.Deregister(envBinding);
        EventBus<OnPlayerDeathEvent>.Deregister(onDeathBinding);
        EventBus<OnRoadSectionChanged>.Deregister(sectionBinding);
        EventBus<OnPauseEvent>.Deregister(pauseEventBinding);
    }
}