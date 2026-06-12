using System;
using UnityEngine;
using UnityEngine.UI;

public class Obstacle : MonoBehaviour
{
    private ObstacleConfig config;
    [SerializeField] private float speed;
    private IObstacleBehavior[] behaviors;
    [SerializeField] private WorldSpeed worldSpeed;
    [SerializeField] Sprite[] sprites;
    public Action OnDespawn;
    private void Start()
    {
        Image image = GetComponent<Image>();
        if (sprites != null && sprites.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, sprites.Length);
            image.sprite = sprites[randomIndex];
        }
    }

    public void Initialize(WorldSpeed worldSpeed, ObstacleConfig config = null)
    {
        this.worldSpeed = worldSpeed;
        this.config = config;
        behaviors = GetComponents<IObstacleBehavior>();
    }

    private void OnEnable()
    {
        if (behaviors == null) return;
        foreach (var b in behaviors)
            b.OnSpawned(config);
    }

    private void Update()
    {
        if (worldSpeed == null || behaviors == null) return;

        float speed = worldSpeed.CurrentWorldSpeed;

        foreach (var b in behaviors)
            b.Tick(this.speed * speed);

        CheckDespawn();
    }


    // ── Despawn ─────────────────────────────────────────────────────────

    private void CheckDespawn()
    {
        float threshold = config != null ? config.despawnXThreshold : -15f;

        foreach (var b in behaviors)
        {
            if (b.ShouldDespawn(threshold))
            {
                Despawn();
                return;
            }
        }
    }

    private void Despawn()
    {
        if (OnDespawn != null)
        {
            // Obstacle pooled: devolver al pool (pool llama SetActive(false)).
            OnDespawn.Invoke();
            OnDespawn = null;
        }
        else
        {
            // Obstacle no-pooled (señal de velocidad): auto-destruir.
            Destroy(gameObject);
        }
    }
}
