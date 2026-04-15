using UnityEngine;

[CreateAssetMenu(fileName = "SpeedData", menuName = "Game/SpeedData")]
public class SpeedData : ScriptableObject
{
    public float baseWorldSpeed = .5f;

    public float progressionMultiplier = 1f;
    public float boostMultiplier = 1f;

    public float CurrentWorldSpeed =>
        baseWorldSpeed * progressionMultiplier * boostMultiplier;
}
