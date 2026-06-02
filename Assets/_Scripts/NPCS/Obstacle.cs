using UnityEngine;
using UnityEngine.UI;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private SpeedData speedData;
    [SerializeField] private float speed;
    [SerializeField] private WorldSpeed WordSpeed;
    [SerializeField] Sprite[] sprites;

    private void Start()
    {
        Image image = GetComponent<Image>();
        if (sprites != null && sprites.Length > 0)
        {
            int randomIndex = Random.Range(0, sprites.Length);
            image.sprite = sprites[randomIndex];
        }
    }

    public void Initialize(WorldSpeed worldSpeed)
    {
        WordSpeed = worldSpeed;
    }

    public void SetSpeedData(SpeedData speedData)
    {
        this.speedData = speedData;
    }

    private void Update()
    {
        transform.Translate(
    Vector3.left * speed * WordSpeed.CurrentWorldSpeed * Time.deltaTime
);
    }
}
