using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public LevelStage CurrentStage => currentStage;
    [SerializeField] private ProgessBar progessBar;
    [SerializeField] private Transform signSpawnTransfomr;
    [SerializeField] private ObstacleConfig singConfig;
    [SerializeField] private LevelStage currentStage;
    [SerializeField] WorldSpeed WorldSpeed;
    private float levelProgession;
    private float completionTime;
    private bool levelCompleted;
    EventBinding<OnLevelUpdateEvent> OnLevelUpdateEvent;
    EventBinding<OnLevelStartEvent> OnLevelStartEvent;
    private int currentStageIndex;

    Level_Scriptable currentLevel => ProgressionManager.Instance.CurrentLevel;

    private void OnEnable()
    {
        OnLevelStartEvent = new EventBinding<OnLevelStartEvent>(OnLevelStart);
        EventBus<OnLevelStartEvent>.Register(OnLevelStartEvent);


        OnLevelUpdateEvent = new EventBinding<OnLevelUpdateEvent>(OnLevelUpdate);
        EventBus<OnLevelUpdateEvent>.Register(OnLevelUpdateEvent);
    }

    private void OnDisable()
    {
        EventBus<OnLevelStartEvent>.Deregister(OnLevelStartEvent);
        EventBus<OnLevelUpdateEvent>.Deregister(OnLevelUpdateEvent);
    }

    private void OnLevelUpdate(OnLevelUpdateEvent e)
    {
        IncreaseLevelProgession();
        CheckStageProgress();
        e.levelProgession = levelProgession;
    }

    private void OnLevelStart(OnLevelStartEvent e)
    {
        levelProgession = 0;
        completionTime = 0;
        currentStageIndex = 0;
        levelCompleted = false;
        currentStage = currentLevel.stages[0];
        WorldSpeed.SetSpeedData(currentStage.speedData);

        EventBus<OnRoadEnvironmentChanged>.Raise(
          new OnRoadEnvironmentChanged
          {
              environmentPreset = currentStage.environment
          });
        EventBus<OnRoadStageChanged>.Raise(
    new OnRoadStageChanged
    {
        stage = currentStage
    });


        ShowSpeedSign(currentStage);
    }

    private void CheckStageProgress()
    {
        var stages = currentLevel.stages;

        if (currentStageIndex >= stages.Count - 1)
            return;

        float normalized =
            levelProgession / currentLevel.maxLevelProgession;

        LevelStage nextStage =
            stages[currentStageIndex + 1];

        if (normalized >= nextStage.progressionRequired)
        {
            EventBus<OnRoadEnvironmentChanged>.Raise(
          new OnRoadEnvironmentChanged
          {
              environmentPreset = nextStage.environment
          });
            currentStageIndex++;
            EventBus<OnRoadStageChanged>.Raise(
                new OnRoadStageChanged
                {
                    stage = nextStage
                });

            currentStage = nextStage;
            WorldSpeed.SetSpeedData(nextStage.speedData);
            ShowSpeedSign(nextStage);
        }
    }

    private void ShowSpeedSign(LevelStage nextStage)
    {
        Obstacle sign = Instantiate(singConfig.prefab.gameObject, signSpawnTransfomr).GetComponent<Obstacle>();
        TextMeshProUGUI text = sign.GetComponentInChildren<TextMeshProUGUI>();
        sign.Initialize(WorldSpeed, singConfig);
        text.text = nextStage.displayedSpeed.ToString();
    }

    private void IncreaseLevelProgession()
    {
        if (levelCompleted) return;
        var currentLevel = this.currentLevel;
        if (currentLevel == null) return;
        completionTime += Time.deltaTime;

        if (levelProgession < currentLevel.maxLevelProgession)
        {
            var speedData = currentStage.speedData;
            levelProgession += WorldSpeed.DistanceThisFrame;

            if (speedData != null)
            {
                float normalized = levelProgession / currentLevel.maxLevelProgession;

                WorldSpeed.SetProgression(normalized);
            }

            if (progessBar != null)
                progessBar.UpdateProgess(levelProgession, currentLevel.maxLevelProgession);
        }
        else
        {
            levelCompleted = true;
            bool isNewRecord = ProgressionManager.Instance.RegisterLevelCompletionTime(currentLevel, completionTime);
            Debug.Log(
        isNewRecord
            ? "¡Nuevo mejor tiempo!"
            : "Nivel completado sin nuevo récord."
    );
            EventBus<OnLevelCompletedEvent>.Raise(new OnLevelCompletedEvent { stars = CalculateStars(completionTime), completionTime = this.completionTime });
        }
    }



    private int CalculateStars(float time)
    {
        foreach (var requirement in currentLevel.starRequirements)
        {
            if (time <= requirement.maxTime)
                return requirement.stars;
        }

        return 1;
    }
}