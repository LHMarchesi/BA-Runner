using UnityEngine;

public class RoadManager : MonoBehaviour
{
    [SerializeField] private RoadConfig roadConfig;
    [SerializeField] private WorldSpeed worldSpeed;
    [SerializeField] private ScoreSystem scoreSystem;

    private float progression;
    private int currentSectionIndex;
    private int currentLoop;

    private RoadSection currentSection;

    public float Progression => progression;
    public int CurrentLoop => currentLoop;
    public RoadSection CurrentSection => currentSection;

    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        UpdateProgression();
        CheckSectionProgression();
    }

    void Initialize()
    {
        progression = 0;
        currentLoop = 0;
        currentSectionIndex = 0;

        EnterSection(
            roadConfig.sections[0]);
    }

    void UpdateProgression()
    {
        float boostImpact =
            Mathf.Pow(
                worldSpeed.PlayerBoostMultiplier,
                1.5f);

        progression +=
            Time.deltaTime *
            boostImpact *
            worldSpeed.CurrentWorldSpeed;

        float delta =
        Time.deltaTime *
        boostImpact *
        worldSpeed.CurrentWorldSpeed;

        scoreSystem.AddDistance(delta);
    }

    void CheckSectionProgression()
    {
        if (currentSectionIndex >=
            roadConfig.sections.Count - 1)
            return;

        var nextSection =
            roadConfig.sections[
                currentSectionIndex + 1];

        if (progression >=
            nextSection.progressionRequired)
        {
            currentSectionIndex++;
            EnterSection(nextSection);
        }
    }

    void EnterSection(RoadSection newSection)
    {
        currentSection = newSection;

        worldSpeed.SetSpeedData(newSection.worldSpeedData);

        EventBus<OnRoadSectionChanged>.Raise(
            new OnRoadSectionChanged
            {
                roadSection = newSection
            });

        if (newSection.waveConfig != null && newSection.waveConfig.Length > 0)
        {
            EventBus<OnWaveConfigChanged>.Raise(
                new OnWaveConfigChanged
                {
                    waveConfigs = newSection.waveConfig
                });
        }
        EventBus<OnRoadEnvironmentChanged>.Raise(
      new OnRoadEnvironmentChanged
      {
          environmentPreset = newSection.environment
      });
    }
}
