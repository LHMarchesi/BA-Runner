using UnityEngine;

public class ChangingLaneBehavior : MonoBehaviour, IObstacleBehavior
{
    private ChangingLaneObstacleConfig config;

    private Transform[] availableLanes;

    private int currentLaneIndex = -1;
    private int targetLaneIndex = -1;

    private bool isChangingLane;
    private bool hasChangedLane;

    private float distanceTravelled;
    private float targetWorldY;
    private bool lanesConfigured;

    public void OnSpawned(
     ObstacleConfig obstacleConfig)
    {
        config =
            obstacleConfig as ChangingLaneObstacleConfig;

        distanceTravelled = 0f;

        isChangingLane = false;
        hasChangedLane = false;

        targetLaneIndex = -1;
        targetWorldY = 0f;

        /*
         * Los carriles serán configurados por SpawnManager
         * después de activar el objeto.
         */
        availableLanes = null;
        currentLaneIndex = -1;
        lanesConfigured = false;
    }

    /// <summary>
    /// SpawnManager llama a este método después de colocar
    /// el obstáculo dentro del Road_Holder correspondiente.
    /// </summary>
    public void ConfigureLanes(
      Transform[] lanes,
      int spawnLaneIndex)
    {
        if (lanes == null ||
            lanes.Length == 0)
        {

            lanesConfigured = false;
            return;
        }

        if (spawnLaneIndex < 0 ||
            spawnLaneIndex >= lanes.Length)
        {
            lanesConfigured = false;
            return;
        }

        availableLanes = lanes;
        currentLaneIndex = spawnLaneIndex;
        lanesConfigured = true;
    }

    public void Tick(float worldSpeed)
    {
        if (config == null)
            return;

        float frameDistance =
            Mathf.Abs(worldSpeed) * Time.deltaTime;

        MoveHorizontally(frameDistance);

        if (!isChangingLane && !hasChangedLane)
        {
            distanceTravelled += frameDistance;

            if (distanceTravelled >= config.changeDistance)
            {
                BeginLaneChange();
            }
        }

        if (isChangingLane)
        {
            UpdateLaneChange();
        }
    }

    private void MoveHorizontally(float frameDistance)
    {
        Vector3 localPosition =
            transform.localPosition;

        localPosition.x -= frameDistance;

        transform.localPosition = localPosition;
    }

    private void BeginLaneChange()
    {
        hasChangedLane = true;

        targetLaneIndex = FindTargetLaneIndex();

        Transform targetLane =
            availableLanes[targetLaneIndex];

        Vector3 worldPosition =
            transform.position;

        targetWorldY =
            targetLane.position.y;

        transform.SetParent(
            targetLane,
            worldPositionStays: true
        );

        transform.position = worldPosition;

        transform.SetAsLastSibling();

        isChangingLane = true;
    }

    private void UpdateLaneChange()
    {
        Vector3 worldPosition =
            transform.position;

        worldPosition.y = Mathf.MoveTowards(
            worldPosition.y,
            targetWorldY,
            config.laneChangeSpeed * Time.deltaTime
        );

        transform.position = worldPosition;

        if (Mathf.Abs(
                transform.position.y -
                targetWorldY) <= 0.01f)
        {
            FinishLaneChange();
        }
    }

    private void FinishLaneChange()
    {
        Vector3 worldPosition =
            transform.position;

        worldPosition.y = targetWorldY;

        transform.position = worldPosition;

        /*
         * Como ahora es hijo del Road_Holder destino,
         * su posición vertical local debería ser cero.
         *
         */
        Vector3 localPosition =
            transform.localPosition;

        localPosition.y = 0f;

        transform.localPosition = localPosition;

        currentLaneIndex = targetLaneIndex;
        targetLaneIndex = -1;

        isChangingLane = false;
    }

    /// <summary>
    /// Busca el carril más cercano que esté realmente
    /// arriba o abajo del carril actual.
    ///
    /// De esta manera no importa el orden del array.
    /// </summary>
    private int FindTargetLaneIndex()
    {
        Transform currentLane =
            availableLanes[currentLaneIndex];

        if (currentLane == null)
            return -1;

        float currentY =
            currentLane.position.y;

        int bestIndex = -1;

        float closestDistance =
            Mathf.Infinity;

        for (int i = 0; i < availableLanes.Length; i++)
        {
            if (i == currentLaneIndex)
                continue;

            Transform candidateLane =
                availableLanes[i];

            if (candidateLane == null)
                continue;

            float difference =
                candidateLane.position.y - currentY;

            bool isValidDirection =
                config.changeDirection ==
                ChangingLaneObstacleConfig
                    .LaneChangeDirection.Up
                    ? difference > 0f
                    : difference < 0f;

            if (!isValidDirection)
                continue;

            float distance =
                Mathf.Abs(difference);

            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            bestIndex = i;
        }

        return bestIndex;
    }

    public bool ShouldDespawn(
        float despawnXThreshold)
    {
        return transform.localPosition.x <
               despawnXThreshold;
    }
}