using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SurvivalUIManager : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] private Image backgroundImage;

    [Header("Run UI")]
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI loopText;

    [Header("End Screen")]
    [SerializeField] private GameObject endScreen;

    private EventBinding<OnRoadEnvironmentChanged> envBinding;
    private EventBinding<OnPlayerDeathEvent> onDeathBinding;
    private EventBinding<OnRoadSectionChanged> sectionBinding;
    private EventBinding<OnLevelUpdateEvent> updateEventBinding;

    private void OnEnable()
    {
        envBinding = new EventBinding<OnRoadEnvironmentChanged>(OnEnvironmentChanged);
        EventBus<OnRoadEnvironmentChanged>.Register(envBinding);

        onDeathBinding = new EventBinding<OnPlayerDeathEvent>(OnSurvivalEnded);
        EventBus<OnPlayerDeathEvent>.Register(onDeathBinding);

        updateEventBinding = new EventBinding<OnLevelUpdateEvent>(OnUpdateEvent);
        EventBus<OnLevelUpdateEvent>.Register(updateEventBinding);

    }

    private void Start()
    {
        endScreen.SetActive(false);
        SetDistance(0);
        SetLoop(0);
        SetScore(0);
    }

    #region ENVIRONMENT (BACKGROUND)

    private void OnEnvironmentChanged(OnRoadEnvironmentChanged e)
    {
        StartCoroutine(TransitionBackground(e.environmentPreset));
    }

    private IEnumerator TransitionBackground(EnvironmentPreset env)
    {
        if (env == null || env.background == null)
            yield break;

        yield return backgroundImage.DOFade(0, 0.25f).WaitForCompletion();

        backgroundImage.sprite = env.background;

        yield return backgroundImage.DOFade(1, 0.25f).WaitForCompletion();
    }


    #endregion


    #region RUN UI (external updates)

    public void SetDistance(float value)
    {
        distanceText.text = $"{value:0.0} m";
    }

    public void SetScore(int value)
    {
        scoreText.text = value.ToString();
    }

    public void SetLoop(int loop)
    {
        loopText.text = $"LOOP {loop}";
        loopText.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f);
    }

    #endregion

    #region END RUN

    private void OnSurvivalEnded(OnPlayerDeathEvent e)
    {
        StartCoroutine(LoseSequence());

        scoreText.text = $"SCORE: {e.score}";
        distanceText.text = $"DISTANCE: {e.distance:0.0} m";
        loopText.text = $"STAGE: {e.loops}";
    } 
    
    private void OnUpdateEvent(OnLevelUpdateEvent e)
    {
        scoreText.text = $"SCORE: {e.score}";
        distanceText.text = $"DISTANCE: {e.distance:0.0} m";
        loopText.text = $"STAGE: {e.loops}";
    }

    [SerializeField] Image despedidoImage;
    [SerializeField] TextMeshProUGUI countDownText;
    [SerializeField] Image countDown;
    [SerializeField] AudioClip countdownBeep;

    IEnumerator LoseSequence()
    {
        endScreen.gameObject.SetActive(true);
        despedidoImage.gameObject.SetActive(true);
        despedidoImage.color = new Color(1, 1, 1, 0);

        yield return despedidoImage.DOFade(1, 0.5f).WaitForCompletion();

        yield return new WaitForSeconds(3f);

        // Mostrar countdown
        countDown.gameObject.SetActive(true);
        countDownText.gameObject.SetActive(true);
        countDown.color = new Color(1, 1, 1, 0);
        countDownText.alpha = 0;

        yield return countDown.DOFade(1, 0.5f).WaitForCompletion();
        yield return countDownText.DOFade(1, 0.5f).WaitForCompletion();

        //  Countdown retro
        for (int i = 10; i >= 0; i--)
        {
            countDownText.text = "CONTINUE " + i.ToString() + "?";
            AudioManager.Instance.PlaySFX(countdownBeep);
            // punch (retro feel)
            countDownText.transform.localScale = Vector3.one * 1.5f;
            countDownText.transform.DOScale(1f, 0.2f);

            yield return new WaitForSeconds(1f);
        }

        SceneManager.LoadScene("SampleScene");
        GameManager.Instance.ChangeState(GameState.Playing);
    }

    #endregion

    private void OnDisable()
    {
        EventBus<OnRoadEnvironmentChanged>.Deregister(envBinding);
        EventBus<OnPlayerDeathEvent>.Deregister(onDeathBinding);
        EventBus<OnRoadSectionChanged>.Deregister(sectionBinding);
    }
}