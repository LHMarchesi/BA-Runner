using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using BARunner.Survival;
public enum SurvivalMode
{
    Solo,
    Coop,
    Versus
}

[Serializable]
public class SurvivalPlayerRig
{
    [Header("Gameplay")]
    public PlayerController player;
    public SurvivalTrackController track;
    public WorldSpeed worldSpeed;
    public ScoreSystem score;

    [Header("HUD")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI distanceToStageText;
    [Header("Death UI")]
    public GameObject deathOverlay;
    public TextMeshProUGUI reviveText;
}

public class SurvivalManager : MonoBehaviour
{
    /// <summary>
    /// Raised after a Solo run has been finalized and its BA Runner statistics have
    /// been saved (or a save failure has been reported). The scene UI owns presentation.
    /// </summary>
    public event Action<SurvivalRunResult, BARunnerStatistics, bool> SoloRunEnded;
    [Header("Mode")]
    [SerializeField] private SurvivalMode mode;

    [Header("Players")]
    [SerializeField] private SurvivalPlayerRig[] players;

    [Header("Coop")]
    [SerializeField] private int stagesToRevive = 4;

    [SerializeField] private float reviveDelay = 0.75f;

    [Header("Versus")]
    [SerializeField] private float opponentSpeedIncrease = 0.08f;
    [SerializeField] private TextMeshProUGUI versusWinnerText;

    [Header("Global UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private MenuNavigation gameOverNav;
    [SerializeField] private GameObject initialButton;
    [SerializeField] private GameObject tutorialImage;
    [SerializeField] private TextMeshProUGUI teamScoreText;
    [SerializeField]
    private float scoreCountDuration = 0.8f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Solo Result Integration")]
    [SerializeField] private SurvivalResultAdapter soloResultAdapter;
    [SerializeField] private TextMeshProUGUI soloFinalScoreText;
    [SerializeField] private TextMeshProUGUI soloFinalStatsText;
    [SerializeField] private TextMeshProUGUI soloBestScoreText;
    [SerializeField] private TextMeshProUGUI soloNewRecordText;
    [SerializeField, Range(1.02f, 1.5f)] private float newRecordPulseScale = 1.14f;
    [SerializeField, Min(0.05f)] private float newRecordPulseDuration = 0.42f;

    private float survivalElapsedTime;
    private int[] reviveProgress;
    private bool[] isReviving;

    private bool gameEnded;
    private int versusLastStanding = -1;
    private Tween scoreCountTween;
    private Tween newRecordPulseTween;
    private Transform newRecordPulseTarget;
    private Vector3 newRecordPulseBaseScale;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        mode = SurvivalRunConfig.SelectedMode;
        survivalElapsedTime = 0f;
        gameEnded = false;

        PlayTutorialFade();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        StopNewRecordPulse();

        if (players == null)
        {
            Debug.LogError("[SurvivalManager] Players array no está asignado.");
            return;
        }

        reviveProgress = new int[players.Length];
        isReviving = new bool[players.Length];

        if (mode == SurvivalMode.Solo)
        {
            EnsureSoloResultTexts();
            if (soloNewRecordText != null)
                soloNewRecordText.gameObject.SetActive(false);

            if (soloResultAdapter == null)
                soloResultAdapter = GetComponent<SurvivalResultAdapter>();

            if (soloResultAdapter == null)
                soloResultAdapter = FindObjectOfType<SurvivalResultAdapter>();

            // Automatically add the adapter if it was not placed in the scene or assigned
            // in the Inspector. It lives on the manager and only handles this run's result.
            if (soloResultAdapter == null)
                soloResultAdapter = gameObject.AddComponent<SurvivalResultAdapter>();

            if (!soloResultAdapter.BeginRun())
            {
                Debug.LogWarning(
                    "[SurvivalManager] Could not capture a Party profile for this Solo run. Check the preceding SurvivalResultAdapter warning. Gameplay will continue without profile statistics.",
                    this);
            }
        }

        for (int i = 0; i < players.Length; i++)
        {
            int index = i;

            SurvivalPlayerRig rig = players[i];

            if (rig == null)
            {
                Debug.LogWarning(
                    $"[SurvivalManager] Player Rig {i} está vacío."
                );

                continue;
            }

            if (rig.player == null)
            {
                Debug.LogError(
                    $"[SurvivalManager] Player {i + 1} no tiene PlayerController asignado."
                );

                continue;
            }

            if (rig.track == null)
            {
                Debug.LogError(
                    $"[SurvivalManager] Player {i + 1} no tiene SurvivalTrackController asignado."
                );

                continue;
            }

            rig.player.SetSurvivalMode(true);

            rig.player.Died +=
                player =>
                    HandlePlayerDeath(index);

            rig.track.StageCompleted +=
                track =>
                    HandleStageCompleted(index);

            HideDeathOverlay(index);

            rig.track.StartRun();
        }

        UpdateAllHUD();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(
                GameState.Survival
            );
        }
    }


    // =========================================================
    // UPDATE HUD
    // =========================================================

    private void Update()
    {
        if (gameEnded)
            return;

        // Time.deltaTime follows the game's time scale, so normal Time.timeScale pauses
        // are excluded from the recorded Survival time.
        if (mode == SurvivalMode.Solo)
            survivalElapsedTime += Time.deltaTime;

        UpdateAllHUD();
    }

    private void UpdateAllHUD()
    {
        // Once the run has ended, preserve the last visible values.
        if (gameEnded)
            return;

        int teamScore = 0;

        if (players == null)
            return;

        for (int i = 0; i < players.Length; i++)
        {
            SurvivalPlayerRig rig = players[i];

            // Seguridad: slot vacío en el array
            if (rig == null)
                continue;

            // -------------------------
            // SCORE
            // -------------------------

            if (rig.score != null)
            {
                if (rig.scoreText != null)
                {
                    rig.scoreText.text =
                        $"SCORE {rig.score.Score}";
                }

                teamScore += rig.score.Score;
            }

            // -------------------------
            // STAGE
            // -------------------------

            if (
                rig.stageText != null &&
                rig.track != null
            )
            {
                rig.stageText.text =
                    $"STAGE {rig.track.TotalStagesCompleted + 1}";
            }

            // -------------------------
            // DISTANCE
            // -------------------------

            if (
                rig.distanceToStageText != null &&
                rig.track != null
            )
            {
                int distance =
                    Mathf.CeilToInt(
                        rig.track.DistanceToNextStage
                    );

                rig.distanceToStageText.text =
                    $"NEXT STAGE {distance}";
            }
        }

        // -------------------------
        // TEAM SCORE
        // -------------------------

        if (teamScoreText != null)
        {
            teamScoreText.text =
                $"TEAM SCORE {teamScore}";
        }
    }


    // =========================================================
    // PLAYER DEATH
    // =========================================================

    private void HandlePlayerDeath(
        int playerIndex
    )
    {
        if (gameEnded)
            return;

        // Freeze every gameplay/statistics source before dispatching the mode-specific end flow.
        FreezePlayerRig(playerIndex);

        switch (mode)
        {
            case SurvivalMode.Coop:

                HandleCoopDeath(
                    playerIndex
                );

                break;


            case SurvivalMode.Versus:

                HandleVersusDeath(
                    playerIndex
                );

                break;


            case SurvivalMode.Solo:

                EndSolo(playerIndex);

                break;
        }
    }


    // =========================================================
    // STAGE COMPLETED
    // =========================================================

    private void HandleStageCompleted(
        int playerIndex
    )
    {
        if (gameEnded)
            return;

        UpdatePlayerHUD(
            playerIndex
        );

        switch (mode)
        {
            case SurvivalMode.Coop:

                HandleCoopStage(
                    playerIndex
                );

                break;


            case SurvivalMode.Versus:

                HandleVersusStage(
                    playerIndex
                );

                break;
        }
    }


    // =========================================================
    // COOP - DEATH
    // =========================================================

    private void HandleCoopDeath(
        int playerIndex
    )
    {
        reviveProgress[playerIndex] = 0;

        ShowDeathOverlay(
            playerIndex
        );

        UpdateReviveUI(
            playerIndex
        );

        /*
         * Si murieron todos:
         * no existe nadie que pueda revivir al equipo.
         */
        if (AreAllPlayersDead())
        {
            EndCoop();
        }
    }


    // =========================================================
    // COOP - STAGE
    // =========================================================

    private void HandleCoopStage(
        int survivorIndex
    )
    {

        for (int i = 0; i < players.Length; i++)
        {
            if (i == survivorIndex)
                continue;

            if (players[i].player.IsAlive)
                continue;


            if (isReviving[i])
                continue;

            reviveProgress[i]++;

            reviveProgress[i] =
                Mathf.Min(
                    reviveProgress[i],
                    stagesToRevive
                );

            UpdateReviveUI(i);

            if (
                reviveProgress[i] >=
                stagesToRevive
            )
            {
                StartCoroutine(
                    ReviveSequence(i)
                );
            }
        }
    }


    // =========================================================
    // REVIVE SEQUENCE
    // =========================================================

    private IEnumerator ReviveSequence(
        int playerIndex
    )
    {
        isReviving[playerIndex] = true;

        if (
            players[playerIndex].reviveText != null
        )
        {
            players[playerIndex]
                .reviveText
                .text =
                "REVIVING...";
        }

        yield return new WaitForSeconds(
            reviveDelay
        );

        RevivePlayer(
            playerIndex
        );

        yield return new WaitForSeconds(
            0.15f
        );

        HideDeathOverlay(
            playerIndex
        );

        isReviving[playerIndex] = false;
    }


    private void RevivePlayer(
        int index
    )
    {
        reviveProgress[index] = 0;

        players[index]
            .player
            .Revive();

        players[index]
            .track
            .SetRunning(true);

        UpdatePlayerHUD(index);
    }


    // =========================================================
    // DEATH UI
    // =========================================================

    private void ShowDeathOverlay(int index)
    {
        GameObject overlay =
            players[index].deathOverlay;

        if (overlay == null)
            return;

        overlay.SetActive(true);
    }

    private void HideDeathOverlay(int index)
    {
        GameObject overlay =
            players[index].deathOverlay;

        if (overlay == null)
            return;

        overlay.SetActive(false);
    }


    private void UpdateReviveUI(
        int index
    )
    {
        TextMeshProUGUI text =
            players[index].reviveText;

        if (text == null)
            return;

        text.text =
            $"{reviveProgress[index]} / " +
            $"{stagesToRevive}";
    }

    private void AnimateFinalTeamScore(
    int finalScore
)
    {
        if (teamScoreText == null)
            return;

        scoreCountTween?.Kill();

        int displayedScore = 0;

        teamScoreText.text =
            $"TEAM SCORE\n{displayedScore:N0}";

        scoreCountTween =
            DOTween.To(
                () => displayedScore,
                value =>
                {
                    displayedScore = value;

                    teamScoreText.text =
                        $"TEAM SCORE\n" +
                        $"{displayedScore:N0}";
                },
                finalScore,
                scoreCountDuration
            )
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);

        gameOverNav.AssignButton(initialButton);
    }
    // =========================================================
    // HUD
    // =========================================================

    private void UpdatePlayerHUD(
        int index
    )
    {
        if (
            index < 0 ||
            index >= players.Length
        )
        {
            return;
        }

        SurvivalPlayerRig rig =
            players[index];

        if (
            rig.scoreText != null &&
            rig.score != null
        )
        {
            rig.scoreText.text =
                $"SCORE {rig.score.Score}";
        }

        if (
    rig.stageText != null &&
    rig.track != null
)
        {
            rig.stageText.text =
                $"STAGE " +
                $"{rig.track.TotalStagesCompleted + 1}";
        }
    }


    // =========================================================
    // VERSUS
    // =========================================================

    private void HandleVersusStage(
     int playerIndex
 )
    {
        if (players.Length < 2)
            return;

        int opponentIndex =
            playerIndex == 0
                ? 1
                : 0;

        /*
         * Si el adversario ya murió,
         * no necesitamos aplicarle nada.
         */
        if (
            !players[opponentIndex]
                .player
                .IsAlive
        )
        {
            return;
        }

        players[opponentIndex]
            .worldSpeed
            .AddSurvivalSpeed(
                opponentSpeedIncrease
            );

        Debug.Log(
            $"[VERSUS] P{playerIndex + 1} completó stage. " +
            $"P{opponentIndex + 1} recibe +" +
            $"{opponentSpeedIncrease * 100f:F0}% speed."
        );
    }
    public void RestartSurvival()
    {
        Time.timeScale = 1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.name
        );
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(
                GameState.MainMenu
            );
        }

        SceneManager.LoadScene(
            mainMenuSceneName
        );
    }

    private void HandleVersusDeath(
     int deadPlayerIndex
 )
    {
        if (gameEnded)
            return;

        int winnerIndex =
            deadPlayerIndex == 0
                ? 1
                : 0;

        /*
         * Por seguridad.
         */
        if (
            winnerIndex < 0 ||
            winnerIndex >= players.Length
        )
        {
            return;
        }

        EndVersus(
            winnerIndex
        );
    }


    // =========================================================
    // HELPERS
    // =========================================================

    private int CountAlivePlayers()
    {
        int count = 0;

        foreach (
            SurvivalPlayerRig rig
            in players
        )
        {
            if (rig.player.IsAlive)
                count++;
        }

        return count;
    }


    private bool AreAllPlayersDead()
    {
        return
            CountAlivePlayers() == 0;
    }


    // =========================================================
    // END CONDITIONS
    // =========================================================

    private void EndCoop()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        StopAllTracks();

        int teamScore = 0;

        foreach (
            SurvivalPlayerRig rig
            in players
        )
        {
            if (rig.score != null)
            {
                teamScore +=
                    rig.score.Score;
            }
        }

        for (
            int i = 0;
            i < players.Length;
            i++
        )
        {
            HideDeathOverlay(i);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        AnimateFinalTeamScore(
            teamScore
        );
    }


    private void EndVersus(
     int winnerIndex
 )
    {
        if (gameEnded)
            return;

        gameEnded = true;

        StopAllTracks();

        /*
         * Ocultamos overlays de crash individuales
         * porque ahora mostramos el resultado global.
         */
        for (
            int i = 0;
            i < players.Length;
            i++
        )
        {
            HideDeathOverlay(i);
        }

        if (versusWinnerText != null)
        {
            versusWinnerText.text =
                $"PLAYER {winnerIndex + 1} WINS!";
            versusWinnerText.gameObject.SetActive(true);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverNav != null)
        {
            gameOverNav.AssignButton();
        }

    }

    [Header("Tutorial")]
    [SerializeField] private float tutorialFadeInDuration = 0.35f;
    [SerializeField] private float tutorialVisibleDuration = 1.2f;
    [SerializeField] private float tutorialFadeOutDuration = 0.5f;

    private CanvasGroup tutorialCanvasGroup;
    private Sequence tutorialSequence;
    private void PlayTutorialFade()
    {
        if (tutorialImage == null)
            return;


        tutorialCanvasGroup =
            tutorialImage.GetComponent<CanvasGroup>();

        tutorialSequence?.Kill();

        tutorialImage.SetActive(true);

        tutorialCanvasGroup.alpha = 0f;

        tutorialCanvasGroup.interactable = false;

        tutorialCanvasGroup.blocksRaycasts = false;

        tutorialSequence =
            DOTween.Sequence();


        tutorialSequence.Append(
            tutorialCanvasGroup.DOFade(
                1f,
                tutorialFadeInDuration
            )
        );


        tutorialSequence.AppendInterval(
            tutorialVisibleDuration
        );


        tutorialSequence.Append(
            tutorialCanvasGroup.DOFade(
                0f,
                tutorialFadeOutDuration
            )
        );


        tutorialSequence.OnComplete(
            () =>
            {
                if (tutorialImage != null)
                {
                    tutorialImage.SetActive(false);
                }

                tutorialSequence = null;
            }
        );
    }

    private void EnsureSoloResultTexts()
    {
        if (gameOverPanel == null)
            return;

        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        if (panelRect == null)
        {
            Debug.LogWarning("[SurvivalManager] Game Over Panel has no RectTransform; automatic Solo result labels cannot be created.", this);
            return;
        }

        if (soloFinalStatsText == null)
        {
            soloFinalStatsText = FindChildText(gameOverPanel.transform, "BARunnerSoloFinalStats");
            if (soloFinalStatsText == null)
            {
                soloFinalStatsText = CreateAutoResultText(
                    "BARunnerSoloFinalStats", panelRect, new Vector2(0.5f, 0.57f),
                    new Vector2(620f, 190f), 25f, Color.white);
            }
        }

        if (soloNewRecordText == null)
        {
            soloNewRecordText = FindChildText(gameOverPanel.transform, "BARunnerSoloNewRecord");
            if (soloNewRecordText == null)
            {
                soloNewRecordText = CreateAutoResultText(
                    "BARunnerSoloNewRecord", panelRect, new Vector2(0.5f, 0.79f),
                    new Vector2(620f, 85f), 44f, new Color(1f, 0.78f, 0.25f));
            }
        }
    }

    private static TextMeshProUGUI FindChildText(Transform parent, string objectName)
    {
        if (parent == null) return null;
        Transform child = parent.Find(objectName);
        return child != null ? child.GetComponent<TextMeshProUGUI>() : null;
    }

    private static TextMeshProUGUI CreateAutoResultText(
        string objectName,
        RectTransform parent,
        Vector2 anchor,
        Vector2 size,
        float fontSize,
        Color color)
    {
        GameObject textObject = new GameObject(
            objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        rect.SetAsLastSibling();

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Max(16f, fontSize * 0.7f);
        text.fontSizeMax = fontSize;
        text.raycastTarget = false;
        text.text = string.Empty;
        return text;
    }

    private void EndSolo(int playerIndex)
    {
        if (gameEnded)
            return;

        gameEnded = true;
        StopAllTracks();
        StopNewRecordPulse();
        EnsureSoloResultTexts();

        if (players == null || playerIndex < 0 || playerIndex >= players.Length || players[playerIndex] == null)
        {
            Debug.LogError("[SurvivalManager] Cannot create the Solo result because the player's rig is missing.", this);
            ShowSoloGameOver();
            return;
        }

        SurvivalPlayerRig rig = players[playerIndex];

        int finalScore = rig.score != null ? rig.score.Score : 0;
        float finalDistance = rig.score != null ? rig.score.Distance : 0f;
        int finalStages = rig.track != null
    ? rig.track.TotalStagesCompleted + 1
    : 0;

        SurvivalRunResult result = new SurvivalRunResult(
            finalScore,
            finalDistance,
            finalStages,
            survivalElapsedTime);

        BARunnerStatistics savedStatistics = null;
        bool isNewScoreRecord = false;

        if (soloResultAdapter != null)
        {
            string saveError;
            bool saved = soloResultAdapter.TryRecordRun(
                result,
                out savedStatistics,
                out isNewScoreRecord,
                out saveError);

            if (!saved)
            {
                // A score only counts as a new record when the write succeeds.
                isNewScoreRecord = false;

                Debug.LogWarning(
                    "[SurvivalManager] Solo result could not be saved to the Party profile. " + saveError,
                    this);

                // Still try to show the previous personal best if the save failed.
                string loadError;
                if (!soloResultAdapter.TryLoadStatistics(out savedStatistics, out loadError))
                    Debug.LogWarning("[SurvivalManager] Could not load the previous Survival record. " + loadError, this);
            }
        }

        int bestScore = savedStatistics != null ? savedStatistics.bestSurvivalScore : finalScore;

        if (soloFinalScoreText != null)
            soloFinalScoreText.text = $"SCORE {finalScore:N0}";

        if (soloBestScoreText != null)
        {
            soloBestScoreText.text = savedStatistics != null
                ? $"BEST SCORE {bestScore:N0}"
                : "BEST SCORE —";
        }

        if (soloFinalStatsText != null)
        {
            string bestScoreLine = soloBestScoreText == null
                ? (savedStatistics != null ? $"BEST SCORE {bestScore:N0}\n" : "BEST SCORE —\n")
                : string.Empty;

            string currentScoreLine = soloFinalScoreText == null
                ? $"SCORE {finalScore:N0}\n"
                : string.Empty;
            string newRecordLine = isNewScoreRecord && soloNewRecordText == null
                ? "NEW RECORD!\n"
                : string.Empty;

            soloFinalStatsText.text =
                newRecordLine + currentScoreLine + bestScoreLine +
                $"DISTANCE {finalDistance:N0}\n" +
                $"STAGES {finalStages}\n" +
                $"TIME {FormatSurvivalTime(survivalElapsedTime)}";
        }

        if (soloNewRecordText != null)
        {
            soloNewRecordText.text = "NEW RECORD!";
            soloNewRecordText.gameObject.SetActive(isNewScoreRecord);
            if (isNewScoreRecord)
                StartNewRecordPulse(soloNewRecordText.transform);
        }
        else if (isNewScoreRecord)
        {
            Transform fallbackPulseTarget = soloFinalStatsText != null
                ? soloFinalStatsText.transform
                : (soloFinalScoreText != null ? soloFinalScoreText.transform : null);
            if (fallbackPulseTarget != null)
                StartNewRecordPulse(fallbackPulseTarget);
        }

        if (isNewScoreRecord)
            Debug.Log("[SurvivalManager] NEW SURVIVAL SCORE RECORD: " + finalScore, this);
        else if (soloResultAdapter == null || !soloResultAdapter.HasCapturedRun)
            Debug.LogWarning("[SurvivalManager] The result UI is shown, but no Party profile was captured; no BA Runner statistics were saved.", this);

        // Prefer the scene's original SurvivalUIManager (end screen, leaderboard,
        // retry countdown). Keep this manager's panel as a fallback for scenes that
        // do not contain a SurvivalUIManager.
        Action<SurvivalRunResult, BARunnerStatistics, bool> handler = SoloRunEnded;
        if (handler != null)
        {
            Debug.Log("[SurvivalManager] Dispatching Solo result to SurvivalUIManager.", this);
            handler.Invoke(result, savedStatistics, isNewScoreRecord);
        }
        else
        {
            Debug.LogWarning(
                "[SurvivalManager] No SurvivalUIManager subscribed to SoloRunEnded. Using the fallback Game Over panel.",
                this);
            ShowSoloGameOver();
        }
    }

    private void StartNewRecordPulse(Transform target)
    {
        StopNewRecordPulse();
        if (target == null)
            return;

        newRecordPulseTarget = target;
        newRecordPulseBaseScale = target.localScale;
        Vector3 targetScale = newRecordPulseBaseScale * newRecordPulseScale;

        newRecordPulseTween = target
            .DOScale(targetScale, newRecordPulseDuration)
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

        if (newRecordPulseTarget != null)
        {
            newRecordPulseTarget.localScale = newRecordPulseBaseScale;
            newRecordPulseTarget = null;
        }
    }

    private void ShowSoloGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverNav != null)
        {
            if (initialButton != null)
                gameOverNav.AssignButton(initialButton);
            else
                gameOverNav.AssignButton();
        }
    }

    private static string FormatSurvivalTime(float totalSeconds)
    {
        int wholeSeconds = Mathf.Max(0, Mathf.FloorToInt(totalSeconds));
        int hours = wholeSeconds / 3600;
        int minutes = (wholeSeconds % 3600) / 60;
        int seconds = wholeSeconds % 60;

        return hours > 0
            ? $"{hours:00}:{minutes:00}:{seconds:00}"
            : $"{minutes:00}:{seconds:00}";
    }

    private void FreezePlayerRig(int index)
    {
        if (players == null || index < 0 || index >= players.Length)
            return;

        SurvivalPlayerRig rig = players[index];
        if (rig == null)
            return;

        // Stop the track first so it cannot accumulate more distance or complete a stage.
        if (rig.track != null)
            rig.track.SetRunning(false);

        // Also stop these references directly. This protects against an Inspector setup
        // where the rig and track point to different WorldSpeed/ScoreSystem components.
        if (rig.score != null)
            rig.score.SetRunning(false);

        if (rig.worldSpeed != null)
            rig.worldSpeed.SetFrozen(true);

        if (rig.player != null)
            rig.player.SetHorizontalInput(0f);
    }

    private void StopAllTracks()
    {
        if (players == null)
            return;

        for (int i = 0; i < players.Length; i++)
            FreezePlayerRig(i);
    }

    private void OnDestroy()
    {
        tutorialSequence?.Kill();
        scoreCountTween?.Kill();
        StopNewRecordPulse();
    }
}