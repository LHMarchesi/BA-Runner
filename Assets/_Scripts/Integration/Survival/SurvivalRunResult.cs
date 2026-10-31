using UnityEngine;

namespace BARunner.Survival
{
    /// <summary>
    /// Snapshot of one completed BA Runner Survival run.
    /// This model belongs to BA Runner, not to the reusable Party package.
    /// </summary>
    public sealed class SurvivalRunResult
    {
        public int Score { get; }
        public float Distance { get; }
        public int StagesCompleted { get; }
        public float SurvivalTimeSeconds { get; }

        public SurvivalRunResult(
            int score,
            float distance,
            int stagesCompleted,
            float survivalTimeSeconds)
        {
            Score = Mathf.Max(0, score);
            Distance = IsFinite(distance) ? Mathf.Max(0f, distance) : 0f;
            StagesCompleted = Mathf.Max(0, stagesCompleted);
            SurvivalTimeSeconds = IsFinite(survivalTimeSeconds)
                ? Mathf.Max(0f, survivalTimeSeconds)
                : 0f;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
