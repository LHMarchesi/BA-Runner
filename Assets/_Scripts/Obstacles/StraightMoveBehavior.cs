using UnityEngine;

public class StraightMoveBehavior : MonoBehaviour, IObstacleBehavior
{
    public void OnSpawned(ObstacleConfig config) { }

    public void Tick(float worldSpeed)
    {
        transform.localPosition += Vector3.left * worldSpeed * Time.deltaTime;
    }

    public bool ShouldDespawn(float despawnXThreshold)
    {
        return transform.localPosition.x < despawnXThreshold;
    }
}
