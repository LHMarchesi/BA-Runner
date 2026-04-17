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
    [SerializeField] GameObject winBtnContinue;
    [SerializeField] Image BackgroundImage;
    [SerializeField] Image despedidoImage;
    [SerializeField] Image countDown;
    [SerializeField] Image WinImage;
    [SerializeField] Image[] stars;
    [SerializeField] TextMeshProUGUI countDownText;
    [SerializeField] SceneTransition transitionToCinematics;

    [SerializeField] AudioClip countdownBeep;
    [SerializeField] private AudioClip starCollectSFX;
    [SerializeField] private AudioClip lvlCompleted;

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
       StartCoroutine(WinSequence());
    }

    IEnumerator WinSequence()
    {
        foreach (var star in stars)
        {
            star.gameObject.SetActive(false);
        }
        winBtnContinue.gameObject.SetActive(false);
        WinImage.sprite = LevelManager.instance.CurrentLevel.winLevelImage;
        WinImage.gameObject.SetActive(true);
        WinImage.color = new Color(1, 1, 1, 0);

        yield return WinImage.DOFade(1, 0.5f).WaitForCompletion();

        yield return new WaitForSeconds(0.5f);

        foreach (var star in stars)
        {
            star.gameObject.SetActive(true);
            star.transform.localScale = Vector3.zero;
            star.color = new Color(1, 1, 1, 0);
           
        }

        for (int i = 0; i < stars.Length; i++)
        {
            var star = stars[i];
            star.DOFade(1, 0.5f).WaitForCompletion();
            Sequence seq = DOTween.Sequence();

            seq.Append(star.transform.DOScale(1.3f, 0.2f).SetEase(Ease.OutBack)); // crece con rebote
            seq.Append(star.transform.DOScale(1f, 0.1f)); // vuelve a tamaño normal

            // pequeño stretch (feeling extra)
            seq.Join(star.transform.DOScaleY(0.8f, 0.1f).SetLoops(2, LoopType.Yoyo));

            yield return seq.WaitForCompletion();
            AudioManager.Instance.PlaySFX(starCollectSFX);
            yield return new WaitForSeconds(0.5f); // delay entre estrellas
        }

        AudioManager.Instance.PlaySFX(lvlCompleted);
        yield return new WaitForSeconds(.2f);
        winBtnContinue.gameObject.SetActive(true);
    }
}
