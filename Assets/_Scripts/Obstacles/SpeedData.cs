using UnityEngine;

[CreateAssetMenu(fileName = "SpeedData", menuName = "Game/SpeedData")]
public class SpeedData : ScriptableObject
{
    public float baseWorldSpeed;
    public float minProgressionMultiplier;
    public float maxProgressionMultiplier;
}
