using UnityEngine;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private RectTransform indicatorPoint;

    public Button Button => button;
    public RectTransform IndicatorPoint => indicatorPoint;
}