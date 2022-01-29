using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickForSTARTGAME : MonoBehaviour, IClickable
{
	GameLogic gameLogic;
	CameraController camController;
	Hand hand;
	void Start() // Gets references to many gameObjects to make them run nicely
    {
		gameLogic = GameObject.Find("GameLogic").GetComponent<GameLogic>(); // Gets the gameLogic script from the object
		camController = GameObject.Find("GameLogic").GetComponent<CameraController>(); // Ditto for camera
		hand = GameObject.Find("GameLogic").GetComponent<Hand>(); // Adds Lachlan to Fortnite
	}

	public void onClick()
	{
		camController.Begin(); // Update speed to get there faster
		hand.lookUp(); // Move camera to hand
		gameLogic.BeginGame(); // Starts the game
		Debug.Log("Game starting");
		// Animate
	}
}
