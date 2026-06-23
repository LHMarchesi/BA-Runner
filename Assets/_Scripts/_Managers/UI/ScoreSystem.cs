using System;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    [Header("Score")]
    public int Score { get; private set; }
    public float Distance { get; private set; }
    public float Stage { get; private set; }

    private bool isRunning;

    private EventBinding<OnRoadSectionChanged> sectionBinding;

    EventBinding<OnPlayerDeathEvent> playerDeathBinding;

    private void OnEnable()
    {
        sectionBinding = new EventBinding<OnRoadSectionChanged>(OnRoadSectionChanged);
        EventBus<OnRoadSectionChanged>.Register(sectionBinding);

        playerDeathBinding = new EventBinding<OnPlayerDeathEvent>(HandlePLayerDeath);
        EventBus<OnPlayerDeathEvent>.Register(playerDeathBinding);

        isRunning = true;
    }

    private void HandlePLayerDeath(OnPlayerDeathEvent e)
    {
        isRunning = false;
    }

    private void OnDisable()
    {
        EventBus<OnRoadSectionChanged>.Deregister(sectionBinding);
        EventBus<OnPlayerDeathEvent>.Deregister(playerDeathBinding);
    }



    private void Update()
    {
        EventBus<OnLevelUpdateEvent>.Raise(
      new OnLevelUpdateEvent
      {
          score = Score,
          distance = Distance,
          loops = Stage
      }
  );
    }

    public void SetRunnig(bool value)
    {
        isRunning = value;
    }

    private void OnRoadSectionChanged(OnRoadSectionChanged e)
    {
        Stage++;
    }


    public void AddDistance(float value)
    {
        if (!isRunning)
            return;
        Distance += value;
        Score = Mathf.FloorToInt(Distance * 10f);
    }

}