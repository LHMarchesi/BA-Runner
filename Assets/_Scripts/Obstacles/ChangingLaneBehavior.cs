using UnityEngine;

public class ChangingLaneBehavior : MonoBehaviour, IObstacleBehavior
{
    private ChangingLaneObstacleConfig config;

    private bool isChangingLane;
    private bool hasChangedLane;
    private float timer;
    private Vector3 targetPosition;
    private float targetY;
    public void OnSpawned(ObstacleConfig config)
    {
        this.config = config as ChangingLaneObstacleConfig;
        timer = 0f;
        isChangingLane = false;
        hasChangedLane = false;
    }

    public void Tick(float worldSpeed)
    {
        if (config == null)
            return;

        Vector3 pos = transform.localPosition;
        pos.x -= worldSpeed * Time.deltaTime;

        if (!isChangingLane && !hasChangedLane)
        {
            timer += Time.deltaTime;

            if (timer >= config.changeDelay)
            {
                float laneOffset =
                    config.changeDirection == ChangingLaneObstacleConfig.LaneChangeDirection.Up
                        ? 100f
                        : -100f;

                targetY = pos.y + laneOffset;
                isChangingLane = true;
                hasChangedLane = true;
            }
        }

        // Mover sólo en Y
        if (isChangingLane)
        {
            pos.y = Mathf.MoveTowards(
                pos.y,
                targetY,
                config.laneChangeSpeed * Time.deltaTime
            );

            if (Mathf.Abs(pos.y - targetY) < 0.01f)
            {
                pos.y = targetY;
                isChangingLane = false;
            }
        }

        transform.localPosition = pos;
    }

    public bool ShouldDespawn(float despawnXThreshold)
    {
        return transform.localPosition.x < despawnXThreshold;
    }
}   