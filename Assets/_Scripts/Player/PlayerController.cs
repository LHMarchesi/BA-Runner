using System;
using System.Buffers.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float laneOffset;
    [SerializeField] Transform startPosition;
    [SerializeField] private Transform[] lanes;
    [SerializeField] private float maxDistance;
    [SerializeField] private float boostForce;
    [SerializeField] private float springStrength;
    [SerializeField] private float maxReturnForce;
    [SerializeField] private float damping;
    [SerializeField] private AudioClip crashSound;
    [SerializeField] private AudioClip startSound;
    [SerializeField] private SpeedData onDeathSpeedData;

    private Vector3 targetPosition;
    private int currentCarPosition;
    private int maxPosition = 3;
    private float baseX;
    private bool canMove = true;
    private bool isAlive = true;
    public bool canCollide = true;
    private float inputX;
    private float currentVelocityX;
    [SerializeField] private float brakingForce;

    private SpeedData speedData => GameManager.Instance.CurrentLevel?.speedData;

    private void Start()
    {
        
        targetPosition = startPosition.position;
        transform.position = targetPosition;
        baseX = transform.position.x;
        LevelManager.OnLevelComplete += () => canCollide = false;

        if (startSound != null)
        {
            AudioManager.Instance.PlaySFX(startSound);
        }
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        inputX = input.x;

        if (!context.started || !canMove) return;

        float vertical = input.y;

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
        if (!isAlive) return;
        HandleBoost();
        transform.position += Vector3.right * currentVelocityX * Time.deltaTime;

        transform.position = new Vector3(
      transform.position.x,
      Mathf.Lerp(transform.position.y, targetPosition.y, 10f * Time.deltaTime),
      transform.position.z
  );
    }

    private void HandleBoost()
    {
        if (!canMove) return;
        float currentX = transform.position.x;
        float distance = baseX - currentX;

        bool canAccelerate = Mathf.Abs(currentX - baseX) < maxDistance;
        // Boost
        if (inputX > 0 && canAccelerate)
        {
            currentVelocityX = boostForce;
            if (speedData != null) speedData.boostMultiplier = currentVelocityX / 250; // Update boost multiplier based on current velocity          
        }
        else
        {
            if (speedData != null) speedData.boostMultiplier = 1f; // Reset boost multiplier when not boosting
        }


        // Resorte
        float returnForce = distance * springStrength;
        returnForce = Mathf.Clamp(returnForce, -maxReturnForce, maxReturnForce);

        currentVelocityX += returnForce * Time.deltaTime;

        if (inputX < 0)
        {
            currentVelocityX += returnForce * brakingForce * Time.deltaTime;

        }

        // Damping
        currentVelocityX *= damping;
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
        if(!canCollide || !isAlive) return;
        canCollide = false;
        canMove = false;
        isAlive = false;
        GameManager.Instance.SetSpeedData(onDeathSpeedData);
        
        transform.SetParent(collision.transform);

        GameManager.Instance.ChangeState(GameState.Lose);
        AudioManager.Instance.PlaySFX(crashSound);  
        //trigger end game 
    }
}
