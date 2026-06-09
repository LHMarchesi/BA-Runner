public interface IObstacleBehavior
{
   
    void OnSpawned();

    void Tick(float worldSpeed);

    bool ShouldDespawn(float despawnYThreshold);
}
