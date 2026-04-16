using UnityEngine;
using UnityEngine.UI;

public class Npc_Car : MonoBehaviour
{
    [SerializeField] private SpeedData speedData;
    [SerializeField] private float speed;
    [SerializeField] Sprite[] sprites;

    private void Start()
    {
        Image image = GetComponent<Image>();
        if (sprites != null && sprites.Length > 0)
        {
            int randomIndex = Random.Range(0, sprites.Length);
            image.sprite = sprites[randomIndex];
        }
        speedData = LevelManager.instance.CurrentLevel.speedData;
    }

    private void Update()
    {
        transform.Translate(
    Vector3.left * speed * speedData.CurrentWorldSpeed * Time.deltaTime
);
    }
}
