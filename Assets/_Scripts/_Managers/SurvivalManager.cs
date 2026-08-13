using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
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
    [SerializeField] private TextMeshProUGUI teamScoreText;
    [SerializeField]
    private float scoreCountDuration = 0.8f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private int[] reviveProgress;
    private bool[] isReviving;

    private bool gameEnded;
    private int versusLastStanding = -1;
    private Tween scoreCountTween;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        mode =
        SurvivalRunConfig.SelectedMode;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        reviveProgress =
            new int[players.Length];

        isReviving =
            new bool[players.Length];

        for (int i = 0; i < players.Length; i++)
        {
            int index = i;

            players[i].player.Died +=
                player =>
                    HandlePlayerDeath(index);

            players[i].track.StageCompleted +=
                track =>
                    HandleStageCompleted(index);

            HideDeathOverlay(index);

            players[i].track.StartRun();
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

        UpdateAllHUD();
    }

    private void UpdateAllHUD()
    {
        int teamScore = 0;

        for (int i = 0; i < players.Length; i++)
        {
            SurvivalPlayerRig rig =
                players[i];

            if (rig.score == null)
                continue;

            if (rig.scoreText != null)
            {
                rig.scoreText.text =
                    $"SCORE {rig.score.Score}";
            }

            if (rig.stageText != null)
            {
                rig.stageText.text =
                    $"STAGE {rig.track.TotalStagesCompleted + 1}";
            }

            if (rig.distanceToStageText != null && rig.track != null)
            {
                int distance =
                    Mathf.CeilToInt(
                        rig.track.DistanceToNextStage
                    );

                rig.distanceToStageText.text =
                    $"NEXT STAGE {distance}";
            }

            teamScore += rig.score.Score;
        }

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

        players[playerIndex]
            .track
            .SetRunning(false);

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

                EndSolo();

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


    private void EndSolo()
    {
        gameEnded = true;

        StopAllTracks();
    }


    private void StopAllTracks()
    {
        foreach (
            SurvivalPlayerRig rig
            in players
        )
        {
            rig.track.SetRunning(false);
        }
    }
}