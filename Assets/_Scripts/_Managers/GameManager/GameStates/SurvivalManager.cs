using TMPro;
using UnityEngine;

public class SurvivalManager : MonoBehaviour
{
    private float survivalTime;
    [SerializeField] private TextMeshProUGUI text;
    private Level_Scriptable currentLevel;

    void Start()
    {
        currentLevel = ProgressionManager.Instance.CurrentLevel;
    }

    void Update()
    {
        survivalTime += Time.deltaTime;

        float difficulty = survivalTime / 30f; // cada 30s escala
        text.text = $"Survival Time: {survivalTime:F1}s\nDifficulty: {difficulty:F2}";
        currentLevel.speedData.currentProgressionMultiplier = Mathf.Lerp(
            currentLevel.speedData.minProgressionMultiplier,
            currentLevel.speedData.maxProgressionMultiplier + difficulty,
            difficulty
        );
    }
}