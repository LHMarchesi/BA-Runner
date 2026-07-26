using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SportCarBehavior : MonoBehaviour, IObstacleBehavior
{
    private enum State
    {
        Warning,
        Charging
    }

    private State state;
    private SportCarObstacleConfig config;

    [SerializeField] private GameObject indicator;
    [SerializeField] private RawImage indicatorImage;
    [SerializeField] private float indicatorScrollSpeed = 0.5f;
    [SerializeField] private TextMeshProUGUI indicatorText;

    private float timer;

    public void OnSpawned(ObstacleConfig obstacleConfig)
    {
        config = obstacleConfig as SportCarObstacleConfig;

        if (config == null)
        {
            Debug.LogError(
                $"{nameof(SportCarBehavior)} recibió una configuración incorrecta."
            );
            return;
        }

        timer = 0f;
        state = State.Warning;

        indicator.SetActive(true);

        Rect uv = indicatorImage.uvRect;
        uv.x = 0f;
        indicatorImage.uvRect = uv;

        UpdateIndicatorText();
    }

    public void Tick(float worldSpeed)
    {
        switch (state)
        {
            case State.Warning:
                timer += Time.deltaTime;

                ScrollIndicator();
                UpdateIndicatorText();

                if (timer >= config.warningDuration)
                {
                    indicator.SetActive(false);
                    state = State.Charging;
                }

                break;

            case State.Charging:
                transform.localPosition +=
                    Vector3.right *
                    config.chargeSpeed *
                    Time.deltaTime;

                break;
        }
    }

    private void ScrollIndicator()
    {
        Rect uv = indicatorImage.uvRect;
        uv.x -= indicatorScrollSpeed * Time.deltaTime;
        indicatorImage.uvRect = uv;
    }

    private void UpdateIndicatorText()
    {
        if (indicatorText == null || config.warningDuration <= 0f)
            return;

        float normalizedTime = timer / config.warningDuration;

        int countdownNumber = normalizedTime switch
        {
            < 1f / 3f => 3,
            < 2f / 3f => 2,
            _ => 1
        };

        indicatorText.text = countdownNumber.ToString();
    }

    public bool ShouldDespawn(float despawnXThreshold)
    {
        return transform.localPosition.x > despawnXThreshold;
    }
}