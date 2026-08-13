using System;
using UnityEngine;

public class SurvivalTrackController : MonoBehaviour
{
    [Header("Road")]
    [SerializeField] private RoadConfig roadConfig;

    [Header("Systems")]
    [SerializeField] private WorldSpeed worldSpeed;
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private ScoreSystem scoreSystem;

    [Header("Fallback")]
    [Tooltip(
        "Se usa si no se puede calcular la distancia " +
        "entre dos RoadSections."
    )]
    [SerializeField] private float fallbackStageDistance = 100f;


    public int TotalStagesCompleted
    {
        get;
        private set;
    }

    public int CurrentSectionIndex
    {
        get;
        private set;
    }

    public RoadSection CurrentSection
    {
        get;
        private set;
    }

    public float CurrentStageProgress
    {
        get;
        private set;
    }

    public float CurrentStageTargetDistance
    {
        get;
        private set;
    }


    public event Action<SurvivalTrackController>
        StageCompleted;


    private bool running;
    private float debugTimer;


    // =========================================================
    // START
    // =========================================================

    public void StartRun()
    {
        if (
            roadConfig == null ||
            roadConfig.sections == null ||
            roadConfig.sections.Count == 0
        )
        {
            Debug.LogError(
                $"[{gameObject.name}] RoadConfig inválido."
            );

            return;
        }

        TotalStagesCompleted = 0;
        CurrentSectionIndex = 0;
        CurrentStageProgress = 0f;

        worldSpeed.ResetSurvivalModifiers();

        if (scoreSystem != null)
        {
            scoreSystem.ResetScore();
        }

        running = true;

        ApplyCurrentSection();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!running)
            return;

        if (
            worldSpeed == null ||
            worldSpeed.IsFrozen
        )
        {
            return;
        }

        float boostImpact =
            Mathf.Pow(
                worldSpeed.PlayerBoostMultiplier,
                1.5f
            );

        float delta =
            Time.deltaTime *
            boostImpact *
            worldSpeed.CurrentWorldSpeed;

        if (delta <= 0f)
            return;

        CurrentStageProgress +=
            delta;

        if (scoreSystem != null)
        {
            scoreSystem.AddDistance(
                delta
            );
        }

        if (
            CurrentStageProgress >=
            CurrentStageTargetDistance
        )
        {
            CompleteStage();
        }
    }

    // =========================================================
    // COMPLETE STAGE
    // =========================================================
    public float DistanceToNextStage
    {
        get
        {
            return Mathf.Max(
                0f,
                CurrentStageTargetDistance -
                CurrentStageProgress
            );
        }
    }
    private void CompleteStage()
    {
        /*
         * Conservamos la distancia sobrante.
         */
        CurrentStageProgress -=
            CurrentStageTargetDistance;

        TotalStagesCompleted++;

        if (scoreSystem != null)
        {
            scoreSystem.AddStage();
        }

        Debug.Log(
            $"[{gameObject.name}] " +
            $"STAGE COMPLETADO → " +
            $"{TotalStagesCompleted}"
        );

        /*
         * COOP:
         * suma 1 al revive del compañero muerto.
         *
         * VERSUS:
         * después aplicaremos el debuff.
         */
        StageCompleted?.Invoke(this);

        AdvanceSection();
    }


    // =========================================================
    // SECTION
    // =========================================================

    private void AdvanceSection()
    {
        CurrentSectionIndex++;

        if (
            CurrentSectionIndex >=
            roadConfig.sections.Count
        )
        {
            CurrentSectionIndex = 0;
        }

        ApplyCurrentSection();
    }


    private void ApplyCurrentSection()
    {
        CurrentSection =
            roadConfig.sections[
                CurrentSectionIndex
            ];

        CurrentStageTargetDistance =
            GetCurrentSectionDistance();

        if (
            CurrentStageTargetDistance <= 0f
        )
        {
            CurrentStageTargetDistance =
                fallbackStageDistance;
        }


        // ==========================================
        // SPEED
        // ==========================================

        if (
            worldSpeed != null &&
            CurrentSection.worldSpeedData != null
        )
        {
            worldSpeed.SetSpeedData(
                CurrentSection.worldSpeedData
            );
        }


        // ==========================================
        // WAVE
        // ==========================================

        ApplyWave();


        Debug.Log(
            $"[{gameObject.name}] " +
            $"SECTION: {CurrentSection.stageName} | " +
            $"Distancia stage: " +
            $"{CurrentStageTargetDistance}"
        );
    }


    // =========================================================
    // GET STAGE DISTANCE
    // =========================================================

    private float GetCurrentSectionDistance()
    {
        var sections =
            roadConfig.sections;

        if (sections.Count == 1)
        {
            return fallbackStageDistance;
        }

        /*
         * En RoadManager:
         *
         * section 0 comienza en distancia 0.
         *
         * Las demás comienzan en su
         * progressionRequired.
         */

        float currentStart;

        if (CurrentSectionIndex == 0)
        {
            currentStart = 0f;
        }
        else
        {
            currentStart =
                sections[
                    CurrentSectionIndex
                ].progressionRequired;
        }


        // ==========================================
        // NO ES LA ÚLTIMA
        // ==========================================

        if (
            CurrentSectionIndex <
            sections.Count - 1
        )
        {
            float nextStart =
                sections[
                    CurrentSectionIndex + 1
                ].progressionRequired;

            float distance =
                nextStart -
                currentStart;

            if (distance > 0f)
                return distance;
        }


        // ==========================================
        // ÚLTIMA SECTION
        // ==========================================

        /*
         * RoadConfig no tiene un endpoint después
         * de la última section.
         *
         * Para Survival usamos la longitud de la
         * section anterior.
         */

        if (CurrentSectionIndex > 0)
        {
            float previousStart;

            if (CurrentSectionIndex - 1 == 0)
            {
                previousStart = 0f;
            }
            else
            {
                previousStart =
                    sections[
                        CurrentSectionIndex - 1
                    ].progressionRequired;
            }

            float previousDistance =
                currentStart -
                previousStart;

            if (previousDistance > 0f)
                return previousDistance;
        }

        return fallbackStageDistance;
    }


    // =========================================================
    // WAVES
    // =========================================================

    private void ApplyWave()
    {
        if (spawnManager == null)
            return;

        if (
            CurrentSection.waveConfig == null ||
            CurrentSection.waveConfig.Length == 0
        )
        {
            return;
        }

        int index =
            UnityEngine.Random.Range(
                0,
                CurrentSection.waveConfig.Length
            );

        WaveConfig wave =
            CurrentSection.waveConfig[index];

        if (wave != null)
        {
            spawnManager.SetWaveConfig(
                wave
            );
        }
    }


    // =========================================================
    // RUNNING
    // =========================================================

    public void SetRunning(
        bool value
    )
    {
        running = value;

        if (worldSpeed != null)
        {
            worldSpeed.SetFrozen(
                !value
            );
        }

        if (scoreSystem != null)
        {
            scoreSystem.SetRunnig(
                value
            );
        }
    }
}