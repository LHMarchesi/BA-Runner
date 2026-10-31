using Bunker26.Party;
using UnityEngine;

namespace BARunner.Survival
{
    /// <summary>
    /// BA Runner-side bridge between a completed Survival run and the Party profile
    /// that owned the run when it started. This class does not belong in the Party package.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SurvivalResultAdapter : MonoBehaviour
    {
        [Header("Diagnostics")]
        [SerializeField] private bool logRunResults = true;

        private PlayerProfileService _profiles;
        private BARunnerStatisticsService _statisticsService;
        private string _capturedProfileId;
        private string _capturedParticipantId;
        private string _capturedDisplayName;
        private bool _hasCapturedRun;
        private bool _hasRecordedRun;

        public string CapturedProfileId { get { return _capturedProfileId; } }
        public string CapturedParticipantId { get { return _capturedParticipantId; } }
        public string CapturedDisplayName { get { return _capturedDisplayName; } }
        public bool HasCapturedRun { get { return _hasCapturedRun; } }

        /// <summary>
        /// Captures the Party leader/profile at the beginning of the run.
        /// Later leader changes do not change which profile receives this run's result.
        /// </summary>
        public bool BeginRun()
        {
            if (_hasCapturedRun)
                return true;

            PartyRuntime runtime;
            try
            {
                // Reuse the persistent runtime when it already exists. If Survival was launched
                // directly, GetOrCreate also restores Player 1's preferred profile from Party data.
                runtime = PartyRuntime.GetOrCreate(8, true);
            }
            catch (System.Exception exception)
            {
                Debug.LogError(
                    "[SurvivalResultAdapter] Could not initialize PartyRuntime. " + exception,
                    this);
                return false;
            }

            if (runtime == null || runtime.Party == null || runtime.Profiles == null)
            {
                Debug.LogWarning(
                    "[SurvivalResultAdapter] PartyRuntime or its services are unavailable. This run will not be linked to a Party profile.",
                    this);
                return false;
            }

            PartyParticipant leader = runtime.Party.Leader;
            if (leader == null || leader.type != PartyParticipantType.LocalHuman)
            {
                Debug.LogWarning(
                    "[SurvivalResultAdapter] No local Party leader was found. This run will not be saved to a profile.",
                    this);
                return false;
            }

            if (string.IsNullOrWhiteSpace(leader.profileId))
            {
                Debug.LogWarning(
                    "[SurvivalResultAdapter] The Party leader has no profile assigned. Select a profile before starting Survival to save statistics.",
                    this);
                return false;
            }

            PlayerProfile profile = runtime.Profiles.Get(leader.profileId);
            if (profile == null)
            {
                Debug.LogWarning(
                    "[SurvivalResultAdapter] The Party leader's profile could not be loaded. This run will not be saved.",
                    this);
                return false;
            }

            _profiles = runtime.Profiles;
            _statisticsService = new BARunnerStatisticsService();
            _capturedProfileId = profile.id;
            _capturedParticipantId = leader.id;
            _capturedDisplayName = string.IsNullOrWhiteSpace(leader.DisplayName) || leader.DisplayName == "Unknown"
                ? profile.displayName
                : leader.DisplayName;
            _hasCapturedRun = true;
            _hasRecordedRun = false;

            if (logRunResults)
            {
                Debug.Log(
                    "[SurvivalResultAdapter] Run owner captured: " + _capturedDisplayName +
                    " | Profile: " + _capturedProfileId +
                    " | Participant: " + _capturedParticipantId +
                    " | Statistics file: " + _statisticsService.GetFilePathForDiagnostics(_capturedProfileId),
                    this);
            }

            return true;
        }

        /// <summary>Records this run once against the profile captured by BeginRun.</summary>
        public bool TryRecordRun(SurvivalRunResult result, out string error)
        {
            BARunnerStatistics ignoredStatistics;
            bool ignoredNewRecord;
            return TryRecordRun(result, out ignoredStatistics, out ignoredNewRecord, out error);
        }

        /// <summary>
        /// Records the run and returns the updated profile statistics plus the new-record flag.
        /// </summary>
        public bool TryRecordRun(
            SurvivalRunResult result,
            out BARunnerStatistics updatedStatistics,
            out bool isNewScoreRecord,
            out string error)
        {
            updatedStatistics = null;
            isNewScoreRecord = false;
            error = null;

            if (!_hasCapturedRun || string.IsNullOrWhiteSpace(_capturedProfileId))
            {
                error = "No Party profile was captured when the Survival run started.";
                return false;
            }

            if (_hasRecordedRun)
            {
                error = "The result for this Survival run has already been recorded.";
                return false;
            }

            if (result == null)
            {
                error = "The Survival run result is null.";
                return false;
            }

            if (_profiles == null || _profiles.Get(_capturedProfileId) == null)
            {
                error = "The profile captured at run start no longer exists or cannot be loaded.";
                return false;
            }

            if (!_statisticsService.TryRecordRun(
                    _capturedProfileId,
                    result,
                    out updatedStatistics,
                    out isNewScoreRecord,
                    out error))
            {
                return false;
            }

            _hasRecordedRun = true;

            if (logRunResults)
            {
                Debug.Log(
                    "[SurvivalResultAdapter] Saved Survival result for " + _capturedDisplayName +
                    " | Score: " + result.Score +
                    " | Distance: " + result.Distance.ToString("F1") +
                    " | Stages: " + result.StagesCompleted +
                    " | Runs played: " + updatedStatistics.survivalRunsPlayed +
                    " | New score record: " + isNewScoreRecord +
                    " | Statistics file: " + _statisticsService.GetFilePathForDiagnostics(_capturedProfileId),
                    this);
            }

            return true;
        }

        /// <summary>Loads the captured profile's BA Runner statistics for display.</summary>
        public bool TryLoadStatistics(out BARunnerStatistics statistics, out string error)
        {
            statistics = null;
            error = null;

            if (!_hasCapturedRun || string.IsNullOrWhiteSpace(_capturedProfileId) || _statisticsService == null)
            {
                error = "No Party profile was captured for this run.";
                return false;
            }

            try
            {
                statistics = _statisticsService.Load(_capturedProfileId);
                return true;
            }
            catch (System.Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }
    }
}
