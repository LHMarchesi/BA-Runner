using UnityEngine;

public class WorldSpeed : MonoBehaviour
{
    public float PlayerBoostMultiplier { get; set; } = 2f;
    public float ProgressionMultiplier { get; private set; } = 1f;

    private SpeedData currentSpeedData;

    public float CurrentWorldSpeed =>
        currentSpeedData.baseWorldSpeed *
        ProgressionMultiplier *
         Mathf.Lerp(1f, PlayerBoostMultiplier, 2f);

    public void SetSpeedData(SpeedData speedData)
    {
        Debug.Log("Data seteada a: " + speedData);
        currentSpeedData = speedData;
    }

    public void SetProgression(float normalized)
    {
        ProgressionMultiplier =
            Mathf.Lerp(
                currentSpeedData.minProgressionMultiplier,
                currentSpeedData.maxProgressionMultiplier,
                normalized);
    }
}