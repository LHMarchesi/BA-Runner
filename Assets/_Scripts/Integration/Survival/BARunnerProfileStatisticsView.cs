using System;
using Bunker26.Party;
using Bunker26.Party.Samples;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace BARunner.Survival
{
    /// <summary>
    /// Adds BA Runner statistics to the Party profile details screen.
    /// Keep this component in the BA Runner project, not in the Party package.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BARunnerProfileStatisticsView : MonoBehaviour
    {
        [SerializeField] private PartySetupScreenView profileScreen;
        [SerializeField] private string sectionTitle = "BA RUNNER - SURVIVAL";
        [SerializeField] private bool expandStatsTextToFit = true;

        private BARunnerStatisticsService _statisticsService;

        // Installs this view extension automatically. This removes the easy-to-miss manual
        // AddComponent step from the Party profile screen setup.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneInstaller()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PartySetupScreenView[] screens = UnityEngine.Object.FindObjectsOfType<PartySetupScreenView>(true);
            for (int i = 0; i < screens.Length; i++)
            {
                PartySetupScreenView screen = screens[i];
                if (screen == null) continue;

                BARunnerProfileStatisticsView extension = screen.GetComponent<BARunnerProfileStatisticsView>();
                if (extension == null)
                    extension = screen.gameObject.AddComponent<BARunnerProfileStatisticsView>();

                extension.Bind(screen);
            }
        }

        private void Awake()
        {
            _statisticsService = new BARunnerStatisticsService();
            if (profileScreen == null)
                profileScreen = GetComponent<PartySetupScreenView>();
        }

        private void Bind(PartySetupScreenView screen)
        {
            if (screen == null) return;
            if (profileScreen != null)
                profileScreen.ProfileDetailsDisplayed -= AppendStatistics;

            profileScreen = screen;
            if (_statisticsService == null)
                _statisticsService = new BARunnerStatisticsService();

            profileScreen.ProfileDetailsDisplayed -= AppendStatistics;
            profileScreen.ProfileDetailsDisplayed += AppendStatistics;
        }

        private void OnEnable()
        {
            if (profileScreen == null)
                profileScreen = GetComponent<PartySetupScreenView>();

            if (profileScreen != null)
                Bind(profileScreen);
            else
                Debug.LogError(
                    "[BARunnerProfileStatisticsView] PartySetupScreenView reference is missing. Assign it in the Inspector.",
                    this);
        }

        private void OnDisable()
        {
            if (profileScreen != null)
                profileScreen.ProfileDetailsDisplayed -= AppendStatistics;
        }

        private void AppendStatistics(PlayerProfile profile, Text target)
        {
            if (profile == null || target == null)
                return;

            try
            {
                BARunnerStatistics statistics = _statisticsService.Load(profile.id);
                target.text += "\n\n" + sectionTitle + "\n" +
                               "Runs jugadas: " + statistics.survivalRunsPlayed + "\n" +
                               "Récord de score: " + statistics.bestSurvivalScore.ToString("N0") + "\n" +
                               "Récord de distancia: " + statistics.bestSurvivalDistance.ToString("N0") + "\n" +
                               "Récord de stages: " + statistics.bestSurvivalStages + "\n" +
                               "Tiempo total: " + FormatTime(statistics.totalSurvivalTimeSeconds);

                if (expandStatsTextToFit && target.rectTransform != null)
                {
                    // The original Party sample text box was sized for six generic rows.
                    // Recalculate its required height so the appended game-specific rows are not clipped.
                    target.verticalOverflow = VerticalWrapMode.Overflow;
                    float requiredHeight = target.preferredHeight + 8f;
                    target.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                        Mathf.Max(target.rectTransform.rect.height, requiredHeight));
                }

                Debug.Log("[BARunnerProfileStatisticsView] Loaded BA Runner stats for profile " +
                          profile.id + " | Runs: " + statistics.survivalRunsPlayed +
                          " | Best score: " + statistics.bestSurvivalScore +
                          " | File: " + _statisticsService.GetFilePathForDiagnostics(profile.id), this);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    "[BARunnerProfileStatisticsView] Could not load BA Runner statistics for profile '" +
                    profile.id + "'. " + exception.Message,
                    this);

                target.text += "\n\n" + sectionTitle + "\nEstadísticas no disponibles.";
            }
        }

        private static string FormatTime(float totalSeconds)
        {
            int seconds = Mathf.Max(0, Mathf.FloorToInt(totalSeconds));
            int hours = seconds / 3600;
            int minutes = (seconds % 3600) / 60;
            int remainingSeconds = seconds % 60;

            if (hours > 0)
                return hours + " h " + minutes + " min";
            if (minutes > 0)
                return minutes + " min " + remainingSeconds + " s";
            return remainingSeconds + " s";
        }
    }
}
