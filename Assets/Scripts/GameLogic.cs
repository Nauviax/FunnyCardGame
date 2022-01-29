using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
	PremadeCards premade; // For creating new cards with non-random values (Optionaly not random anyway)
	Hand uiHand; // Holds a reference to the Hand object that displays the hand

	float[] initialCoords = new float[3] { 0, 0, 0 }; // This and next two lines used to position cards
	float verticalSpacing = 10;
	float horisontalSpacing = 7;

	void Start()
    {
		board = new Board(); // This variable will be referenced *extensively*
		premade = GetComponent<PremadeCards>(); // Grab this for getting cards later
		uiHand = gameObject.GetComponent<Hand>(); // Wee woo wee woo lachlan added a line here :o
		board.upcomingRowFront = new Card[4]; // Perhaps a little messy, but it works
		board.opponentRowFront = new Card[4];
		board.upcomingRowBack = new Card[4];
		board.opponentRowBack = new Card[4];
		board.playerRow = new Card[4];
		board.ownedCards = new List<Card>(); // Will hold all cards the player owns. Starting cards should be set here !!!

		// Testing lines below here
		/*for (int ii = 0; ii < 10; ii++) // Creates random cards and puts them into your deck
		{
			board.ownedCards.Add(premade.GetCard(Cards.Random, true)); // Generates double cards with random values, NOT a random premade card
		}*/
		board.ownedCards.Add(premade.GetCard(Cards.Basic1, true)); // A simple starting deck. Each win will let the player add to this deck, so the small size should be fine
		board.ownedCards.Add(premade.GetCard(Cards.Basic2, true));
		board.ownedCards.Add(premade.GetCard(Cards.Basic3, true));
		board.ownedCards.Add(premade.GetCard(Cards.Basic4, true));
		board.ownedCards.Add(premade.GetCard(Cards.Basic5, true));
		BeginGame();
    }
	public void GenerateAICards() // Called whenever the AI should create cards to play. These cards are currently completely random !!!
	{
		int cardNum = Random.Range(1, 3);
		for (int ii = 0; ii < cardNum; ii++) // Creates 1-2 random cards and adds them to upcoming
		{
			PlaceCard(premade.GetCard(Cards.Random, true), Random.Range(0, 4), true); // Places on the enemy side
		}
	}
	public void BeginGame() // Set initial values, deal starting hand, and generate AI cards for first round
	{
		// Set initial values
		board.playerDust = 50; // !!! Set much lower when finished debugging
		board.opponentDust = 50;
		board.playerTookFreeCard = false; // Player may take a free card on his/her first turn

		board.gameHand = new List<Card>(); // Create empty hand
		board.gameDeck = new List<Card>(board.ownedCards); // Set the deck to all owned cards
		board.gameDeck = board.gameDeck.OrderBy(a => Random.Range(0f, 100f)).ToList(); // Randomly shuffles the deck, using random floats to reduce ties

		// Deal starting hand
		for (int ii = 0; ii < 3; ii++) // 3 starting cards, for now
		{
			if (board.gameDeck.Count > 0) // If there are cards still in the deck,
			{
				addCardToHand(board.gameDeck[0]); // Get the next card in the deck and add it to player hand
				board.gameDeck.RemoveAt(0); // Remove this card from the deck
			}
		}
		FreeCardGet(false); // Also deal player two free cards, possibly allowing for a "Take no free cards" challenge, but also for convenience/qol
		FreeCardGet(false); // These cards do not count towards the "One free card per turn limit," so the player is free to draw a third on his/her first turn

	   // Generate AI cards for first round
	   GenerateAICards(); // Now seperate as is called in more than one spot
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
					board.opponentRowFront[ii].SetPos(initialCoords[0] + ii * horisontalSpacing, initialCoords[1] + verticalSpacing, initialCoords[2]); // Move it to the correct location
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
						board.playerDust += board.playerRow[ii].DustValue; // Player gets a dust refund based on the cards value
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
						board.playerDust += board.playerRow[ii].DustValue; // Player gets a dust refund based on the cards value
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
		GenerateAICards();

		// Set up for next turn
		board.playerTookFreeCard = false; // Player may retrieve a new free card

		// Draw card from deck
		if (board.gameDeck.Count > 0) // If there are cards still in the deck,
		{
			addCardToHand(board.gameDeck[0]); // Get the next card in the deck and add it to player hand
			board.gameDeck.RemoveAt(0); // Remove this card from the deck
		}

	}
	public bool PlaceCard(Card card, int location, bool isEnemySide = false) // Will place the given card at a certian location on the board. Returns true if sucsessful, false if failed
	{
		if (isEnemySide) // Optional input determines if card should be placed on enemy side. Enemies require less checks if they can place a card, like no cost needed
		{
			if (board.upcomingRowFront[location] != null) // If location is taken, fail (!!! Uses front by default !!!)
			{
				return false;
			}
			board.upcomingRowFront[location] = card; // Place the card,
			card.SetPos(initialCoords[0] + location * horisontalSpacing, initialCoords[1] + verticalSpacing * 2, initialCoords[2]); // Put it in the correct spot
			return true; // yay
		}
		else
		{
			if (board.playerRow[location] != null) // If location is taken, fail
			{
				return false;
			}
			if (card.DustCost > board.playerDust) // If I can't afford to place this card, and will die because of it, fail
			{
				return false;
			}
			if (board.gameHand.Contains(card)) // If I have the card in my hand, (I'ma idiot proof the fuck out of this)
			{
				board.playerDust -= card.DustCost; // Subtract the COST of the card, not the value,
				board.playerRow[location] = card; // Place the card,
				card.SetPos(initialCoords[0] + location * horisontalSpacing, initialCoords[1], initialCoords[2]); // Put it in the correct spot,
				removeCardFromHand(card); // And remove the card from the player's hand
				return true; // yay
			}
		}
		return false; // Something bad happened
	}
	public void FreeCardGet(bool oneOnly = true) // Called when player requests his/her free card of the turn. If oneOnly is false, then this card is not counted as the players "one per turn" card
	{
		if (!board.playerTookFreeCard) // If player has not yet retrieved a free card
		{
			addCardToHand(premade.GetCard(Cards.Free, true)); // Add new free card to player deck
			board.playerTookFreeCard = oneOnly; // Player now has his/her free card, unless oneOnly is manually set to false (For giving player 2 free cards at the begining of the game)
		}
	}
	public void GameEnd(bool playerWon) // Called when the game ends
	{

	}
	// Lachlan's hooks, use where appropriate
	public void addCardToHand(Card card) {
		board.gameHand.Add(card);
		uiHand.addCard(card);
	}
	public void removeCardFromHand(Card card) {
		board.gameHand.Remove(card);
		uiHand.removeCard(card);
	}
}
