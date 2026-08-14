using System;
using System.Buffers.Text;
using System.Collections;
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
    public event Action<PlayerController> Died;

    [SerializeField] private UnityEngine.UI.Image playerImage;
    [SerializeField]
    private bool survivalMode;

    [SerializeField]
    private Sprite normalCar;

    [SerializeField]
    private Sprite damagedCar;
    public bool IsAlive => isAlive;

    public void SetHorizontalInput(float value)
    {
        if (!isAlive || gameIsPaused)
            return;

        inputX = Mathf.Clamp(
            value,
            -1f,
            1f
        );
    }

    public void TryChangeLane(int direction)
    {
        if (
            !isAlive ||
            gameIsPaused ||
            !canChangeLane
        )
        {
            return;
        }

        if (
            direction > 0 &&
            currentCarPosition < maxPosition
        )
        {
            currentCarPosition++;
            Move();
        }
        else if (
            direction < 0 &&
            currentCarPosition > 0
        )
        {
            currentCarPosition--;
            Move();
        }
    }
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
        EventBus<OnLevelStartEvent>.Deregister(levelStartBinding);
    }

    private void OnLevelStart(OnLevelStartEvent e)
    {
        SpeedData = e.levelSpeedData;
    }

    private void OnLevelCompleted(OnLevelCompletedEvent e)
    {
        canCollide = false;

        inputX = 0f;
        currentVelocityX = 0f;
        wasBoosting = false;

        isAlive = false;
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
        if (playerImage != null &&
        normalCar != null)
        {
            playerImage.sprite =
                normalCar;
        }
        targetPosition = startPosition.position;
        transform.position = targetPosition;
        baseX = transform.position.x;

        if (startSound != null)
        {
            AudioManager.Instance.PlaySFX(startSound);
        }
    }

    public void OnMove(
    InputAction.CallbackContext context
)
    {
        Vector2 input =
            context.ReadValue<Vector2>();

        SetHorizontalInput(
            input.x
        );

        if (!context.started)
            return;

        if (input.y > 0f)
        {
            TryChangeLane(1);
        }
        else if (input.y < 0f)
        {
            TryChangeLane(-1);
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

    private void OnTriggerEnter2D(
    Collider2D collision
)
    {
        if (!canCollide || !isAlive)
            return;

        canCollide = false;
        canChangeLane = false;
        isAlive = false;

        inputX = 0f;
        currentVelocityX = 0f;

        if (
     playerImage != null &&
     damagedCar != null
 )
        {
            playerImage.sprite =
                damagedCar;
        }
        if (survivalMode)
        {
            WorldSpeed.SetFrozen(true);

            Died?.Invoke(this);
        }
        else
        {
            transform.SetParent(
                collision.transform
            );

            EventBus<OnPlayerDeathEvent>.Raise(
                new OnPlayerDeathEvent()
            );
        }

        AudioManager.Instance.PlaySFX(
            crashSound
        );
    }

    public void Revive()
    {
        CancelInvoke();

        currentCarPosition = 0;

        transform.SetParent(
            lanes[0],
            true
        );

        transform.position =
            startPosition.position;

        targetPosition =
            startPosition.position;

        baseX =
            startPosition.position.x;

        currentVelocityX = 0f;
        inputX = 0f;

        isAlive = true;
        canChangeLane = true;

        if (
     playerImage != null &&
     normalCar != null
 )
        {
            playerImage.sprite =
                normalCar;
        }

        WorldSpeed.SetFrozen(false);

        StartCoroutine(
            ReviveInvulnerability()
        );
    }

    private IEnumerator ReviveInvulnerability()
    {
        canCollide = false;

        yield return new WaitForSeconds(
            1.5f
        );

        canCollide = true;
    }
}
