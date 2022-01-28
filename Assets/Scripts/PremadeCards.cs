using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Cards // 2 cost per attack point, 1 per health
{
	Random, // Will return a card with attack 0-2 and health 1-4 inclusive, NOT a random premade card
	Free, // The free card drawn from the free card pile
	Basic1, // Basic cards have no modifiers, just interesting stat combinations. These also double as starter cards
	Basic2,
	Basic3,
	Basic4,
	Basic5,
}
public enum Modifiers // Cost 5 each
{
	None, // Default modifier
	Free, // Costs nothing to play
	Venomous, // Instantly kill cards upon damaging them
	Flying, // Will always attack opponent directly, over cards
	Dusty, // Will give 3 extra dust on death, on top of normal drops
	MovingL, // Moves to the left after all friendly cards have attacked
	MovingR, // Like above, but to the right. MovingL becomes this card if blocked, and vice versa
	Brutish, // Takes one less damage on this side, possibly taking none
	Pronged, // Attacks to the left and right, rather than straight ahead
	Thorny, // Attackers take one point of damage when attacking this side
	Musical, // Friendly cards gain one attack on this side of the board
	Syphoning, // Gains one health after attacking, but takes one from the other side of this card
	Guarding, // Absorbs all enemy attacks on this side, taking the damage itself instead
	Vampiric, // When this card kills an enemy, gain two health. (This side only)
}
public enum Effects // Normally put on 0/0 cards, Varying cost, not recovered: < x >
{
	None, // Default effect < 0 >
	Buff, // +1 damage and health to all friendly cards on the side this card is placed on. (Default front) < 10 >
	Flip, // Placed on top of any card, will flip that card and consume this card. < 5 >
	Nuke, // ALL cards take 1 damage on this side < 7 >
	Kill, // Placed on top of any card, deals 10 damage to this side of that card < 10 >
	Dust, // Immediatly gives the player 5 dust < 0 >
	Skip, // The enemy will not attack or move for one turn < 12 >
	Find, // Draw any card from your owned cards, including ones that are in hand or dead < 8 >
}
public class PremadeCards : MonoBehaviour
{
	CardAssets cardAssets; // Will store references to all GameObjects for rendering cards

	void Start()
	{
		cardAssets = gameObject.GetComponent<CardAssets>(); // Get the card assets
	}
	public Card GetCard(Cards template, bool isDouble) // Creates a card. Single sided cards will only use the front stats
	{
		// Default values of cards set first here
		int[] stats = new int[4] {0, 0, 0, 0}; // Front attack and health, then back
		Modifiers[] modifiers = new Modifiers[2] { Modifiers.None, Modifiers.None}; // Front modifier, then back. (Front should be default for modifiers that don't need a side, eg: Free)
		Effects effect = Effects.None; // Preformed when card is placed; Normally paired with 0/0 statline
		// Then adjust values based on card requested:
		switch (template) // These values are subject to change, but they'll do for now
		{
			case Cards.Random:
				stats = new int[4] { Random.Range(0, 3), Random.Range(0, 3), Random.Range(1, 5), Random.Range(1, 5) };
				break;
			case Cards.Free:
				stats = new int[4] { 0, 1, 0, 1 };
				modifiers = new Modifiers[2] { Modifiers.Free, Modifiers.None };
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
		Card newCard = Instantiate(isDouble ? cardAssets.CardObjDouble : cardAssets.CardObjSingle).GetComponent<Card>(); // Create a new card with blank values
		newCard.SetStats(stats[0], stats[1], stats[2], stats[3]); // Set the cards stats
		// Modifiers (!!! Not implemented !!!)
		// Effects 
		newCard.SetPos(-50, -50, -50); // Effectively hides the card from player view
		return newCard;
	}
	public void PreformModifier() // Placeholder until I decide someway to implement modifiers
	{

	}
}
