using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Cards
{
	Random, // Will return a card with attack 0-2 and health 1-4 inclusive, NOT a random premade card
	Free, // The free card drawn from the free card pile
	Basic1, // Basic cards have no modifiers, just interesting stat combinations. These also double as starter cards
	Basic2,
	Basic3,
	Basic4,
	Basic5
}
public enum Modifiers
{
	None,
	Free // Costs nothing to play
}
public enum Effects
{
	None
}
public class PremadeCards : MonoBehaviour
{
	[SerializeField] GameObject cardObjDouble; // A reference to the prefab for the double sided card
	[SerializeField] GameObject cardObjSingle; // The single sided one

	public Card GetCard(Cards template, bool isDouble) // Creates a card. Single sided cards will only use the front stats
	{
		// Default values of cards set first here
		int[] stats = new int[4] {0, 0, 0, 0}; // Front attack and health, then back
		Modifiers[] modifiers = new Modifiers[2] { Modifiers.None, Modifiers.None}; // Front modifier, then back
		Effects effect = Effects.None; // Preformed when card is placed; Normally paired with 0/0 statline
		// Then adjust values based on card requested:
		switch (template) // These values are subject to change, but they'll do for now
		{
			case Cards.Random:
				stats = new int[4] { Random.Range(0, 3), Random.Range(0, 3), Random.Range(1, 5), Random.Range(1, 5) };
				break;
			case Cards.Free:
				stats = new int[4] { 0, 1, 0, 1 };
				modifiers = new Modifiers[2] { Modifiers.Free, Modifiers.Free };
				break;
			case Cards.Basic1:
				stats = new int[4] { 1, 3, 2, 1 };
				break;
			case Cards.Basic2:
				stats = new int[4] { 2, 2, 1, 2 };
				break;
			case Cards.Basic3:
				stats = new int[4] { 1, 1, 1, 2 };
				break;
			case Cards.Basic4:
				stats = new int[4] { 0, 1, 1, 1 };
				break;
			case Cards.Basic5:
				stats = new int[4] { 2, 1, 0, 4 };
				break;
			default: // Shouldn't happen
				break;
		}
		Card newCard = Instantiate(isDouble ? cardObjDouble : cardObjSingle).GetComponent<Card>(); // Create a new card with blank values
		newCard.SetStats(stats[0], stats[1], stats[2], stats[3]); // Set the cards stats
		// Modifiers (!!! Not implemented !!!)
		// Effects 
		newCard.SetPos(-50, -50, -50); // Effectively hides the card from player view
		return newCard;
	}
}
