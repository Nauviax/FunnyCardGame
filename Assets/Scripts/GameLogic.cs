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

	float[] initialCoords = new float[3] { 0, 0, 50 }; // This and next two lines used to position cards
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
    }
	public void GenerateAICards() // Called whenever the AI should create cards to play. These cards are currently completely random !!!
	{
		int cardNum = Random.Range(1, 3);
		for (int ii = 0; ii < cardNum; ii++) // Creates 1-2 random cards and adds them to upcoming
		{
			PlaceCard(premade.GetCard(Cards.Random, true), Random.Range(0, 4), true); // Places on the enemy side
		}
	}
	public void BeginGame() // Set initial values, deal starting hand, and generate AI cards for first round (Called from Hand.cs)
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
				AddCardToHand(board.gameDeck[0]); // Get the next card in the deck and add it to player hand
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
		HandlePlayerAttacks(); // So clean looking!

		// Determine if game has been won
		if (board.opponentDust < 0) // Opponent must have LESS than 0 dust to end the game
		{
			GameEnd(true); // Player has won
			return; // Stop game
		}

		// Preform midturn actions, such as moving cards
		HandleMidturnEvents(); // So very clean

		// Move cards down and preform enemy attacks
		HandleOpponentAttacks(); // Nice and tidy

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
			AddCardToHand(board.gameDeck[0]); // Get the next card in the deck and add it to player hand
			board.gameDeck.RemoveAt(0); // Remove this card from the deck
		}
	}
	void HandlePlayerAttacks() // Simply moves code out of the EndTurn() method, to make it seem cleaner
	{
		bool[] removeCards = new bool[2]; // First index means remove victim, second means remove the attacking card
		for (int ii = 0; ii < 4; ii++) // 4 cards in player row
		{
			if (board.playerRow[ii] == null) // Empty spots do not attack
			{
				continue; // Next column
			}
			// Attack with front stats
			if (board.playerRow[ii].CardModifiers[0] == Modifiers.Pronged) // Check for pronged modifier, which allows cards to attack left/right instead of directly ahead
			{
				if (ii == 0) // On far left side
				{
					removeCards = DamageCard(board.playerRow[ii], board.opponentRowFront[ii + 1], true, true);
					if (removeCards[0]) // If attacked card died,
					{
						board.opponentRowFront[ii + 1] = null;
					}
					if (removeCards[1]) // If the attacking card died,
					{
						board.playerRow[ii] = null;
						continue; // This card is now dead
					}
				}
				else if (ii == 3) // On far right side
				{
					removeCards = DamageCard(board.playerRow[ii], board.opponentRowFront[ii - 1], true, true);
					if (removeCards[0]) // If attacked card died,
					{
						board.opponentRowFront[ii - 1] = null;
					}
					if (removeCards[1]) // If the attacking card died,
					{
						board.playerRow[ii] = null;
						continue; // This card is now dead
					}
				}
				else // Will actually attact twice
				{
					removeCards = DamageCard(board.playerRow[ii], board.opponentRowFront[ii + 1], true, true);
					if (removeCards[0]) // If attacked card died,
					{
						board.opponentRowFront[ii + 1] = null;
					}
					if (removeCards[1]) // If the attacking card died,
					{
						board.playerRow[ii] = null;
						continue; // This card is now dead
					}
					removeCards = DamageCard(board.playerRow[ii], board.opponentRowFront[ii - 1], true, true);
					if (removeCards[0]) // If attacked card died,
					{
						board.opponentRowFront[ii - 1] = null;
					}
					if (removeCards[1]) // If the attacking card died,
					{
						board.playerRow[ii] = null;
						continue; // This card is now dead
					}
				}
			}
			else // Attack normally
			{
				removeCards = DamageCard(board.playerRow[ii], board.opponentRowFront[ii], true, true); // Fancy new attacking method, attack w front
				if (removeCards[0]) // If attacked card died,
				{
					board.opponentRowFront[ii] = null;
				}
				if (removeCards[1]) // If the attacking card died,
				{
					board.playerRow[ii] = null;
					continue; // This card is now dead
				}
			}
			// Attack with back stats
			if (board.playerRow[ii].CardModifiers[1] == Modifiers.Pronged) // Check for pronged modifier
			{
				if (ii == 0) // On far left side
				{
					removeCards = DamageCard(board.playerRow[ii], board.opponentRowBack[ii + 1], false, true);
					if (removeCards[0]) // If attacked card died,
					{
						board.opponentRowBack[ii + 1] = null;
					}
					if (removeCards[1]) // If the attacking card died,
					{
						board.playerRow[ii] = null;
						continue; // This card is now dead
					}
				}
				else if (ii == 3) // On far right side
				{
					removeCards = DamageCard(board.playerRow[ii], board.opponentRowBack[ii - 1], false, true);
					if (removeCards[0]) // If attacked card died,
					{
						board.opponentRowBack[ii - 1] = null;
					}
					if (removeCards[1]) // If the attacking card died,
					{
						board.playerRow[ii] = null;
						continue; // This card is now dead
					}
				}
				else // Will actually attact twice
				{
					removeCards = DamageCard(board.playerRow[ii], board.opponentRowBack[ii + 1], false, true);
					if (removeCards[0]) // If attacked card died,
					{
						board.opponentRowBack[ii + 1] = null;
					}
					if (removeCards[1]) // If the attacking card died,
					{
						board.playerRow[ii] = null;
						continue; // This card is now dead
					}
					removeCards = DamageCard(board.playerRow[ii], board.opponentRowBack[ii - 1], false, true);
					if (removeCards[0]) // If attacked card died,
					{
						board.opponentRowBack[ii - 1] = null;
					}
					if (removeCards[1]) // If the attacking card died,
					{
						board.playerRow[ii] = null;
						continue; // This card is now dead
					}
				}
			}
			else
			{
				DamageCard(board.playerRow[ii], board.opponentRowBack[ii], false, true); // Attack w back
			}
		}
	}
	void HandleMidturnEvents() // Ditto, as above
	{

		for (int ii = 0; ii < 4; ii++)
		{
			if (board.playerRow[ii] == null) // Skip empty spots
			{
				continue;
			}
			// Moving cards (There is SO MUCH repeated code here, but slightly different so I can't easily make it nicer)
			if (board.playerRow[ii].CardModifiers[0] == Modifiers.MovingL || board.playerRow[ii].CardModifiers[1] == Modifiers.MovingL) // If a card wants to move left
			{
				if (ii == 0) // If at far left of board,
				{
					board.playerRow[ii].Turn(); // Turn around
					if (board.playerRow[ii + 1] == null) // If the card to the right is empty,
					{
						board.playerRow[ii + 1] = board.playerRow[ii]; // Move here
						board.playerRow[ii + 1].SetPos(initialCoords[0] + (ii + 1) * horisontalSpacing, initialCoords[1], initialCoords[2]); // Update pos
						board.playerRow[ii] = null; // I'm not here anymore
					} // Else could be turn again, but no point as we know it's blocked
					continue; // Next card (ignore next if)
				}
				if (board.playerRow[ii - 1] == null) // If the card to the left is empty,
				{
					board.playerRow[ii - 1] = board.playerRow[ii]; // Move here
					board.playerRow[ii - 1].SetPos(initialCoords[0] + (ii - 1) * horisontalSpacing, initialCoords[1], initialCoords[2]); // Update pos
					board.playerRow[ii] = null; // I'm not here anymore
				}
				else // This is the same code from inside the "If (ii == 0)" from above
				{
					board.playerRow[ii].Turn(); // Turn around
					if (board.playerRow[ii + 1] == null) // If the card to the right is empty,
					{
						board.playerRow[ii + 1] = board.playerRow[ii]; // Move here
						board.playerRow[ii + 1].SetPos(initialCoords[0] + (ii + 1) * horisontalSpacing, initialCoords[1], initialCoords[2]); // Update pos
						board.playerRow[ii] = null; // I'm not here anymore
					} // Else could be turn again, but no point as we know it's blocked
				}
			}
			if (board.playerRow[ii].CardModifiers[0] == Modifiers.MovingR || board.playerRow[ii].CardModifiers[1] == Modifiers.MovingR) // Ditto for right
			{
				if (ii == 4) // If at far RIGHT of board,
				{
					board.playerRow[ii].Turn(); // Turn around
					if (board.playerRow[ii - 1] == null) // If the card to the left is empty,
					{
						board.playerRow[ii - 1] = board.playerRow[ii]; // Move here
						board.playerRow[ii - 1].SetPos(initialCoords[0] + (ii - 1) * horisontalSpacing, initialCoords[1], initialCoords[2]); // Update pos
						board.playerRow[ii] = null; // I'm not here anymore
					} // Else could be turn again, but no point as we know it's blocked
					continue; // Next card (ignore next if)
				}
				if (board.playerRow[ii + 1] == null) // If the card to the right is empty,
				{
					board.playerRow[ii + 1] = board.playerRow[ii]; // Move here
					board.playerRow[ii + 1].SetPos(initialCoords[0] + (ii + 1) * horisontalSpacing, initialCoords[1], initialCoords[2]); // Update pos
					board.playerRow[ii] = null; // I'm not here anymore
				}
				else // This is the same code from inside the "If (ii == 4)" from above
				{
					board.playerRow[ii].Turn(); // Turn around
					if (board.playerRow[ii - 1] == null) // If the card to the left is empty,
					{
						board.playerRow[ii - 1] = board.playerRow[ii]; // Move here
						board.playerRow[ii - 1].SetPos(initialCoords[0] + (ii - 1) * horisontalSpacing, initialCoords[1], initialCoords[2]); // Update pos
						board.playerRow[ii] = null; // I'm not here anymore
					} // Else could be turn again, but no point as we know it's blocked
				}
			}
		}
	}
	void HandleOpponentAttacks() // Ditto, as above
	{
		bool[] removeCards = new bool[2]; // First index means remove victim, second means remove the attacking card
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
				removeCards = DamageCard(board.opponentRowFront[ii], board.playerRow[ii], true, false); // I've assumed opponent cards will not have modifiers, so no pronged checks
				if (removeCards[0]) // If attacked card died,
				{
					board.playerRow[ii] = null;
				}
				if (removeCards[1]) // If the attacking card died,
				{
					board.opponentRowFront[ii] = null;
				}
			}
			// Back cards attack
			if (board.opponentRowBack[ii] != null) // Empty spots do not attack
			{
				removeCards = DamageCard(board.opponentRowBack[ii], board.playerRow[ii], false, false);
				if (removeCards[0]) // If attacked card died,
				{
					board.playerRow[ii] = null;
				}
				if (removeCards[1]) // If the attacking card died,
				{
					board.opponentRowBack[ii] = null;
				}
			}
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
			card.SetRotation(0, 0, 0); //Face it the right way
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
				foreach (MeshRenderer mesh in card.transform.GetComponentsInChildren<MeshRenderer>()) {
					mesh.enabled = true;
				}
				card.transform.parent = null;
				board.playerDust -= card.DustCost; // Subtract the COST of the card, not the value,
				board.playerRow[location] = card; // Place the card,
				card.SetPos(initialCoords[0] + location * horisontalSpacing, initialCoords[1], initialCoords[2]); // Put it in the correct spot,
				card.SetRotation(0, 0, 0); // Face it the right way,
				RemoveCardFromHand(card); // And remove the card from the player's hand
				return true; // yay
			}
		}
		return false; // Something bad happened
	}
	public void FreeCardGet(bool oneOnly = true) // Called when player requests his/her free card of the turn. If oneOnly is false, then this card is not counted as the players "one per turn" card
	{
		if (!board.playerTookFreeCard) // If player has not yet retrieved a free card
		{
			AddCardToHand(premade.GetCard(Cards.Free, true)); // Add new free card to player deck
			board.playerTookFreeCard = oneOnly; // Player now has his/her free card, unless oneOnly is manually set to false (For giving player 2 free cards at the begining of the game)
		}
	}
	public bool[] DamageCard(Card attacker, Card victim, bool isFront, bool isPlayerAttacking) // Preforms attacking logic between two cards, returns true if card died (Victim can be null)
	{
		bool[] removeCards = new bool[2]; // First index is for the victim card, second is for attacker as well
		if (victim == null || attacker.CardModifiers[0] == Modifiers.Flying) // If the card is unopposed, or is flying
		{
			if (isPlayerAttacking) // Figure out who to damage,
			{
				if (isFront) // And how much by,
				{
					board.opponentDust -= attacker.DamageFront; // Deal damage directly
				}
				else
				{
					board.opponentDust -= attacker.DamageBack;
				}
			}
			else if (isFront) // And how much by,
			{
				board.playerDust -= attacker.DamageFront; // Deal damage directly
			}
			else
			{
				board.playerDust -= attacker.DamageBack;
			}
			return new bool[2] {false, false}; // No card died
		}
		else // Attacking a card
		{
			if (isFront) // Determines which side to use for stats
			{
				if (attacker.DamageFront != 0) // If attacker card can actually attack,
				{
					if (attacker.CardModifiers[0] == Modifiers.Venomous) // Check for venomous modifier
					{
						victim.Destroy(); // Destroy that card 
						return new bool[2] { true, false }; // And remove it from the board (Must be followed with a line similar to board.playerRow[ii] = null;)
					}
					else
					{
						removeCards = new bool[2] { false, false }; // Assume no cards die
						victim.HealthFront -= attacker.DamageFront - (victim.CardModifiers[0] == Modifiers.Brutish ? 1 : 0); // Deal damage to card, but one less if victim card is brutish
						if (victim.CardModifiers[0] == Modifiers.Thorny) // If attacked card is thorny,
						{
							attacker.HealthFront -= 1; // Deal one damage to attacker
							if (attacker.HealthFront <= 0) // If card is now dead,
							{
								if (isPlayerAttacking) board.playerDust += victim.DustValue; // Player gets a dust refund if they ARE the attacker
								attacker.Destroy(); // Destroy that card
								removeCards[1] = true; // Attacker dies
							}
						}
						if (victim.HealthFront <= 0) // If card is now dead,
						{
							if (!isPlayerAttacking) board.playerDust += victim.DustValue; // Player gets a dust refund based on the cards value, if they are NOT the attacker
							victim.Destroy(); // Destroy that card 
							removeCards[0] = true; // And remove it from the board
						}
						return removeCards; // Set in above two if statements
					}
				}
			}
			else
			{
				if (attacker.DamageBack != 0) // If attacker card can actually attack,
				{
					if (attacker.CardModifiers[1] == Modifiers.Venomous) // Check for venomous modifier
					{
						victim.Destroy(); // Destroy that card 
						return new bool[2] { true, false }; // And remove it from the board (board.playerRow[ii] = null;)
					}
					else
					{
						victim.HealthBack -= attacker.DamageBack - (victim.CardModifiers[1] == Modifiers.Brutish ? 1 : 0); // Deal damage to card, but one less if victim card is brutish
						if (victim.CardModifiers[1] == Modifiers.Thorny) // If attacked card is thorny,
						{
							attacker.HealthBack -= 1; // Deal one damage to attacker
							if (attacker.HealthBack <= 0) // If card is now dead,
							{
								if (isPlayerAttacking) board.playerDust += victim.DustValue; // Player gets a dust refund if they ARE the attacker
								attacker.Destroy(); // Destroy that card
								removeCards[1] = true; // Attacker dies
							}
						}
						if (victim.HealthBack <= 0) // If card is now dead,
						{
							if (!isPlayerAttacking) board.playerDust += victim.DustValue; // Player gets a dust refund based on the cards value, if they are NOT the attacker
							victim.Destroy(); // Destroy that card 
							removeCards[0] = true; // And remove it from the board
						}
						return removeCards; // Set in above two if statements
					}
				}
			}
		}
		return new bool[2] { false, false }; // No card died
	}
	public void GameEnd(bool playerWon) // Called when the game ends
	{

	}
	// Lachlan's hooks, use where appropriate
	public void AddCardToHand(Card card) {
		board.gameHand.Add(card);
		uiHand.addCard(card);
	}
	public void RemoveCardFromHand(Card card) {
		board.gameHand.Remove(card);
		uiHand.removeCard(card);
	}
}
