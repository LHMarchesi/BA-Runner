using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FullscreenToggle : MonoBehaviour
{
    public void Change ()
    {
        Screen.fullScreen = !Screen.fullScreen;
        print("changed screen mode");
    }
}