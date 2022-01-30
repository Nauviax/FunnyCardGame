using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Cards // 2 cost per attack point, 1 per health
{
	None, // Will return a card with no stats, used for cloning cards
	Random, // Will return a card with attack 0-2 and health 1-4 inclusive, NOT a random premade card (Use as placeholder cards, but never in final game)
	Free, // The free card drawn from the free card pile
	Basic1, // Basic cards have no modifiers, just interesting stat combinations. These also double as starter cards / common cards
	Basic2,
	Basic3,
	Basic4,
	Basic5,
	Venomous,
	Flying,
	Dusty,
	Moving,
	Brutish,
	Pronged,
	Thorny,
	Musical,
	Syphoning,
	Guarding,
	VampiricMove, // This card has two modifiers. Otherwise acts the same
	FlyingBrute, // Ditto
	// The rest of these cards are effect cards. The statline is always 0/0 and they are planned to all have the same card rune
	// I have made the desision to NOT implement these cards in the jam, we have little time
	//Buff,
	//Flip,
	//Nuke,
	//Kill,
	//Dust,
	//Skip,
	//Find,
}
public enum Modifiers // Cost 5 each
{
	None, // Default modifier (Does not cost !!!)
	Free, // Costs nothing to play (Does not cost !!!)
	Venomous, // Instantly kill cards upon damaging them
	Flying, // Will always attack opponent directly, over cards
	Dusty, // Will give 5 extra dust on death, on top of normal drops (This is implemented in Card.cs, not GameLogic.cs)
	MovingL, // Moves to the left after all friendly cards have attacked
	MovingR, // Like above, but to the right. MovingL becomes this card if blocked, and vice versa
	Brutish, // Takes one less damage on this side, possibly taking none
	Pronged, // Attacks to the left and right, rather than straight ahead
	Thorny, // Attackers take one point of damage when attacking this side
	Musical, // Other friendly cards currently played on this side permanently gain one attack
	Syphoning, // Gains one health after attacking, but takes one from the other side of this card
	Guarding, // Absorbs all enemy attacks on this side, taking the damage ON THIS LANE instead (If first hit kills this card, take direct damage after)
	Vampiric, // When this card kills an enemy, gain two health. (This side only)
}
public enum Effects // Normally put on 0/0 cards, Varying cost, not recovered: < x > (Effects NOT implemented)
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
	[SerializeField] public CardAssets cardAssets; // Will store references to all GameObjects for rendering cards
	public int GetCostOfEffect(Effects effect) // Returns the dust value of an effect
	{
		switch (effect) // Edit/add costs of effects here
		{
			case Effects.None:
				return 0;
			case Effects.Buff:
				return 10;
			case Effects.Flip:
				return 5;
			case Effects.Nuke:
				return 7;
			case Effects.Kill:
				return 10;
			case Effects.Dust:
				return 0;
			case Effects.Skip:
				return 12;
			case Effects.Find:
				return 8;
			default:
				return 999; // Something dumb to make it obvious something wasn't set
		}
	}
	public string GetModDesc(Modifiers input) // Returns a description of a modifier
	{
		switch (input)
		{
			case Modifiers.None:
				return "No modifier rune.";
			case Modifiers.Free:
				return "A card with this rune costs nothing to play, but still returns its full value on death.";
			case Modifiers.Venomous:
				return "Instantly kill any card upon damaging them.";
			case Modifiers.Flying:
				return "Will always attack opponent directly, over cards.";
			case Modifiers.Dusty:
				return "Will give 5 extra dust on death, on top of normal drops.";
			case Modifiers.MovingL:
				return "Moves to the left after all friendly cards have attacked.";
			case Modifiers.MovingR:
				return "Moves to the right after all friendly cards have attacked.";
			case Modifiers.Brutish:
				return "Takes one less damage on this side - potentially taking none.";
			case Modifiers.Pronged:
				return "Attacks to the left and right, rather than straight ahead.";
			case Modifiers.Thorny:
				return "Attackers take one point of damage when attacking this side.";
			case Modifiers.Musical:
				return "Other friendly cards currently played on this side permanently gain one attack.";
			case Modifiers.Syphoning://potentially rename to parasitic? would make description more intuitive. currently sounds like lifesteal
				return "Gains one health after attacking, but the other side of this card takes one damage.";
			case Modifiers.Guarding:
				return "All enemy attacks on this side are redirected to this lane instead. If this card dies during enemy move, take direct damage from further attacks.";
			case Modifiers.Vampiric:
				return "When this card kills an enemy, this side gains two health.";
			default:
				return "This modifier has no description. (This is a BUG!)";
		}

	}
	public string GetEffDesc(Effects effect) // Returns a description of an effect
	{
		switch (effect)
		{
			case Effects.None:
				return "No effect rune";
			case Effects.Buff:
				return "+1 damage and health to all friendly cards on the side this card is placed on";
			case Effects.Flip:
				return "Placed on top of any card, will flip that card and consume this card";
			case Effects.Nuke:
				return "ALL cards take 1 damage on this side";
			case Effects.Kill:
				return "Placed on top of any card, deals 10 damage to this side of that card";
			case Effects.Dust:
				return "Immediatly gives the player 5 dust";
			case Effects.Skip:
				return "The enemy will not attack or move for one turn";
			case Effects.Find:
				return "Draw any card from your owned cards, including ones that are in hand or dead";
			default:
				return "This effect has no description. (This is a BUG!)";
		}
	}
	public Card GetCard(Cards template, bool isDouble) // Creates a card. Single sided cards will only use the front stats
	{
		// Default values of cards set first here
		int[] stats = new int[4] {0, 0, 0, 0}; // Front attack and health, then back
		Modifiers[] modifiers = new Modifiers[2] { Modifiers.None, Modifiers.None}; // Front modifier, then back. (Front should be default for modifiers that don't need a side, eg: Free)
		Effects effect = Effects.None; // Preformed when card is placed; Normally paired with 0/0 statline
		GameObject cardRune = cardAssets.CNone; // These are the runes used by the card (CardRune same front and back)
		GameObject[] modifierRune = new GameObject[2] { cardAssets.MNone, cardAssets.MNone }; // First index for front, second for back
		GameObject effectRune = cardAssets.ENone; // Effects never require a side
		// Then adjust values based on card requested:
		switch (template) // These values are subject to change, but they'll do for now
		{
			// Basic cards begin here
			case Cards.Random: // Generates random, weighted stats
				int attack = 999; // If you see this number, something went wrong
				switch (Random.Range(0, 10)) // Weighted attack
				{
					case 0:
						attack = 0;
						break;
					case 1: case 2: case 3: case 4: case 5: case 6: case 7:
						attack = 1;
						break;
					case 8: case 9:
						attack = 2;
						break;
				}
				int health = 999; // If you see this number, something went wrong
				switch (Random.Range(0, 10)) // Weighted health
				{
					case 0: case 1: case 2:
						health = 1;
						break;
					case 3: case 4: case 5: case 6:
						health = 2;
						break;
					case 7: case 8:
						health = 3;
						break;
					case 9:
						health = 4;
						break;
				}
				stats = new int[4] { attack, health, attack, health }; // Only one side will be used
				cardRune = cardAssets.CNone;
				break;
			case Cards.Free:
				stats = new int[4] { 0, 1, 0, 1 };
				modifiers = new Modifiers[2] { Modifiers.Free, Modifiers.None };
				cardRune = cardAssets.CFree;
				modifierRune = new GameObject[] {cardAssets.MFree, cardAssets.MNone};
				break;
			case Cards.Basic1:
				stats = new int[4] { 1, 3, 2, 1 };
				cardRune = cardAssets.CBasic1;
				break;
			case Cards.Basic2:
				stats = new int[4] { 2, 2, 1, 2 };
				cardRune = cardAssets.CBasic2;
				break;
			case Cards.Basic3:
				stats = new int[4] { 1, 1, 1, 2 };
				cardRune = cardAssets.CBasic3;
				break;
			case Cards.Basic4:
				stats = new int[4] { 0, 1, 1, 1 };
				cardRune = cardAssets.CBasic4;
				break;
			case Cards.Basic5:
				stats = new int[4] { 2, 1, 0, 4 };
				cardRune = cardAssets.CBasic5;
				break;
		// Modifier cards begin here
			case Cards.Venomous:
				stats = new int[4] { 1, 2, 0, 1 };
				modifiers = new Modifiers[2] { Modifiers.Venomous, Modifiers.None };
				cardRune = cardAssets.CVenomous;
				modifierRune = new GameObject[2] { cardAssets.MVenomous, cardAssets.MNone };
				break;
			case Cards.Flying:
				stats = new int[4] { 1, 1, 2, 1 };
				modifiers = new Modifiers[2] { Modifiers.Flying, Modifiers.None };
				cardRune = cardAssets.CFlying;
				modifierRune = new GameObject[2] { cardAssets.MFlying, cardAssets.MNone };
				break;
			case Cards.Dusty:
				stats = new int[4] { 0, 2, 0, 2 };
				modifiers = new Modifiers[2] { Modifiers.Dusty, Modifiers.None };
				cardRune = cardAssets.CDusty;
				modifierRune = new GameObject[2] { cardAssets.MDusty, cardAssets.MNone };
				break;
			case Cards.Moving:
				stats = new int[4] { 2, 1, 1, 2 };
				modifiers = new Modifiers[2] { Modifiers.MovingR, Modifiers.None }; // This card goes right first
				cardRune = cardAssets.CMoving;
				modifierRune = new GameObject[2] { cardAssets.MMovingR, cardAssets.MNone };
				break;
			case Cards.Brutish:
				stats = new int[4] { 1, 4, 0, 4 };
				modifiers = new Modifiers[2] { Modifiers.Brutish, Modifiers.None };
				cardRune = cardAssets.CBrutish;
				modifierRune = new GameObject[2] { cardAssets.MBrutish, cardAssets.MNone };
				break;
			case Cards.Pronged:
				stats = new int[4] { 1, 1, 1, 1 };
				modifiers = new Modifiers[2] { Modifiers.Pronged, Modifiers.None };
				cardRune = cardAssets.CPronged;
				modifierRune = new GameObject[2] { cardAssets.MPronged, cardAssets.MNone };
				break;
			case Cards.Thorny:
				stats = new int[4] { 0, 3, 0, 2 };
				modifiers = new Modifiers[2] { Modifiers.Thorny, Modifiers.None };
				cardRune = cardAssets.CThorny;
				modifierRune = new GameObject[2] { cardAssets.MThorny, cardAssets.MNone };
				break;
			case Cards.Musical:
				stats = new int[4] { 1, 2, 2, 1 };
				modifiers = new Modifiers[2] { Modifiers.Musical, Modifiers.None };
				cardRune = cardAssets.CMusical;
				modifierRune = new GameObject[2] { cardAssets.MMusical, cardAssets.MNone };
				break;
			case Cards.Syphoning:
				stats = new int[4] { 2, 1, 0, 8 };
				modifiers = new Modifiers[2] { Modifiers.Syphoning, Modifiers.None };
				cardRune = cardAssets.CSyphoning;
				modifierRune = new GameObject[2] { cardAssets.MSyphoning, cardAssets.MNone };
				break;
			case Cards.Guarding:
				stats = new int[4] { 0, 5, 0, 5 };
				modifiers = new Modifiers[2] { Modifiers.Guarding, Modifiers.None };
				cardRune = cardAssets.CGuarding;
				modifierRune = new GameObject[2] { cardAssets.MGuarding, cardAssets.MNone };
				break;
			case Cards.VampiricMove:
				stats = new int[4] { 2, 2, 0, 3 };
				modifiers = new Modifiers[2] { Modifiers.Vampiric, Modifiers.MovingR };
				cardRune = cardAssets.CVampiricMove;
				modifierRune = new GameObject[2] { cardAssets.MVampiric, cardAssets.MMovingR };
				break;
			case Cards.FlyingBrute:
				stats = new int[4] { 1, 2, 2, 2 };
				modifiers = new Modifiers[2] { Modifiers.Flying, Modifiers.Brutish };
				cardRune = cardAssets.CFlyingBrute;
				modifierRune = new GameObject[2] { cardAssets.MFlying, cardAssets.MBrutish };
				break;
			// Effect cards begin here (Currently not implemented)
			/*
			case Cards.Buff:
				effect = Effects.Buff;
				cardRune = cardAssets.CEffect;
				effectRune = cardAssets.EBuff;
				break;
			case Cards.Flip:
				effect = Effects.Flip;
				cardRune = cardAssets.CEffect;
				effectRune = cardAssets.EFlip;
				break;
			case Cards.Nuke:
				effect = Effects.Nuke;
				cardRune = cardAssets.CEffect;
				effectRune = cardAssets.ENuke;
				break;
			case Cards.Kill:
				effect = Effects.Kill;
				cardRune = cardAssets.CEffect;
				effectRune = cardAssets.EKill;
				break;
			case Cards.Dust:
				effect = Effects.Dust;
				cardRune = cardAssets.CEffect;
				effectRune = cardAssets.EDust;
				break;
			case Cards.Skip:
				effect = Effects.Skip;
				cardRune = cardAssets.CEffect;
				effectRune = cardAssets.ESkip;
				break;
			case Cards.Find:
				effect = Effects.Find;
				cardRune = cardAssets.CEffect;
				effectRune = cardAssets.EFind;
				break;
			*/
			default: // "None" card, will return a placeholder card (0,0,0,0 etc)
				break;
		}
		Card newCard = Instantiate(isDouble ? cardAssets.CardObjDouble : cardAssets.CardObjSingle).GetComponent<Card>(); // Create a new card with blank values
		newCard.SetStats(stats, modifiers, effect, cardRune, modifierRune, effectRune, this); // Set the cards stats, modifiers etc and runes, and gives it a reference to this script
		newCard.SetPos(-50, -50, -50); // Effectively hides the card from player view
		return newCard;
	}
}
