using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private ProgessBar progessBar;

    private float levelProgession;
    private float completionTime;
    private bool levelCompleted;
   
    EventBinding<OnLevelUpdateEvent> OnLevelUpdateEvent;
    EventBinding<OnLevelStartEvent> OnLevelStartEvent;
    Level_Scriptable currentLevel => ProgressionManager.Instance.CurrentLevel;

    private void OnEnable()
    {
        OnLevelStartEvent = new EventBinding<OnLevelStartEvent>(OnLevelStart);
        EventBus<OnLevelStartEvent>.Register(OnLevelStartEvent);
        

        OnLevelUpdateEvent = new EventBinding<OnLevelUpdateEvent>(OnLevelUpdate);
        EventBus<OnLevelUpdateEvent>.Register(OnLevelUpdateEvent);
    }

    private void OnLevelUpdate(OnLevelUpdateEvent e)
    {
        IncreaseLevelProgession();
        e.levelProgession = levelProgession; 
    }
    private void OnLevelStart(OnLevelStartEvent e)
    {
        levelProgession = 0;
        completionTime = 0;
        levelCompleted = false;
    }

    private void IncreaseLevelProgession()
    {
        if (levelCompleted) return;
        var currentLevel = this.currentLevel;
        if (currentLevel == null) return;
        completionTime += Time.deltaTime;

        if (levelProgession < currentLevel.maxLevelProgession)
        {
            var speedData = currentLevel.speedData;
            float boostImpact = Mathf.Pow(speedData.playerBoostMultiplier, 1.5f);
            levelProgession += Time.deltaTime * boostImpact;

            if (speedData != null)
            {
                float normalized = levelProgession / currentLevel.maxLevelProgession;

                speedData.currentProgressionMultiplier =
                    Mathf.Lerp(speedData.minProgressionMultiplier,
                               speedData.maxProgressionMultiplier,
                               normalized);
            }

            if (progessBar != null)
                progessBar.UpdateProgess(levelProgession, currentLevel.maxLevelProgession);
        }
        else
        {
            levelCompleted = true;
            EventBus<OnLevelCompletedEvent>.Raise(new OnLevelCompletedEvent{stars = CalculateStars(completionTime), completionTime = this.completionTime});
        }
    }

    int CalculateStars(float time)
    {
        if (time <= 30) return 5;
        if (time <= 45) return 4;
        if (time <= 60) return 3;
        if (time <= 90) return 2;

        return 1;
    }
}