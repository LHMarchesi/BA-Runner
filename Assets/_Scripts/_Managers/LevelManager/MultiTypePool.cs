using System.Collections.Generic;
using UnityEngine;

public class MultiTypePool : MonoBehaviour
{

    private readonly Dictionary<ObstacleConfig, Queue<Obstacle>> pools = new();

    public void Prewarm(ObstacleConfig config, WorldSpeed worldSpeed)
    {
        EnsureQueueExists(config, out var queue);

        int toCreate = config.defaultPoolSize - queue.Count;
        for (int i = 0; i < toCreate; i++)
            queue.Enqueue(CreateNew(config, worldSpeed));
    }

    public Obstacle Get(ObstacleConfig config, WorldSpeed worldSpeed)
    {
        if (pools.TryGetValue(config, out var queue) && queue.Count > 0)
            return queue.Dequeue();

        return CreateNew(config, worldSpeed);
    }

    public void Return(ObstacleConfig config, Obstacle obstacle)
    {
        obstacle.gameObject.SetActive(false);
        obstacle.transform.SetParent(transform);

        EnsureQueueExists(config, out var queue);
        queue.Enqueue(obstacle);
    }


    private Obstacle CreateNew(ObstacleConfig config, WorldSpeed worldSpeed)
    {
        Obstacle instance = Instantiate(config.prefab, transform);
        instance.Initialize(worldSpeed, config);
        instance.gameObject.SetActive(false);
        return instance;
    }

    private void EnsureQueueExists(ObstacleConfig config, out Queue<Obstacle> queue)
    {
        if (!pools.TryGetValue(config, out queue))
        {
            queue = new Queue<Obstacle>();
            pools[config] = queue;
        }
    }
}
