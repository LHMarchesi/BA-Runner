using TMPro;
using UnityEngine;

public class SurvivalManager : MonoBehaviour
{
    private float survivalTime;
    [SerializeField] private TextMeshProUGUI text;
    private SpeedData speedData;

    void Start()
    {
        speedData = LevelManager.instance.CurrentLevel.speedData;
    }

    void Update()
    {
        survivalTime += Time.deltaTime;

        float difficulty = survivalTime / 30f; // cada 30s escala
        text.text = $"Survival Time: {survivalTime:F1}s\nDifficulty: {difficulty:F2}";
        speedData.currentProgressionMultiplier = Mathf.Lerp(
            speedData.minProgressionMultiplier,
            speedData.maxProgressionMultiplier + difficulty,
            difficulty
        );
    }
}