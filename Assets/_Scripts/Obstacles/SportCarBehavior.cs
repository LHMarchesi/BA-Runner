using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private RawImage indicatorImage;
    [SerializeField] private float indicatorScrollSpeed = 0.5f;

    private float timer;
    private void Awake()
    {
        
    }

    public void OnSpawned(ObstacleConfig obstacleConfig)
    {
        config = obstacleConfig as SportCarObstacleConfig;
        timer = 0f;
        state = State.Warning;

        indicator.SetActive(true);

        Rect uv = indicatorImage.uvRect;
        uv.x = 0;
        indicatorImage.uvRect = uv;
    }

    public void Tick(float worldSpeed)
    {
        switch (state)
        {
            case State.Warning:
                ScrollIndicator();
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
    private void ScrollIndicator()
    {
        Rect uv = indicatorImage.uvRect;
        uv.x -= indicatorScrollSpeed * Time.deltaTime;
        indicatorImage.uvRect = uv;
    }
    public bool ShouldDespawn(float despawnXThreshold)
    {
        return transform.localPosition.x > despawnXThreshold;
    }
}