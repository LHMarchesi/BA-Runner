using TMPro;
using UnityEngine;

public class SurvivalManager : MonoBehaviour
{
    private float progessionTime;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] Level_Scriptable levelSurvival;
    [SerializeField] WorldSpeed worldSpeed;


    void Update()
    {
        progessionTime += Time.deltaTime;

        
    }
}