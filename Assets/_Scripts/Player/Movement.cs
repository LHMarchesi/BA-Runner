using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    private Vector3 targetPosition;

    [SerializeField] float constantSpeed;
    [SerializeField] float decerationSpeed;
    [SerializeField] float accelerationSpeed;
    [SerializeField] float currentSpeed;

    private int currentCarPosition;
    private int maxPosition = 3;
    [SerializeField] float laneOffset;
    [SerializeField] Transform startPosition;
    [SerializeField] private Transform[] lanes;

    private void Start()
    {
        targetPosition = startPosition.position;
        transform.position = targetPosition;
    }

    private bool canMove = true;
    public void OnMove(InputAction.CallbackContext context)
    {
        if (!context.started || !canMove) return;

        Vector2 input = context.ReadValue<Vector2>();
        float vertical = input.y;
        float horizontal = input.x;

        if (vertical > 0 && currentCarPosition < maxPosition)
        {
            currentCarPosition++;
            Move();
        }
        else if (vertical < 0 && currentCarPosition > 0)
        {
            currentCarPosition--;
            Move();
        }

        if (horizontal > 0)
        {
            Accelerate();
        }
        else if (horizontal < 0)
        {
            //Decelerate();
        }
    }

    private void Accelerate()
    {
        throw new NotImplementedException();
    }

    private void Move()
    {
        canMove = false;
        UpdatePosition();
        Invoke(nameof(ResetMove), 0.1f); // cooldown
    }

    private void ResetMove()
    {
        canMove = true;
    }


    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, 10f * Time.deltaTime);
        transform.Translate(transform.right * constantSpeed * Time.deltaTime);
        transform.Translate(transform.right * decerationSpeed * Time.deltaTime);

    }
    private void UpdatePosition()
    {
        Transform lane = lanes[currentCarPosition];
        transform.SetParent(lane);

        transform.SetAsLastSibling();

        targetPosition = new Vector3(
            startPosition.position.x,
            startPosition.position.y + currentCarPosition * laneOffset,
            startPosition.position.z
        );

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collided with: " + collision.gameObject.name);
    }
}
