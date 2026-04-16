using UnityEngine;

public class Npc_Car : MonoBehaviour
{
    [SerializeField] private SpeedData speedData;
    [SerializeField] private float speed;

    private void Start()
    {
        speedData = LevelManager.instance.CurrentLevel.speedData;
    }

    private void Update()
    {
        transform.Translate(
    Vector3.left * speed * speedData.CurrentWorldSpeed * Time.deltaTime
);
    }
}
