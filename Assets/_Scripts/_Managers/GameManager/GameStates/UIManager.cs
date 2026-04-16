using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField]GameObject loseScreen;
    [SerializeField]Image BackgroundImage;
    [SerializeField] SceneTransition transitionToCinematics;


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
        BackgroundImage.sprite = GameManager.Instance.CurrentLevel.levelBackground;
    }
    public void ToggleLoseScreen(bool value)
    {
        loseScreen.gameObject.SetActive(value);
    }
    public void WinTranstion()
    {
        transitionToCinematics.StartTransition();
    }
}
