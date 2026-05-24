using TMPro;
using UnityEngine;

public class SurvivalManager : MonoBehaviour
{
    private float progessionTime;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] Level_Scriptable levelSurvival;


    void Update()
    {
        progessionTime += Time.deltaTime;

        float difficulty = progessionTime / 30f; // cada 30s escala
        text.text = $"Survival Time: {progessionTime:F1}s\nDifficulty: {difficulty:F2}";
        levelSurvival.speedData.currentProgressionMultiplier = Mathf.Lerp(
            levelSurvival.speedData.minProgressionMultiplier,
            levelSurvival.speedData.maxProgressionMultiplier + difficulty,
            difficulty
        );
    }
}