using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] GameObject loseScreen;
    [SerializeField] Image BackgroundImage;
    [SerializeField] Image despedidoImage;
    [SerializeField] Image countDown;
    [SerializeField] TextMeshProUGUI countDownText;
    [SerializeField] SceneTransition transitionToCinematics;

    [SerializeField] AudioClip countdownBeep;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        loseScreen.gameObject.SetActive(false);
    }

    private void Start()
    {
        var currentLevel = LevelManager.instance.CurrentLevel;
        if (currentLevel != null && BackgroundImage != null)
        {
            BackgroundImage.sprite = currentLevel.levelBackground;
        }
    }
    public void ToggleLoseSequence(bool value)
    {
        loseScreen.gameObject.SetActive(value);
        StartCoroutine(LoseSequence());
    }

    IEnumerator LoseSequence()
    {
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

    public void WinTranstion()
    {
        transitionToCinematics.StartTransition();
    }
}
