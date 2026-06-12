using UnityEngine;

public class SportCarBehavior : MonoBehaviour, IObstacleBehavior
{
    private enum State
    {
        Warning,
        Charging
    }

    private State state;
    private SportCarObstacleConfig config;
    [SerializeField]private GameObject indicator;

    private float timer;

    public void OnSpawned(ObstacleConfig obstacleConfig)
    {
        config = obstacleConfig as SportCarObstacleConfig;
        timer = 0f;
        state = State.Warning;


        indicator.SetActive(true);
    }

    public void Tick(float worldSpeed)
    {
        switch (state)
        {
            case State.Warning:

                timer += Time.deltaTime;

                if (timer >= config.warningDuration)
                {
                    indicator.SetActive(false);
                    state = State.Charging;
                }

                break;

            case State.Charging:

                transform.localPosition +=
                    Vector3.right *
                    config.chargeSpeed *
                    Time.deltaTime;

                break;
        }
    }

    public bool ShouldDespawn(float despawnXThreshold)
    {
        return transform.localPosition.x > despawnXThreshold;
    }
}