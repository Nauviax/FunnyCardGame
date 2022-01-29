using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Cards // 2 cost per attack point, 1 per health
{
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
	Buff,
	Flip,
	Nuke,
	Kill,
	Dust,
	Skip,
	Find,
}
public enum Modifiers // Cost 5 each (TEMP!!! if $$$ then I have implemented code for modifier)
{
	None, // Default modifier (Does not cost !!!)$$$
	Free, // Costs nothing to play (Does not cost !!!)$$$
	Venomous, // Instantly kill cards upon damaging them$$$
	Flying, // Will always attack opponent directly, over cards$$$
	Dusty, // Will give 5 extra dust on death, on top of normal drops (This is implemented in Card.cs, not GameLogic.cs)$$$
	MovingL, // Moves to the left after all friendly cards have attacked$$$
	MovingR, // Like above, but to the right. MovingL becomes this card if blocked, and vice versa$$$
	Brutish, // Takes one less damage on this side, possibly taking none$$$
	Pronged, // Attacks to the left and right, rather than straight ahead$$$
	Thorny, // Attackers take one point of damage when attacking this side$$$
	Musical, // Friendly cards gain one attack on this side of the board
	Syphoning, // Gains one health after attacking, but takes one from the other side of this card
	Guarding, // Absorbs all enemy attacks on this side, taking the damage itself instead (Even if first hit would kill it, it absorbs all hits)
	Vampiric, // When this card kills an enemy, gain two health. (This side only)$$$
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
			case Cards.Random:
				stats = new int[4] { Random.Range(0, 3), Random.Range(1, 5), Random.Range(0, 3), Random.Range(1, 5) };
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
			// Effect cards begin here
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
			default: // Shouldn't happen, will return a placeholder card (0,0,0,0 etc)
				break;
		}
		Card newCard = Instantiate(isDouble ? cardAssets.CardObjDouble : cardAssets.CardObjSingle).GetComponent<Card>(); // Create a new card with blank values
		newCard.SetStats(stats, modifiers, effect, cardRune, modifierRune, effectRune, this); // Set the cards stats, modifiers etc and runes, and gives it a reference to this script
		newCard.SetPos(-50, -50, -50); // Effectively hides the card from player view
		return newCard;
	}
}
