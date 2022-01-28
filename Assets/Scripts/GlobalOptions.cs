using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalOptions : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 60; // Sets the max FPS, and removes that horrible wining in my ear
    }
}
