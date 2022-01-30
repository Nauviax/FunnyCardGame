using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieAfterFalling : MonoBehaviour
{
    void Update() // Just a little bit of cleanup
    {
        if (transform.position.y < -25) // If fallen, (Floor is -30)
		{
			Destroy(gameObject); // Die
		}
    }
}
