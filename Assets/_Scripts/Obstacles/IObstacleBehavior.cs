public interface IObstacleBehavior
{
   
    void OnSpawned(ObstacleConfig config);

    void Tick(float worldSpeed);

    bool ShouldDespawn(float despawnYThreshold);
}
