using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // For editing the text

public class Card : MonoBehaviour
{
	int damageFront;
	int healthFront;
	int damageBack;
	int healthBack;
	TextMeshPro textFront;
	TextMeshPro textBack;
	string fill = "         "; // Might make cleaner later idk

	public void setStats(int damageFront, int healthFront, int damageBack, int healthBack)
	{
		this.damageFront = damageFront;
		this.healthFront = healthFront;
		this.damageBack = damageBack;
		this.healthBack = healthBack;
		textFront = transform.GetChild(0).GetComponent<TextMeshPro>();
		textBack = transform.GetChild(1).GetComponent<TextMeshPro>();
		UpdateText();
	}
	public void setPos(float xx, float yy, float zz)
	{
		transform.position = new Vector3(xx, yy, zz);
	}
	void Update()
	{
		
	}
	void UpdateText()
	{
		textFront.SetText(HealthFront.ToString() + fill + DamageFront.ToString());
		textBack.SetText(HealthBack.ToString() + fill + DamageBack.ToString());
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
}
