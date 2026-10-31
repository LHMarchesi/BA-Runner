using System;
using System.IO;
using UnityEngine;

namespace BARunner.Survival
{
    /// <summary>
    /// JSON persistence for BA Runner-specific statistics.
    /// Each Party profile gets its own file, keyed by the stable profile ID.
    /// </summary>
    public sealed class BARunnerStatisticsService
    {
        private readonly string _directoryPath;

        public BARunnerStatisticsService(string directoryPath = null)
        {
            _directoryPath = string.IsNullOrWhiteSpace(directoryPath)
                ? Path.Combine(Application.persistentDataPath, "Bunker26", "BARunner", "Profiles")
                : directoryPath;
        }

        /// <summary>Returns the resolved file path. Useful for diagnostics and support logs.</summary>
        public string GetFilePathForDiagnostics(string profileId)
        {
            return GetFilePath(profileId);
        }

        /// <summary>Loads the BA Runner statistics for a profile, or returns a new empty record.</summary>
        public BARunnerStatistics Load(string profileId)
        {
            string filePath = GetFilePath(profileId);
            if (!File.Exists(filePath))
                return new BARunnerStatistics();

            try
            {
                string json = File.ReadAllText(filePath);
                BARunnerStatistics statistics = JsonUtility.FromJson<BARunnerStatistics>(json);
                if (statistics == null)
                    throw new InvalidDataException("The statistics JSON did not contain a valid object.");

                statistics.EnsureValidData();
                return statistics;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Could not load BA Runner statistics for profile '" + profileId +
                    "'. The existing file was left untouched. " + exception.Message,
                    exception);
            }
        }

        /// <summary>Records a run and persists it. Existing data is not overwritten if loading fails.</summary>
        public bool TryRecordRun(
            string profileId,
            SurvivalRunResult result,
            out BARunnerStatistics updatedStatistics,
            out string error)
        {
            bool ignoredNewRecord;
            return TryRecordRun(profileId, result, out updatedStatistics, out ignoredNewRecord, out error);
        }

        /// <summary>
        /// Records a run and reports whether its score is a new personal best.
        /// </summary>
        public bool TryRecordRun(
            string profileId,
            SurvivalRunResult result,
            out BARunnerStatistics updatedStatistics,
            out bool isNewScoreRecord,
            out string error)
        {
            updatedStatistics = null;
            isNewScoreRecord = false;
            error = null;

            if (result == null)
            {
                error = "The Survival run result is null.";
                return false;
            }

            try
            {
                BARunnerStatistics statistics = Load(profileId);
                isNewScoreRecord = statistics.RecordSurvivalRun(result);
                Save(profileId, statistics);
                updatedStatistics = statistics;
                return true;
            }
            catch (Exception exception)
            {
                error = "Could not save BA Runner statistics: " + exception.Message;
                return false;
            }
        }

        private void Save(string profileId, BARunnerStatistics statistics)
        {
            if (statistics == null)
                throw new ArgumentNullException(nameof(statistics));

            statistics.EnsureValidData();
            string filePath = GetFilePath(profileId);
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            string temporaryPath = filePath + ".tmp";
            string backupPath = filePath + ".bak";
            File.WriteAllText(temporaryPath, JsonUtility.ToJson(statistics, true));

            if (!File.Exists(filePath))
            {
                File.Move(temporaryPath, filePath);
                return;
            }

            try
            {
                File.Replace(temporaryPath, filePath, backupPath, true);
                if (File.Exists(backupPath))
                    File.Delete(backupPath);
                return;
            }
            catch (PlatformNotSupportedException)
            {
                // Fall through to the backup-based replacement below.
            }
            catch (IOException)
            {
                // Some Unity target platforms do not support File.Replace.
                // The backup-based replacement below preserves the old file.
            }
            catch (UnauthorizedAccessException)
            {
                // Fall through to the backup-based replacement. If access is still denied,
                // the caller receives the error and the old file remains intact.
            }

            File.Copy(filePath, backupPath, true);
            try
            {
                File.Delete(filePath);
                File.Move(temporaryPath, filePath);
                if (File.Exists(backupPath))
                    File.Delete(backupPath);
            }
            catch
            {
                if (!File.Exists(filePath) && File.Exists(backupPath))
                    File.Copy(backupPath, filePath, true);
                throw;
            }
        }

        private string GetFilePath(string profileId)
        {
            if (string.IsNullOrWhiteSpace(profileId))
                throw new ArgumentException("A non-empty profile ID is required.", nameof(profileId));

            // Party profile IDs are generated as GUIDs. Restrict filenames to safe characters
            // so a malformed profile ID cannot escape the statistics directory.
            for (int i = 0; i < profileId.Length; i++)
            {
                char character = profileId[i];
                bool isSafe =
                    (character >= 'a' && character <= 'z') ||
                    (character >= 'A' && character <= 'Z') ||
                    (character >= '0' && character <= '9') ||
                    character == '-' || character == '_';

                if (!isSafe)
                    throw new ArgumentException("The profile ID contains characters that are not valid in a statistics filename.", nameof(profileId));
            }

            return Path.Combine(_directoryPath, profileId + ".json");
        }
    }
}
