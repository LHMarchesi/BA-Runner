using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using BARunner.Survival;

public class SurvivalUIManager : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] private MeshRenderer backgroundImage;

    [Header("Run UI")]
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI loopText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private SceneTransition sceneTransition;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button menuButton;

    [Header("End Screen")]
    [SerializeField] private GameObject endScreen;
    [SerializeField] private InputField enterNameField;
    [SerializeField] private GameObject PausePanel;

    [Header("Party / BA Runner Result UI")]
    [Tooltip("Optional. If empty, the manager is found in the active scene.")]
    [SerializeField] private SurvivalManager survivalManager;
    [Tooltip("Create a TMP text under End Screen and assign it here to show the personal best.")]
    [SerializeField] private TextMeshProUGUI bestSurvivalScoreText;
    [Tooltip("Create a TMP text under End Screen and assign it here to show NEW RECORD! with a pulse.")]
    [SerializeField] private TextMeshProUGUI newRecordText;
    [SerializeField, Range(1.02f, 1.5f)] private float newRecordPulseScale = 1.14f;
    [SerializeField, Min(0.05f)] private float newRecordPulseDuration = 0.42f;

    private EventBinding<OnRoadEnvironmentChanged> envBinding;
    private EventBinding<OnPlayerDeathEvent> onDeathBinding;
    private EventBinding<OnRoadSectionChanged> sectionBinding;
    private EventBinding<OnLevelUpdateEvent> updateEventBinding;
    private EventBinding<OnPauseEvent> pauseEventBinding;

    private float survivalTime;
    private bool timerRunning;
    private bool hasHandledEnd;
    private bool scoreSaved;
    private float lastScore;
    private Tween newRecordPulseTween;
    private Vector3 newRecordBaseScale;
    private bool hasNewRecordBaseScale;

    [SerializeField] private Image despedidoImage;
    [SerializeField] private TextMeshProUGUI countDownText;
    [SerializeField] private Image countDown;
    [SerializeField] private AudioClip countdownBeep;
    [SerializeField] private LeaderboardUI leaderboardUI;

    private void Awake()
    {
        if (survivalManager == null)
            survivalManager = FindObjectOfType<SurvivalManager>();
    }

    private void OnEnable()
    {
        envBinding = new EventBinding<OnRoadEnvironmentChanged>(OnEnvironmentChanged);
        EventBus<OnRoadEnvironmentChanged>.Register(envBinding);

        // Keep the legacy event path for any other mode/system that still raises it.
        // Survival Solo now uses SurvivalManager.SoloRunEnded instead.

        updateEventBinding = new EventBinding<OnLevelUpdateEvent>(OnUpdateEvent);
        EventBus<OnLevelUpdateEvent>.Register(updateEventBinding);

        pauseEventBinding = new EventBinding<OnPauseEvent>(OnPauseEventTriggered);
        EventBus<OnPauseEvent>.Register(pauseEventBinding);

        if (survivalManager == null)
            survivalManager = FindObjectOfType<SurvivalManager>();

        if (survivalManager != null)
        {
            survivalManager.SoloRunEnded -= HandleSoloRunEnded;
            survivalManager.SoloRunEnded += HandleSoloRunEnded;
        }
        else
        {
            Debug.LogError("[SurvivalUIManager] No SurvivalManager found. Solo result UI cannot receive the completed run.", this);
        }
    }

    private void Start()
    {
        survivalTime = 0f;
        timerRunning = true;
        hasHandledEnd = false;
        scoreSaved = false;
        lastScore = 0f;

        if (endScreen != null)
            endScreen.SetActive(false);
        else
            Debug.LogError("[SurvivalUIManager] End Screen is not assigned in the Inspector.", this);

        if (newRecordText != null)
            newRecordText.gameObject.SetActive(false);

        SetDistance(0f);
        SetLoop(0);
        SetScore(0);
        SetTime(0f);
    }

    private void Update()
    {
        if (!timerRunning)
            return;

        survivalTime += Time.deltaTime;
        SetTime(survivalTime);
    }

    public void ResumeGame()
    {
        GameManager.Instance.ChangeState(GameState.Playing);
    }

    private void OnPauseEventTriggered(OnPauseEvent pauseEvent)
    {
        if (pauseEvent.isPaused)
        {
            if (PausePanel == null) return;
            PausePanel.SetActive(true);
            Button button = PausePanel.GetComponentInChildren<Button>();
            if (button != null)
            {
                button.Select();
                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(button.gameObject);
            }
        }
        else if (PausePanel != null)
        {
            PausePanel.SetActive(false);
        }
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

        if (backgroundImage != null)
            backgroundImage.material = env.background;
    }

    #endregion

    #region RUN UI

    public void SetDistance(float value)
    {
        if (distanceText != null)
            distanceText.text = $"{value:0.0} m";
    }

    public void SetTime(float seconds)
    {
        if (timeText == null) return;

        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        timeText.text = $"TIME: {minutes:00}:{secs:00}";
    }

    public void SetScore(int value)
    {
        if (scoreText != null)
            scoreText.text = value.ToString();
    }

    public void SetLoop(int loop)
    {
        if (loopText == null) return;
        loopText.text = $"LOOP {loop}";
        loopText.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f);
    }

    #endregion

    #region END RUN

    /// <summary>
    /// Called by SurvivalManager after the Party-linked result has been recorded.
    /// This is the canonical Solo completion path for this UI.
    /// </summary>
    private void HandleSoloRunEnded(
        SurvivalRunResult result,
        BARunnerStatistics statistics,
        bool isNewRecord)
    {
        if (hasHandledEnd)
            return;

        if (result == null)
        {
            Debug.LogError("[SurvivalUIManager] Received a null SurvivalRunResult.", this);
            return;
        }

        hasHandledEnd = true;
        timerRunning = false;

        // Use the exact same score that BA Runner stored as the profile record.
        // This keeps the local leaderboard and Party-linked statistics consistent.
        lastScore = result.Score;
        survivalTime = result.SurvivalTimeSeconds;

        if (scoreText != null)
            scoreText.text = $"SCORE: {result.Score:N0}";
        if (distanceText != null)
            distanceText.text = $"DISTANCE: {result.Distance:0.0} m";
        if (loopText != null)
            loopText.text = $"STAGE: {result.StagesCompleted}";
        SetTime(survivalTime);

        if (bestSurvivalScoreText != null)
        {
            bestSurvivalScoreText.text = statistics != null
                ? $"BEST SCORE: {statistics.bestSurvivalScore:N0}"
                : "BEST SCORE: —";
        }

        if (newRecordText != null)
        {
            newRecordText.text = "NEW RECORD!";
            newRecordText.gameObject.SetActive(isNewRecord);
        }
        else if (isNewRecord)
        {
            Debug.LogWarning(
                "[SurvivalUIManager] New record saved, but New Record Text is not assigned. Create a TextMeshProUGUI under End Screen and assign it in the Inspector.",
                this);
        }

        Debug.Log(
            "[SurvivalUIManager] Received SoloRunEnded | Score: " + result.Score +
            " | Distance: " + result.Distance.ToString("F1") +
            " | Stages: " + result.StagesCompleted +
            " | Time: " + result.SurvivalTimeSeconds.ToString("F1") +
            " | New record: " + isNewRecord,
            this);

        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();

        StartCoroutine(LoseSequence());
    }

    // Legacy fallback for systems that still raise OnPlayerDeathEvent.
    // PlayerController's Survival branch does not raise this event.
    private void OnUpdateEvent(OnLevelUpdateEvent e)
    {
        // Do not let late HUD broadcasts overwrite the final result screen.
        if (hasHandledEnd)
            return;

        lastScore = e.score;
        if (scoreText != null) scoreText.text = $"SCORE: {e.score}";
        if (distanceText != null) distanceText.text = $"DISTANCE: {e.distance:0.0} m";
        if (loopText != null) loopText.text = $"STAGE: {e.loops}";
    }

    private IEnumerator LoseSequence()
    {
        if (endScreen == null)
        {
            Debug.LogError("[SurvivalUIManager] Cannot show defeat sequence because End Screen is not assigned.", this);
            yield break;
        }

        endScreen.SetActive(true);

        if (newRecordText != null && newRecordText.gameObject.activeSelf)
            StartNewRecordPulse();

        if (despedidoImage != null)
        {
            despedidoImage.gameObject.SetActive(true);
            despedidoImage.color = new Color(1f, 1f, 1f, 0f);
            yield return despedidoImage.DOFade(1f, 0.5f).WaitForCompletion();
        }

        if (enterNameField != null)
            enterNameField.gameObject.SetActive(false);

        yield return new WaitForSeconds(3f);

        bool isHighScore = LeaderBoardManager.IsHighScore(lastScore);
        if (enterNameField != null)
            enterNameField.gameObject.SetActive(isHighScore);

        if (countDown != null)
        {
            countDown.gameObject.SetActive(true);
            countDown.color = new Color(1f, 1f, 1f, 0f);
        }

        if (retryButton != null)
        {
            retryButton.Select();
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
        }

        if (countDownText != null)
            countDownText.alpha = 0f;

        if (countDown != null)
            yield return countDown.DOFade(1f, 0.5f).WaitForCompletion();
        if (countDownText != null)
            yield return countDownText.DOFade(1f, 0.5f).WaitForCompletion();

        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(RetrySurvival);
            retryButton.onClick.AddListener(RetrySurvival);
        }

        for (int i = 10; i >= 0; i--)
        {
            if (countDownText != null)
            {
                countDownText.text = "CONTINUE " + i + "?";
                countDownText.transform.localScale = Vector3.one * 1.5f;
                countDownText.transform.DOScale(1f, 0.2f);
            }

            if (AudioManager.Instance != null && countdownBeep != null)
                AudioManager.Instance.PlaySFX(countdownBeep);

            yield return new WaitForSeconds(1f);
        }

        RetrySurvival();
    }

    private void StartNewRecordPulse()
    {
        if (newRecordText == null) return;

        StopNewRecordPulse();
        newRecordBaseScale = newRecordText.transform.localScale;
        hasNewRecordBaseScale = true;

        newRecordPulseTween = newRecordText.transform
            .DOScale(newRecordBaseScale * newRecordPulseScale, newRecordPulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    private void StopNewRecordPulse()
    {
        if (newRecordPulseTween != null)
        {
            newRecordPulseTween.Kill();
            newRecordPulseTween = null;
        }

        if (newRecordText != null && hasNewRecordBaseScale)
            newRecordText.transform.localScale = newRecordBaseScale;

        hasNewRecordBaseScale = false;
    }

    public void SaveScore()
    {
        if (scoreSaved)
            return;

        string playerName = enterNameField != null ? enterNameField.text.Trim() : string.Empty;
        if (string.IsNullOrEmpty(playerName))
            playerName = "PLAYER";

        LeaderBoardManager.AddScore(playerName, lastScore);
        scoreSaved = true;

        if (enterNameField != null)
            enterNameField.gameObject.SetActive(false);

        if (leaderboardUI != null)
            leaderboardUI.Refresh();
    }

    private void RetrySurvival()
    {
        if (sceneTransition != null)
            sceneTransition.StartTransition(SceneTransition.transitionTo.Survival);
        else
            Debug.LogError("[SurvivalUIManager] SceneTransition is not assigned; cannot retry Survival.", this);
    }

    #endregion

    private void OnDisable()
    {
        if (survivalManager != null)
            survivalManager.SoloRunEnded -= HandleSoloRunEnded;

        StopNewRecordPulse();

        if (envBinding != null)
            EventBus<OnRoadEnvironmentChanged>.Deregister(envBinding);
        if (sectionBinding != null)
            EventBus<OnRoadSectionChanged>.Deregister(sectionBinding);
        if (pauseEventBinding != null)
            EventBus<OnPauseEvent>.Deregister(pauseEventBinding);
        if (updateEventBinding != null)
            EventBus<OnLevelUpdateEvent>.Deregister(updateEventBinding);
    }
}
