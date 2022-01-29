using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickForEndTurn : MonoBehaviour, IClickable
{
	GameLogic gameLogic;
    void Start()
    {
		gameLogic = GameObject.Find("GameLogic").GetComponent<GameLogic>(); // Gets the gameLogic script from the object
    }

	public void onClick()
	{
		gameLogic.EndTurn(); // Ends the turn
		// Animate
	}
}
