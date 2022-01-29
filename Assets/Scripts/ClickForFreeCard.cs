using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickForFreeCard : MonoBehaviour, IClickable
{
	GameLogic gameLogic;
    void Start()
    {
		gameLogic = GameObject.Find("GameLogic").GetComponent<GameLogic>(); // Gets the gameLogic script from the object
    }

	public void onClick()
	{
		gameLogic.FreeCardGet(); // Requests a free card (Will not give anything if card was taken this turn)
	}
}
