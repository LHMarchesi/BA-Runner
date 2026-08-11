using UnityEngine;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private RectTransform indicatorPoint;
    [SerializeField] private bool useGlobalIndicator = true;
    public bool UseGlobalIndicator => useGlobalIndicator;
    public Button Button => button;
    public RectTransform IndicatorPoint => indicatorPoint;
}