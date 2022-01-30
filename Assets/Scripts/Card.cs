using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // For editing the text

public class Card : MonoBehaviour, IClickable
{
	[SerializeField] int[] stats; // Stores card stats, damage then health for front, then back
	[SerializeField] Modifiers[] modifiers; // Stores modifiers for both sides, front first
	Effects effect; // Unused
	GameObject cardRune; // These hold prefabs, not ingame objects!
	GameObject[] modifierRunes; // Single sided cards will just use front, or index 0
	GameObject effectRune;
	public GameObject cardRuneInstance; // These hold ingame objects, not prefabs!
	public GameObject[] modifierRunesInstance;
	public GameObject effectRuneInstance;
	[SerializeField] int dustCost; // Cost of this card
	[SerializeField] int dustValue; // Value of this card (This value is returned to the player on death)

	PremadeCards premade; // Mainly used for getting dust values of effects, retrieved from GameLogic
	TextMeshPro textFront;
	TextMeshPro textBack;
	string fill = "         "; // Might make cleaner later idk

	// Smooth movement code
	float moveSpeed = 100f;
	float rotateSpeed = 180f;
	Vector3 targetPosition;
	Quaternion targetRotation;

	void Start() // Blame Lachlan if this breaks
	{
		//TargetPosition = transform.position;
	}

	void Update() // Blame Lachlan if this breaks
	{
		if (transform.parent != null) transform.position = Vector3.MoveTowards(transform.position, transform.parent.transform.TransformPoint(TargetPosition), Time.deltaTime * moveSpeed);
		else transform.position = Vector3.MoveTowards(transform.position, TargetPosition, Time.deltaTime * moveSpeed);
		transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
		if (cardRuneInstance != null){
			cardRuneInstance.transform.position = transform.position + new Vector3(0, 0, -0.2f);
			cardRuneInstance.transform.rotation = transform.rotation;
            foreach (GameObject modRune in modifierRunesInstance) {
				modRune.transform.position = transform.position + new Vector3(0, -2.5f, -0.2f);
				modRune.transform.rotation = transform.rotation;
            }
		}
	}
	public void Clone(Card cloneMe) // Creates this card to be a copy of the given card
	{
		SetStats(new int[4] { cloneMe.stats[0], cloneMe.stats[1], cloneMe.stats[2], cloneMe.stats[3], }, cloneMe.CardModifiers, cloneMe.CardEffect, cloneMe.cardRune, cloneMe.modifierRunes, cloneMe.effectRune, cloneMe.premade); // Copies all info regarding card
		SetPos(cloneMe.transform.position[0], cloneMe.transform.position[1], cloneMe.transform.position[2]); // Will be moved later anyway (This pos is likely -50,-50,-50 if coming from gameLocic.cs)
	}
    public void SetStats(int[] stats, Modifiers[] modifiers, Effects effect, GameObject cardRune, GameObject[] modifierRunes, GameObject effectRune, PremadeCards premade)
	{
		this.stats = stats;
		this.modifiers = modifiers;
		this.effect = effect;
		this.cardRune = cardRune;
		this.modifierRunes = modifierRunes;
		this.effectRune = effectRune;
		this.premade = premade; // Should be passed in from GameLogic
		dustValue = CalculateDustValue(); // Gets the value of this card, taking into account it's stats and modifiers
		dustCost = CalculateDustCost(dustValue); // Gets the decided cost of playing this card, taking into account effects and the free modifier
		textFront = transform.GetChild(0).GetComponent<TextMeshPro>();
		textBack = transform.GetChild(1).GetComponent<TextMeshPro>();
		UpdateFace();
	}
	public void SetPos(float xx, float yy, float zz)
	{
		//hi it moves smoothly now
		//transform.position = new Vector3(xx, yy, zz);
		TargetPosition = new Vector3(xx, yy, zz);
	}
	public void SetRotation(float xx, float yy, float zz) {
		targetRotation = Quaternion.Euler(xx, yy, zz);
    }
	public void Destroy() // Called when card runs out of health
	{
		Destroy(this.cardRuneInstance);
		Destroy(gameObject); // Fucking dies
	}
	void UpdateFace() // Update all runes/text on this card (!!! Implement updating runes somehow)
	{
		textFront.SetText((DamageFront != 0 ? DamageFront.ToString() : " ") + fill + (HealthFront != 0 ? HealthFront.ToString() : " "));
		textBack.SetText((DamageBack != 0 ? DamageBack.ToString() : " ") + fill + (HealthBack != 0 ? HealthBack.ToString() : " "));
	}
	int CalculateDustValue() // Returns the calculated value of this card, the amount of dust returned when card dies
	{
		int value = 0;
		value += stats[0] * 2 + stats[2] * 2 + stats[1] + stats[3]; // Add damage and health values, damage is worth 2
		value += modifiers[0] != Modifiers.None && modifiers[0] != Modifiers.Free ? 5 : 0; // Add cost of any modifiers, excluding Free and None (5 ea)
		value += modifiers[1] != Modifiers.None && modifiers[1] != Modifiers.Free ? 5 : 0; // Same as above, but for the other side
		value += (modifiers[0] == Modifiers.Dusty ? 5 : 0) + (modifiers[1] == Modifiers.Dusty ? 5 : 0); // Dusty makes this card drop 5 extra free dust on death. This value is not counted towards it's cost, but the modifier itself is.
		return value;
	}
	int CalculateDustCost(int value) // Returns the calculated cost of this card, the amount of dust required to play the card (This is NOT the same as it's value)
	{
		if (modifiers[0] == Modifiers.Free || modifiers[1] == Modifiers.Free)
		{
			return 0; // This card is free to play
		}
		else
		{
			
			int cost = value + premade.GetCostOfEffect(effect); // Player gets damage/health and modifier costs refunded, but NOT the cost of effects.
			cost += (modifiers[0] == Modifiers.Dusty ? -5 : 0) + (modifiers[1] == Modifiers.Dusty ? -5 : 0); // The EXTRA value from the dusty modifier should not be counted towards the cost of the card.
			return cost; // As effects are normally combined with a 0/0 statline, effect cards normally have a value of 0, and a non 0 cost
		}
	}
	public void Turn() // If this card has a moving sigil, flip that sigil (Will only flip front if both side have moving, so DON'T DO THAT)
	{
		if (modifiers[0] == Modifiers.MovingL)
		{
			modifiers[0] = Modifiers.MovingR;
			modifierRunes[0] = premade.cardAssets.MMovingR;
			UpdateFace();
			return;
		}
		if (modifiers[0] == Modifiers.MovingR)
		{
			modifiers[0] = Modifiers.MovingL;
			modifierRunes[0] = premade.cardAssets.MMovingL;
			UpdateFace();
			return;
		}
		if (modifiers[1] == Modifiers.MovingL)
		{
			modifiers[1] = Modifiers.MovingR;
			modifierRunes[1] = premade.cardAssets.MMovingR;
			UpdateFace();
			return;
		}
		if (modifiers[1] == Modifiers.MovingR)
		{
			modifiers[1] = Modifiers.MovingL;
			modifierRunes[1] = premade.cardAssets.MMovingL;
			UpdateFace();
			return;
		}
	}
	public int DamageFront
	{
		get { return stats[0]; }
		set { stats[0] = value; UpdateFace(); }
	}
	public int HealthFront
	{
		get { return stats[1]; }
		set { stats[1] = value; UpdateFace(); }
	}
	public int DamageBack
	{
		get { return stats[2]; }
		set { stats[2] = value; UpdateFace(); }
	}
	public int HealthBack
	{
		get { return stats[3]; }
		set { stats[3] = value; UpdateFace(); }
	}
	public int DustValue
	{
		get { return dustValue; }
	}
	public int DustCost
	{
		get { return dustCost; }
	}
	public Vector3 TargetPosition
	{
		get { return targetPosition; }
		set
		{
			targetPosition = value;
			//Debug.Log(value.ToString()); // Debug !!!
		}
	}
	public Modifiers[] CardModifiers // If you want to change or remove a modifier, try doing so by making a public method in this card class, and also update it's runes! (eg: MovingR -> MovingL) !!!
	{
		get { return modifiers; }
	}
	public Effects CardEffect
	{
		get { return effect; }
	}
	public GameObject CardRune
	{
		get { return cardRune; }
	}
	public GameObject[] ModifierRunes // See Modifiers Property's comment
	{
		get { return modifierRunes; }
	}
	public GameObject EffectRune
	{
		get { return effectRune; }
	}


	// Lachlan put his shit below properties again, fine it can stay here
	public void onClick() {
		Debug.Log("oof owie i hath been click-ed-eth");
		Debug.Log("Front: " + HealthFront + fill + DamageFront);
		Debug.Log("Back: " + HealthBack + fill + DamageBack);
	}
}
