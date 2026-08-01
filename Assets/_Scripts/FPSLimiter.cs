using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    private int limiteDeFps = 60;
        

    void Start()
    {
        Application.targetFrameRate = limiteDeFps;
    }
}
