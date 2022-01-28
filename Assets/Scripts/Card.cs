using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // For editing the text

public class Card : MonoBehaviour, IClickable
{
	int damageFront;
	int healthFront;
	int damageBack;
	int healthBack;
	int dustValue;
	TextMeshPro textFront;
	TextMeshPro textBack;
	string fill = "         "; // Might make cleaner later idk

	//the bit displayed in the hand
	public GameObject rune;

	public void SetStats(int damageFront, int healthFront, int damageBack, int healthBack) // May be replaced later when premade card stats can be stored in bulk
	{
		this.damageFront = damageFront;
		this.healthFront = healthFront;
		this.damageBack = damageBack;
		this.healthBack = healthBack;
		dustValue = damageFront * 2 + damageBack * 2 + healthFront + healthBack; // A simple formula
		textFront = transform.GetChild(0).GetComponent<TextMeshPro>();
		textBack = transform.GetChild(1).GetComponent<TextMeshPro>();
		UpdateText();
	}
	public void SetPos(float xx, float yy, float zz)
	{
		transform.position = new Vector3(xx, yy, zz);
	}
	public void Destroy() // Called when card runs out of health
	{
		Destroy(gameObject); // Fucking dies
	}
	void UpdateText() // Update the statline on this card, displaying nothing for damage if it is 0
	{
		textFront.SetText((DamageFront != 0 ? DamageFront.ToString() : " ") + fill + HealthFront.ToString());
		textBack.SetText((DamageFront != 0 ? DamageFront.ToString() : " ") + fill + HealthBack.ToString());
	}
	public int DamageFront
	{
		get { return damageFront; }
		set { damageFront = value; UpdateText(); }
	}
	public int HealthFront
	{
		get { return healthFront; }
		set { healthFront = value; UpdateText(); }
	}
	public int DamageBack
	{
		get { return damageBack; }
		set { damageBack = value; UpdateText(); }
	}
	public int HealthBack
	{
		get { return healthBack; }
		set { healthBack = value; UpdateText(); }
	}
	public int DustValue
	{
		get { return dustValue; }
	}

	public void onClick() {
		Debug.Log("oof owie i hath been click-ed-eth");
		Debug.Log("Front: "+healthFront+fill+damageFront);
		Debug.Log("Back: " + healthBack + fill + damageBack);
	}
}
