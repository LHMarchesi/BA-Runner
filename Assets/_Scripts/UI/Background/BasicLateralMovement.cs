using UnityEngine;
using UnityEngine.UI;


public class BasicLateralMovement : MonoBehaviour
{
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private bool isUsingSpeedData;
    [SerializeField] private WorldSpeed worldSpeed;

    private Material material;
    private Vector2 offset;

    void Start()
    {
        Image img = GetComponent<Image>();

        material = Instantiate(img.material);
        img.material = material;
    }

    void Update()
    {
        if (isUsingSpeedData)
            offset.x += worldSpeed.CurrentWorldSpeed/2 * Time.deltaTime;
        else
            offset.x += speed * Time.deltaTime;

        material.mainTextureOffset = offset;
    }

}
