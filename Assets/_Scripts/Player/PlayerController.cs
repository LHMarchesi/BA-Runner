using System;
using System.Buffers.Text;
using Unity.VisualScripting;
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
    [SerializeField] private AudioClip moveLaneSound;
    [SerializeField] private AudioClip boostSound;

    private Vector3 targetPosition;
    private int currentCarPosition;
    private int maxPosition = 3;
    private float baseX;
    private bool canChangeLane = true;
    private bool isAlive = true;
    public bool canCollide = true;
    private float inputX;
    private float currentVelocityX;
    [SerializeField] private float brakingForce;
    private SpeedData SpeedData;
    private bool wasBoosting;
    [SerializeField] WorldSpeed WorldSpeed;

    EventBinding<OnLevelCompletedEvent> levelResultBinding;
    EventBinding<OnLevelStartEvent> levelStartBinding;
    EventBinding<OnPauseEvent> pauseEventBinding;

    private float currentPlayerBoostMultiplier;
    private bool gameIsPaused = false;

    private void OnEnable()
    {
        levelResultBinding = new EventBinding<OnLevelCompletedEvent>(OnLevelCompleted);
        EventBus<OnLevelCompletedEvent>.Register(levelResultBinding);

        levelStartBinding = new EventBinding<OnLevelStartEvent>(OnLevelStart);
        EventBus<OnLevelStartEvent>.Register(levelStartBinding);

        pauseEventBinding = new EventBinding<OnPauseEvent>(OnPauseEventTriggered);
        EventBus<OnPauseEvent>.Register(pauseEventBinding);
    }

    private void OnDisable()
    {
        EventBus<OnLevelCompletedEvent>.Deregister(levelResultBinding);
        EventBus<OnPauseEvent>.Deregister(pauseEventBinding);
    }

    private void OnLevelStart(OnLevelStartEvent e)
    {
        SpeedData = e.levelSpeedData;
    }

    private void OnLevelCompleted(OnLevelCompletedEvent e)
    {
        canCollide = false;
    }

    private void OnPauseEventTriggered(OnPauseEvent e)
    {
        gameIsPaused = e.isPaused;
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.started || !isAlive) return;

        if (!gameIsPaused)
        {
            GameManager.Instance.IsPausing = true;
            GameManager.Instance.ChangeState(GameState.Pause);
        }
        else
        {
            GameManager.Instance.ChangeState(GameState.Playing);
        }
    }

    private void Start()
    {
        targetPosition = startPosition.position;
        transform.position = targetPosition;
        baseX = transform.position.x;

        if (startSound != null)
        {
            AudioManager.Instance.PlaySFX(startSound);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        inputX = input.x;

        if (gameIsPaused)
            return;

        if (!context.started || !canChangeLane)
            return;

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
        AudioManager.Instance.PlaySFX(moveLaneSound);
        canChangeLane = false;
        UpdatePosition();
        Invoke(nameof(ResetLaneChange), 0.1f);
    }

    private void ResetLaneChange()
    {
        canChangeLane = true;
    }

    private void Update()
    {
        if (!isAlive || gameIsPaused) return;

        HandleBoost();

        transform.position += Vector3.right * currentVelocityX * Time.deltaTime;

        float minX = baseX - maxDistance;
        float maxX = baseX + maxDistance;

        if (transform.position.x < minX)
        {
            transform.position = new Vector3(minX, transform.position.y, transform.position.z);
            currentVelocityX = 0; // Matamos la fuerza acumulada para que no tiemble
        }
        else if (transform.position.x > maxX)
        {
            transform.position = new Vector3(maxX, transform.position.y, transform.position.z);
            currentVelocityX = 0; // Matamos la fuerza acumulada para que no tiemble
        }

        // 2. Aplicamos el movimiento vertical suavizado
        transform.position = new Vector3(
            transform.position.x,
            Mathf.Lerp(transform.position.y, targetPosition.y, 10f * Time.deltaTime),
            transform.position.z
        );
    }

    private void HandleBoost()
    {
        float currentX = transform.position.x;

        bool canAccelerate = (baseX - currentX) < maxDistance;
        bool isBoosting = inputX > 0 && canAccelerate;

        float targetBoost = isBoosting ? 2.5f : 1f;

        if (WorldSpeed != null)
        {
            WorldSpeed.PlayerBoostMultiplier = Mathf.Lerp(
                WorldSpeed.PlayerBoostMultiplier,
                targetBoost,
                10f * Time.deltaTime
            );
        }

        if (isBoosting)
        {
            if (!wasBoosting)
            {
                AudioManager.Instance.PlaySFX(boostSound);
            }
            currentVelocityX = boostForce;
        }
        wasBoosting = isBoosting;

        float displacement = baseX - transform.position.x;
        float springForce = displacement * springStrength;
        float dampingForce = -currentVelocityX * damping;
        float force = springForce + dampingForce;

        currentVelocityX += force * Time.deltaTime;


        if (inputX < 0)
        {
            currentVelocityX -= brakingForce * Time.deltaTime;
        }

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
        if (!canCollide || !isAlive) return;
        canCollide = false;
        canChangeLane = false;
        isAlive = false;

        transform.SetParent(collision.transform);

        EventBus<OnPlayerDeathEvent>.Raise(new OnPlayerDeathEvent());

        AudioManager.Instance.PlaySFX(crashSound);
    }
}