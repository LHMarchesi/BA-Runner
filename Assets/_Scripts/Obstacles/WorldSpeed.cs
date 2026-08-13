using UnityEngine;

public class WorldSpeed : MonoBehaviour
{
    public float PlayerBoostMultiplier
    {
        get;
        set;
    } = 1f;

    public float ProgressionMultiplier
    {
        get;
        private set;
    } = 1f;

    public float SurvivalMultiplier
    {
        get;
        private set;
    } = 1f;

    public bool IsFrozen
    {
        get;
        private set;
    }

    private SpeedData currentSpeedData;


    // =========================================================
    // CURRENT SPEED
    // =========================================================

    public float CurrentWorldSpeed
    {
        get
        {
            if (
                currentSpeedData == null ||
                IsFrozen
            )
            {
                return 0f;
            }

            return
                currentSpeedData.baseWorldSpeed *
                ProgressionMultiplier *
                PlayerBoostMultiplier *
                SurvivalMultiplier;
        }
    }


    // =========================================================
    // SPEED DATA
    // =========================================================

    public void SetSpeedData(
        SpeedData speedData
    )
    {
        if (speedData == null)
            return;

        /*
         * IMPORTANTE:
         *
         * NO tocamos ProgressionMultiplier.
         *
         * Exactamente como en tu WorldSpeed original.
         */
        currentSpeedData =
            speedData;

        Debug.Log(
            $"[{gameObject.name}] " +
            $"Nueva Base Speed: " +
            $"{currentSpeedData.baseWorldSpeed}"
        );
    }


    // =========================================================
    // NORMAL LEVEL PROGRESSION
    // =========================================================

    public void SetProgression(
        float normalized
    )
    {
        if (currentSpeedData == null)
            return;

        ProgressionMultiplier =
            Mathf.Lerp(
                currentSpeedData
                    .minProgressionMultiplier,

                currentSpeedData
                    .maxProgressionMultiplier,

                Mathf.Clamp01(normalized)
            );
    }


    // =========================================================
    // SURVIVAL / VERSUS
    // =========================================================

    public void AddSurvivalSpeed(
        float amount
    )
    {
        SurvivalMultiplier =
            Mathf.Clamp(
                SurvivalMultiplier + amount,
                1f,
                2f
            );
    }


    public void SetFrozen(
        bool frozen
    )
    {
        IsFrozen = frozen;
    }


    public void ResetSurvivalModifiers()
    {
        PlayerBoostMultiplier = 1f;

        /*
         * En Survival los SpeedData ya determinan
         * la velocidad de cada stage.
         */
        ProgressionMultiplier = 1f;

        SurvivalMultiplier = 1f;

        IsFrozen = false;
    }


    public float DistanceThisFrame =>
        CurrentWorldSpeed *
        Time.deltaTime;
}