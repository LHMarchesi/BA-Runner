using UnityEngine;

[CreateAssetMenu(fileName = "SpeedData", menuName = "Game/SpeedData")]
public class SpeedData : ScriptableObject
{
    public float baseWorldSpeed;
    public float boostMultiplier;
    public float minProgressionMultiplier;
    public float maxProgressionMultiplier;
    public float currentProgressionMultiplier;

    public float CurrentWorldSpeed =>
        baseWorldSpeed * currentProgressionMultiplier * boostMultiplier;
}
