using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro; // For editing the text

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

	[SerializeField] GameObject beginGameCube; // These references are for moving ingame objects when rounds start/end
	[SerializeField] GameObject endTurnCube;
	[SerializeField] GameObject freeCardCube;
	[SerializeField] GameObject boardPositions;
	[SerializeField] GameObject mirror;
	[SerializeField] GameObject dustCounter; // Not moved, but text changed
	TextMeshPro dustText;

	[SerializeField] GameObject notEnoughDustPrefab; // lol ( Created when there is not enough dust to place a card )
	[SerializeField] GameObject damageFriendly; // Spawns when player deals damage directly
	[SerializeField] GameObject damageBad; // Spawns when player TAKES damage directly

	float[] initialCoords = new float[3] { 0, 3, 55 }; // This and next two lines used to position cards
	float verticalSpacing = 10;
	float horisontalSpacing = 7;

	[SerializeField] int playerStartingDust = 10;
	[SerializeField] int opponentStartingDust = 10;


	void Start()
    {
		board = new Board(); // This variable will be referenced *extensively*
		premade = GetComponent<PremadeCards>(); // Grab this for getting cards later
		uiHand = gameObject.GetComponent<Hand>(); // Wee woo wee woo lachlan added a line here :o
		dustText = dustCounter.GetComponent<TextMeshPro>(); // For editing the text
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
	public void GenerateRandomPlayerCard() // Adds a random new PREMADE card to owned cards (Victory reward)
	{
		Cards[] listOfCards = (Cards[])Cards.GetValues(typeof(Cards)); // Returns an array of all possible premade card enums
		Card randomCard = premade.GetCard(listOfCards[Random.Range(0, listOfCards.Length)], true); // Gets a new card based on a random enum from the list
		board.ownedCards.Add(randomCard); // Adds the card to the players current hand
	}
	public void GenerateAICards() // Called whenever the AI should create cards to play. These cards are currently completely random !!!
	{
		Card newEnemyCard = premade.GetCard(Cards.Random, false); // Get new enemy card
		newEnemyCard.HealthBack = 0; // Wipe back stats
		newEnemyCard.DamageBack = 0;
		bool sucsess = PlaceCard(newEnemyCard, Random.Range(0, 4), true, true); // Places on the enemy front side
		if (!sucsess) // If card wasn't placed
		{
			newEnemyCard.Destroy(); // Cleanup
		}

		newEnemyCard = premade.GetCard(Cards.Random, false); // Get new new enemy card
		newEnemyCard.HealthFront = 0; // Wipe front stats
		newEnemyCard.DamageFront = 0;
		sucsess = PlaceCard(newEnemyCard, Random.Range(0, 4), true, false); // Places on the enemy back side
		if (!sucsess) // If card wasn't placed
		{
			newEnemyCard.Destroy(); // Cleanup
		}
	}
	public void AnimateShow(GameObject thing, bool show) // Just makes the object move away
	{
		int yDist;
		if (show) yDist = 100; // Go up
		else yDist = -100; // Go down
		thing.transform.position = new Vector3(thing.transform.position[0], thing.transform.position[1] + yDist, thing.transform.position[2]); // Teleport to position (For now)
	}
	public void BeginGame() // Set initial values, deal starting hand, and generate AI cards for first round (Called from Hand.cs)
	{
		// Set initial values, show objects
		board.playerDust = playerStartingDust; // Can set in inspector
		board.opponentDust = opponentStartingDust;
		dustText.SetText("Player dust: " + board.playerDust.ToString() + ", Opponent dust: " + board.opponentDust.ToString());
		board.playerTookFreeCard = false; // Player may take a free card on his/her first turn

		board.upcomingRowFront = new Card[4]; // Perhaps a little messy, but it works
		board.opponentRowFront = new Card[4];
		board.upcomingRowBack = new Card[4];
		board.opponentRowBack = new Card[4];
		board.playerRow = new Card[4];

		board.gameHand = new List<Card>(); // Create empty hand
		board.gameDeck = new List<Card>(); // Set the deck to all owned cards
		foreach (Card card in board.ownedCards) // Need to pass in a copy of owned card, as it is lost when it dies/game ends
		{
			Card newCard = premade.GetCard(Cards.None, true); // Create a new blank card
			newCard.Clone(card); // Clone it gooood
			board.gameDeck.Add(newCard); // Add the clone to this game's deck
		}
		board.gameDeck = board.gameDeck.OrderBy(a => Random.Range(0f, 100f)).ToList(); // Randomly shuffles the deck, using random floats to reduce ties

		beginGameCube.transform.position = new Vector3(-20, -80, 100); // Hidden / under the table (So also moves from being large and outside) (Ready for y += 100)
		beginGameCube.transform.localScale = new Vector3(5, 5, 5); // Permanently make smol (This runs when starting second game, but ehhhh)
		AnimateShow(endTurnCube, true);
		if (freeCardCube.transform.position[1] < 0) AnimateShow(freeCardCube, true); // Only show free card cube if it is hidden
		AnimateShow(boardPositions, true);
		AnimateShow(mirror, true);

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
		if (freeCardCube.transform.position[1] < 0) AnimateShow(freeCardCube, true); // Only show free card cube if it is hidden
		dustText.SetText("Player dust: " + board.playerDust.ToString() + ", Opponent dust: " + board.opponentDust.ToString()); // Update dust count

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
			// Syphoning cards
			if (board.playerRow[ii].CardModifiers[0] == Modifiers.Syphoning) // If front has modifier
			{
				board.playerRow[ii].HealthBack--; // Drain 1 hp from other side,
				if (board.playerRow[ii].HealthBack <= 0) // If card is now dead,
				{
					board.playerDust += board.playerRow[ii].DustValue; // Player gets a dust refund
					board.playerRow[ii].Destroy(); // Destroy that card
					board.playerRow[ii] = null; // And remove it from the board
				}
				else board.playerRow[ii].HealthFront++; // Gain the drained hp
			}
			else if (board.playerRow[ii].CardModifiers[1] == Modifiers.Syphoning) // If back
			{
				board.playerRow[ii].HealthFront--; // Drain 1 hp from other side,
				if (board.playerRow[ii].HealthFront <= 0) // If card is now dead,
				{
					board.playerDust += board.playerRow[ii].DustValue; // Player gets a dust refund
					board.playerRow[ii].Destroy(); // Destroy that card
					board.playerRow[ii] = null; // And remove it from the board
				}
				else board.playerRow[ii].HealthBack++; // Gain the drained hp
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
					if (ii == 3) // If at far RIGHT of board,
					{
						continue; // Nothing I can do
					}
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
				if (ii == 3) // If at far RIGHT of board,
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
					if (ii == 0) // If at far LEFT of board,
					{
						continue; // Nothing I can do
					}
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
		Card[] guarding = new Card[2] { null, null }; // If set, all enemy cards will attack this card (One for each side)
		
		// Check for guarding cards
		for (int ii = 0; ii < 4; ii++)
		{
			if (board.playerRow[ii] == null) // Empty spots can't guard
			{
				continue;
			}
			if (board.playerRow[ii].CardModifiers[0] == Modifiers.Guarding)
			{
				guarding[0] = board.playerRow[ii];
			}
			if (board.playerRow[ii].CardModifiers[1] == Modifiers.Guarding)
			{
				guarding[1] = board.playerRow[ii];
			}
		}
		for (int ii = 0; ii < 4; ii++)
		{
			// Move cards down
			if (board.upcomingRowFront[ii] != null) // If a card wants to move down,
			{
				if (board.opponentRowFront[ii] == null) // And it is not being blocked,
				{
					board.opponentRowFront[ii] = board.upcomingRowFront[ii]; // Move that card forwards
					board.upcomingRowFront[ii] = null; // And remove it from upcoming
					board.opponentRowFront[ii].SetPos(initialCoords[0] + ii * horisontalSpacing, initialCoords[1] + verticalSpacing, initialCoords[2] - 0.5f); // Move it to the correct location
				}
			}
			if (board.upcomingRowBack[ii] != null) // the same, but for back cards now
			{
				if (board.opponentRowBack[ii] == null) // And it is not being blocked,
				{
					board.opponentRowBack[ii] = board.upcomingRowBack[ii]; // Move that card forwards
					board.upcomingRowBack[ii] = null; // And remove it from upcoming
					board.opponentRowBack[ii].SetPos(initialCoords[0] + ii * horisontalSpacing, initialCoords[1] + verticalSpacing, initialCoords[2] + 0.5f); // Move it to the correct location
				}
			}

			// Front cards attack
			if (board.opponentRowFront[ii] != null) // Empty spots do not attack
			{
				if (guarding[0] == null) // Attack normally
				{
					removeCards = DamageCard(board.opponentRowFront[ii], board.playerRow[ii], true, false); // I've assumed opponent cards will not have modifiers, so no pronged checks
				}
				else // Attack specific card
				{
					removeCards = DamageCard(board.opponentRowFront[ii], guarding[0], true, false);
				}
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
				if (guarding[1] == null) // Attack normally
				{
					removeCards = DamageCard(board.opponentRowBack[ii], board.playerRow[ii], false, false); // I've assumed opponent cards will not have modifiers, so no pronged checks
				}
				else // Attack specific card
				{
					removeCards = DamageCard(board.opponentRowBack[ii], guarding[1], false, false);
				}
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
	public bool PlaceCard(Card card, int location, bool isEnemySide = false, bool isEnemyFront = true) // Will place the given card at a certian location on the board. Returns true if sucsessful, false if failed
	{
		if (isEnemySide) // Optional input determines if card should be placed on enemy side. Enemies require less checks if they can place a card, like no cost needed
		{
			if (isEnemyFront) // Placing on the front side?
			{
				if (board.upcomingRowFront[location] != null) // If location is taken, fail (!!! Uses front by default !!!)
				{
					return false;
				}
				board.upcomingRowFront[location] = card; // Place the card,
				card.SetPos(initialCoords[0] + location * horisontalSpacing, initialCoords[1] + verticalSpacing * 2, initialCoords[2] - 0.5f); // Put it in the correct spot
			}
			else // Placing on the back
			{
				if (board.upcomingRowBack[location] != null) // If location is taken, fail (!!! Uses front by default !!!)
				{
					return false;
				}
				board.upcomingRowBack[location] = card; // Place the card,
				card.SetPos(initialCoords[0] + location * horisontalSpacing, initialCoords[1] + verticalSpacing * 2, initialCoords[2] + 0.5f); // This one is PLUS 0.5, not minus
			}
			card.SetRotation(0, 0, 0); //Face it the right way
			card.GetComponentsInChildren<Collider>()[0].enabled = true; //re enable colliders for click detection
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
				GameObject indicator = Instantiate(notEnoughDustPrefab); // lol
				indicator.transform.position = new Vector3(initialCoords[0] + location * horisontalSpacing, initialCoords[1], initialCoords[2]); // Put it where user tried to place
				return false;
			}
			if (board.gameHand.Contains(card)) // If I have the card in my hand, (I'ma idiot proof the fuck out of this)
			{
				if (card.CardModifiers[0] == Modifiers.Musical | card.CardModifiers[1] == Modifiers.Musical) // If this card is musical,
				{
					for (int ii = 0; ii < 4; ii++) // For each card
					{
						if (ii == location) continue; // Don't buff self
						if (board.playerRow[ii] == null) continue; // Can't buff empty
						if (card.CardModifiers[0] == Modifiers.Musical) board.playerRow[ii].DamageFront++; // Buff the front
						else board.playerRow[ii].DamageBack++; // Buff the back
					}
				}
				foreach (MeshRenderer mesh in card.transform.GetComponentsInChildren<MeshRenderer>()) {
					mesh.enabled = true;
				}
				card.GetComponentsInChildren<Collider>()[0].enabled = true; //re enable colliders for click detection
				card.transform.parent = null;
				board.playerDust -= card.DustCost; // Subtract the COST of the card, not the value,
				board.playerRow[location] = card; // Place the card,
				card.SetPos(initialCoords[0] + location * horisontalSpacing, initialCoords[1], initialCoords[2]); // Put it in the correct spot,
				card.SetRotation(0, 0, 0); // Face it the right way,
				RemoveCardFromHand(card); // And remove the card from the player's hand
				dustText.SetText("Player dust: " + board.playerDust.ToString() + ", Opponent dust: " + board.opponentDust.ToString()); // Update dust text
				return true; // yay
			}
		}
		return false; // Something bad happened
	}
	public bool FreeCardGet(bool oneOnly = true) // Called when player requests his/her free card of the turn. If oneOnly is false, then this card is not counted as the players "one per turn" card
	{
		if (!board.playerTookFreeCard) // If player has not yet retrieved a free card
		{
			AddCardToHand(premade.GetCard(Cards.Free, true)); // Add new free card to player deck
			board.playerTookFreeCard = oneOnly; // Player now has his/her free card, unless oneOnly is manually set to false (For giving player 2 free cards at the begining of the game)
			if (freeCardCube.transform.position[1] > 0) AnimateShow(freeCardCube, false); // Only hide free card cube if it is shown (Can already be hidden if player used it)
			return true; // Player got a free card
		}
		else return false; // Player did not get a free card
	}
	public bool[] DamageCard(Card attacker, Card victim, bool isFront, bool isPlayerAttacking) // Preforms attacking logic between two cards, returns true if card died (Victim can be null)
	{
		bool[] removeCards = new bool[2]; // First index is for the victim card, second is for attacker as well
		if (victim == null || attacker.CardModifiers[0] == Modifiers.Flying) // If the card is unopposed, or is flying
		{
			int damageDealt; // Damage delt by this card
			if (isPlayerAttacking) // Figure out who to damage,
			{
				if (isFront) // And how much by, (What side of card stats)
				{
					board.opponentDust -= attacker.DamageFront; // Deal damage directly
					damageDealt = attacker.DamageFront;
				}
				else
				{
					board.opponentDust -= attacker.DamageBack;
					damageDealt = attacker.DamageBack;
				}
				for (int ii = 0; ii < damageDealt; ii++)
				{
					GameObject indicator = Instantiate(damageFriendly); // Drop a friendly point
					indicator.transform.position = new Vector3(attacker.transform.position[0], attacker.transform.position[1] + horisontalSpacing, attacker.transform.position[2]); // Put it where the player card attacked
				}

			}
			else if (isFront) // And how much by,
			{
				board.playerDust -= attacker.DamageFront; // Deal damage directly
				damageDealt = attacker.DamageFront;
			}
			else
			{
				board.playerDust -= attacker.DamageBack;
				damageDealt = attacker.DamageBack;
			}
			for (int ii = 0; ii < damageDealt; ii++)
			{
				GameObject indicator = Instantiate(damageBad); // Drop a friendly point
				indicator.transform.position = new Vector3(attacker.transform.position[0], attacker.transform.position[1] - horisontalSpacing, attacker.transform.position[2]); // Put it where the enemy card attacked
			}
			return new bool[2] {false, false}; // No card died
		}
		else // Attacking a card
		{
			if (isFront) // Determines which side to use for stats
			{
				if (attacker.DamageFront != 0) // If attacker card can actually attack,
				{
					if (attacker.CardModifiers[0] == Modifiers.Venomous) // Check for venomous modifier (This also means it won't be, say, Vampiric)
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
							if (attacker.CardModifiers[0] == Modifiers.Vampiric) // This card directly killed another, vampires absorb hp after kill
							{
								attacker.HealthFront += 2; // Gain two health
							}
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
							if (attacker.CardModifiers[1] == Modifiers.Vampiric) // This card directly killed another, vampires absorb hp after kill
							{
								attacker.HealthBack += 2; // Gain two health
							}
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
		AnimateShow(beginGameCube, true); // So the player can start a new round
		AnimateShow(endTurnCube, false);
		if (freeCardCube.transform.position[1] > 0) AnimateShow(freeCardCube, false); // Only hide free card cube if it is shown (Can already be hidden if player used it)
		AnimateShow(boardPositions, false);
		AnimateShow(mirror, false);

		if (playerWon)
		{
			dustText.SetText("You won! A new card has been added to your deck");
			GenerateRandomPlayerCard(); // Adds a premade card to board.ownedCards
		}
		else
		{
			dustText.SetText("You ran out of dust.");
		}

		// Card cleanup
		foreach (Card card in board.gameDeck)
		{
			if (card == null) continue;
			card.Destroy(); // Remove card
		}
		board.gameDeck.Clear(); // Wipe deck
		foreach (Card card in board.gameHand)
		{
			if (card == null) continue;
			card.Destroy(); // Remove card
		}
		board.gameHand.Clear(); // Wipe hand

		uiHand.clearHand(); // Reset hand cards

		foreach (Card card in board.upcomingRowFront)
		{
			if (card == null) continue;
			card.Destroy(); // Remove card
		}
		board.upcomingRowFront = null; // Wipe row
		foreach (Card card in board.upcomingRowBack)
		{
			if (card == null) continue;
			card.Destroy(); // Remove card
		}
		board.upcomingRowBack = null; // Wipe row
		foreach (Card card in board.opponentRowFront)
		{
			if (card == null) continue;
			card.Destroy(); // Remove card
		}
		board.opponentRowFront = null; // Wipe row
		foreach (Card card in board.opponentRowBack)
		{
			if (card == null) continue;
			card.Destroy(); // Remove card
		}
		board.opponentRowBack = null; // Wipe row
		foreach (Card card in board.playerRow)
		{
			if (card == null) continue;
			card.Destroy(); // Remove card
		}
		board.playerRow = null; // Wipe row
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
