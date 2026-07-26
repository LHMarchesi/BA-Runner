using UnityEngine;

public class ChangingLaneBehavior : MonoBehaviour, IObstacleBehavior
{
    private ChangingLaneObstacleConfig config;

    private bool isChangingLane;
    private bool hasChangedLane;

    private float distanceTravelled;
    private float targetY;

    public void OnSpawned(ObstacleConfig obstacleConfig)
    {
        config = obstacleConfig as ChangingLaneObstacleConfig;

        distanceTravelled = 0f;
        isChangingLane = false;
        hasChangedLane = false;
    }

    public void Tick(float worldSpeed)
    {
        if (config == null)
            return;

        Vector3 pos = transform.localPosition;

        // Distancia recorrida durante este frame.
        float frameDistance = Mathf.Abs(worldSpeed) * Time.deltaTime;

        // Movimiento normal del obstáculo.
        pos.x -= frameDistance;

        if (!isChangingLane && !hasChangedLane)
        {
            distanceTravelled += frameDistance;

            if (distanceTravelled >= config.changeDistance)
            {
                float laneOffset =
                    config.changeDirection ==
                    ChangingLaneObstacleConfig.LaneChangeDirection.Up
                        ? 100f
                        : -100f;

                targetY = pos.y + laneOffset;

                isChangingLane = true;
                hasChangedLane = true;
            }
        }

        // Movimiento únicamente en Y durante el cambio de carril.
        if (isChangingLane)
        {
            pos.y = Mathf.MoveTowards(
                pos.y,
                targetY,
                config.laneChangeSpeed * Time.deltaTime
            );

            if (Mathf.Approximately(pos.y, targetY))
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