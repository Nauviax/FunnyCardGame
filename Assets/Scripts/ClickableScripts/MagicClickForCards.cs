using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicClickForCards : MonoBehaviour, IClickable
{
	GameLogic gameLogic;
	void Start()
	{
		gameLogic = GameObject.Find("GameLogic").GetComponent<GameLogic>(); // Gets the gameLogic script from the object
	}

	public void onClick()
	{
		gameLogic.GenerateRandomPlayerCard(); // Will get a random premade card and add it to the players hand
		gameLogic.board.playerDust += 1000; // Give player dust
	}
}
