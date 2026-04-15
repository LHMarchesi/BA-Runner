using UnityEngine;
using UnityEngine.UI;

public class ProgessBar : MonoBehaviour
{
    public Slider slider;

    public void UpdateProgess(float progess, float maxPoints)
    {
        float update = progess / maxPoints;
        slider.value = update;
    }

    public void ResetValues()
    {
        slider.value = 0;
    }
}
