using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Board // Stores the locations of all cards and variables during a game, and also the players deck outside of a game
{
	// Game values, not used outside of a game
	public int playerDust; // Player currency and health during a game. Aquired when attacking the AI directly or when a player card dies. Lost when player is attacked directly or plays a card
	public int opponentDust; // Opponent health. AI will not require currency to play cards, and can only gain dust by attacking the player
	// Note the game ends when either of the above values drops BELOW 0
	public bool playerTookFreeCard; // Player may only take one free card per turn, but isn't required to do so

	// These cards are currently on the board
	public Card[] upcomingRowFront; // Not controlled by player
	public Card[] opponentRowFront; // Not controlled by player
	public Card[] upcomingRowBack; // Not controlled by player
	public Card[] opponentRowBack; // Not controlled by player
	public Card[] playerRow; // Player played cards

	// These cards are either in the players hand or in the draw pile
	public List<Card> gameHand; // Cards the player can play currently
	public List<Card> gameDeck; // Cards in the draw from pile. Any card that isn't dead or in hand/on board

	// These are all cards the player owns. gameDeck will be set to this at the start of a game. Does not include free cards
	public List<Card> ownedCards; // All cards owned by player, should be (Basically) read only during a game
}
public class GameLogic : MonoBehaviour
{
	public Board board; // Holds most board state information
	[SerializeField] GameObject cardObj; // A reference to the prefab for the double sided card

	float[] initialCoords = new float[3] { 0, 0, 0 }; // This and next two lines used to position cards
	float verticalSpacing = 10;
	float horisontalSpacing = 7;

	void Start()
    {
		board = new Board();
		board.upcomingRowFront = new Card[4]; // This may need to be rewritten later to support AI having single sided cards, or have Card remove one side on creation should it be prompted to do so !!!
		board.opponentRowFront = new Card[4];
		board.playerRow = new Card[4];
		board.ownedCards = new List<Card>(); // Will hold all cards the player owns. Starting cards should be set here !!!

		// Testing lines below here
		BeginGame();

		Card tehJohnCard = Instantiate(cardObj).GetComponent<Card>();
		tehJohnCard.SetStats(1,2,3,4);
		PlaceCard(tehJohnCard, 1);
		// CreateCards(); // Currently just fills the game board with cards for testing
    }

    void Update()
    {
        
    }

	void CreateCards() // Fills the board, not intended to be used for the actual game, just debugging etc
	{

		for (int xx = 0; xx < 4; xx++)
		{
			board.upcomingRowFront[xx] = Instantiate(cardObj).GetComponent<Card>();
			board.upcomingRowFront[xx].SetStats(1, 2, 4, 8);
			board.upcomingRowFront[xx].SetPos(initialCoords[0] + xx * horisontalSpacing, initialCoords[1] + verticalSpacing * 2, initialCoords[2]);
			board.opponentRowFront[xx] = Instantiate(cardObj).GetComponent<Card>();
			board.opponentRowFront[xx].SetStats(3, 4, 4, 3);
			board.opponentRowFront[xx].SetPos(initialCoords[0] + xx * horisontalSpacing, initialCoords[1] + verticalSpacing, initialCoords[2]);
			board.playerRow[xx] = Instantiate(cardObj).GetComponent<Card>();
			board.playerRow[xx].SetStats(xx, 2+xx, 3+xx, 4+xx);
			board.playerRow[xx].SetPos(initialCoords[0] + xx * horisontalSpacing, initialCoords[1], initialCoords[2]);
		}
	}
	public void BeginGame() // Set initial values, deal starting hand, and generate AI cards for first round
	{
		// Set initial values
		board.gameDeck = new List<Card>();
		board.gameHand = new List<Card>();
		// Deal starting hand
		/*for (int ii = 0; ii < 4; ii++) // 4 starting cards for now
		{
			board.gameDeck.Add(new Card()); // Wrong line is autofill, want to take card from deck put into hand
		}*/
		// Generate AI cards for first round
	}
	public void EndTurn() // Called when player ends their turn. Will preform attacking and decide if the game is over or set up for the next turn
	{
		// Preform player attacks
		for (int ii = 0; ii < 4; ii++) // 4 cards in player row
		{
			if (board.playerRow[ii] != null) // Empty spots do not attack
			{
				// Attack front cards
				if (board.opponentRowFront[ii] == null) // If the card is unopposed,
				{
					board.opponentDust -= board.playerRow[ii].DamageFront; // Deal damage directly
				}
				else
				{
					board.opponentRowFront[ii].HealthFront -= board.playerRow[ii].DamageFront; // Deal damage to card
					if (board.opponentRowFront[ii].HealthFront <= 0) // If card is now dead,
					{
						board.opponentRowFront[ii].Destroy(); // Destroy that card 
						board.opponentRowFront[ii] = null; // And remove it from the board
					}
				}
				// Attack back cards
				if (board.opponentRowBack[ii] == null) // If the card is unopposed,
				{
					board.opponentDust -= board.playerRow[ii].DamageBack; // Deal damage directly
				}
				else
				{
					board.opponentRowBack[ii].HealthBack -= board.playerRow[ii].DamageBack; // Deal damage to card
					if (board.opponentRowBack[ii].HealthBack <= 0) // If card is now dead,
					{
						board.opponentRowBack[ii].Destroy(); // Destroy that card 
						board.opponentRowBack[ii] = null; // And remove it from the board
					}
				}
			}
		}
		// Determine if game has been won
		if (board.opponentDust < 0) // Opponent must have LESS than 0 dust to end the game
		{
			GameEnd(true); // Player has won
			return; // Stop game
		}
		// Move cards down and preform enemy attacks
		for (int ii = 0; ii < 4; ii++)
		{
			// Move cards down
			if (board.upcomingRowFront[ii] != null) // If a card wants to move down,
			{
				if (board.opponentRowFront[ii] == null) // And it is not being blocked,
				{
					board.opponentRowFront[ii] = board.upcomingRowFront[ii]; // Move that card forwards
					board.upcomingRowFront[ii] = null; // And remove it from upcoming
				}
			}
			// Front cards attack
			if (board.opponentRowFront[ii] != null) // Empty spots do not attack
			{
				if (board.playerRow[ii] == null) // If the card is unopposed,
				{
					board.playerDust -= board.opponentRowFront[ii].DamageFront; // Deal damage directly
				}
				else
				{
					board.playerRow[ii].HealthFront -= board.opponentRowFront[ii].DamageFront; // Deal damage to card
					if (board.playerRow[ii].HealthFront <= 0) // If card is now dead,
					{
						board.playerDust += board.playerRow[ii].DustValue; // Player gets a dust refund
						board.playerRow[ii].Destroy(); // Destroy that card 
						board.playerRow[ii] = null; // And remove it from the board
					}
				}
			}
			// Back cards attack
			if (board.opponentRowBack[ii] != null) // Empty spots do not attack
			{
				if (board.playerRow[ii] == null) // If the card is unopposed,
				{
					board.playerDust -= board.opponentRowBack[ii].DamageBack; // Deal damage directly
				}
				else
				{
					board.playerRow[ii].HealthBack -= board.opponentRowBack[ii].DamageBack; // Deal damage to card
					if (board.playerRow[ii].HealthBack <= 0) // If card is now dead,
					{
						board.playerDust += board.playerRow[ii].DustValue; // Player gets a dust refund
						board.playerRow[ii].Destroy(); // Destroy that card 
						board.playerRow[ii] = null; // And remove it from the board
					}

				}
			}
		}

		// Determine if game has been lost
		if (board.playerDust < 0) // Player must have LESS than 0 dust to end the game
		{
			GameEnd(false); // Player has lost
			return; // Stop game
		}

		// Generate new AI cards for next turn

		// Set up for next turn
		board.playerTookFreeCard = false; // Player may retrieve a new free card
		// Draw card from deck



	}
	public bool PlaceCard(Card card, int location) // Will place the given card at a certian location on the board. Returns true if sucsessful, false if failed
	{
		if (board.playerRow[location] != null) // If location is taken, fail
		{
			return false;
		}
		if (card.DustValue > board.playerDust) // If I can't afford to place this card, and will die because of it, fail
		{
			return false;
		}
		if (board.gameHand.Contains(card)) // If I have the card in my hand, (I'ma idiot proof the fuck out of this)
		{
			board.playerDust -= card.DustValue; // Subtract the cost of the card,
			board.playerRow[location] = card; // Place the card,
			card.SetPos(initialCoords[0] + location * horisontalSpacing, initialCoords[1], initialCoords[2]); // Put it in the correct spot,
			board.gameHand.Remove(card); // And remove the card from the player's hand
			return true; // yay
		}
		return false; // Something bad happened
	}
	public void FreeCardGet() // Called when player requests his/her free card of the turn
	{
		if (!board.playerTookFreeCard) // If player has not yet retrieved a free card
		{
			Card freeCard = Instantiate(cardObj).GetComponent<Card>(); // Create a new card (!!! Will be replaced when premade cards are stored in bulk somewhere)
			freeCard.SetStats(0, 2, 0, 2); // Set the cards stats (Again will be done automatically later)
			freeCard.SetPos(-50, -50, -50); // Effectively hides the card from player view until we implement a hand !!!
			board.gameHand.Add(freeCard); // Add this card to player deck
			board.playerTookFreeCard = true; // Player now has his/her free card
		}
	}
	public void GameEnd(bool playerWon) // Called when the game ends
	{

	}
}
