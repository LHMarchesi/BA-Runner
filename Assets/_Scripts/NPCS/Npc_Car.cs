using UnityEngine;

public class Npc_Car : MonoBehaviour
{
    public float speed;

    private void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);
    }
}
