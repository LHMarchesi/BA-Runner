using System;
using UnityEngine;

namespace BARunner.Survival
{
    /// <summary>
    /// BA Runner-specific statistics for one Party profile.
    /// Stored separately so the Bunker26.Party package remains game-independent.
    /// </summary>
    [Serializable]
    public sealed class BARunnerStatistics
    {
        public int schemaVersion = 1;
        public int survivalRunsPlayed;
        public int bestSurvivalScore;
        public float bestSurvivalDistance;
        public int bestSurvivalStages;
        public float totalSurvivalTimeSeconds;

        /// <summary>
        /// Records one run and returns true when it establishes a new best score.
        /// The first completed run establishes the initial record.
        /// </summary>
        public bool RecordSurvivalRun(SurvivalRunResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            EnsureValidData();

            bool isNewScoreRecord = survivalRunsPlayed == 0 || result.Score > bestSurvivalScore;

            bestSurvivalScore = Mathf.Max(bestSurvivalScore, result.Score);
            bestSurvivalDistance = Mathf.Max(bestSurvivalDistance, result.Distance);
            bestSurvivalStages = Mathf.Max(bestSurvivalStages, result.StagesCompleted);

            survivalRunsPlayed++;
            totalSurvivalTimeSeconds += result.SurvivalTimeSeconds;

            return isNewScoreRecord;
        }

        public void EnsureValidData()
        {
            if (schemaVersion <= 0)
                schemaVersion = 1;

            survivalRunsPlayed = Mathf.Max(0, survivalRunsPlayed);
            bestSurvivalScore = Mathf.Max(0, bestSurvivalScore);
            bestSurvivalStages = Mathf.Max(0, bestSurvivalStages);

            if (!IsFinite(bestSurvivalDistance) || bestSurvivalDistance < 0f)
                bestSurvivalDistance = 0f;

            if (!IsFinite(totalSurvivalTimeSeconds) || totalSurvivalTimeSeconds < 0f)
                totalSurvivalTimeSeconds = 0f;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
