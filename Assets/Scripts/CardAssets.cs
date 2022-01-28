using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] // Let me see everything
public class CardAssets : MonoBehaviour
{
	// Actual Card Prefab References
	public GameObject CardObjDouble; // A reference to the prefab for the double sided card
	public GameObject CardObjSingle; // The single sided one

	// Card Rune References (Large symbol in the centre)
	public GameObject CRandom;
	public GameObject CFree;
	public GameObject CBasic1;
	public GameObject CBasic2;
	public GameObject CBasic3;
	public GameObject CBasic4;
	public GameObject CBasic5;

	// Modifier Rune References (Medium symbol near the top/side)
	public GameObject MNone;
	public GameObject MFree;
	public GameObject MVenomous;
	public GameObject MFlying;
	public GameObject MDusty;
	public GameObject MMovingL;
	public GameObject MMovingR;
	public GameObject MBrutish;
	public GameObject MPronged;
	public GameObject MMusical;
	public GameObject MSyphoning;
	public GameObject MGuarding;
	public GameObject MVampiric;

	// Effect Rune References (Card will die when played, so perhaps a visual effect while in hand?)
	public GameObject ENone;
	public GameObject EBuff;
	public GameObject EFlip;
	public GameObject ENuke;
	public GameObject EKill;
	public GameObject EDust;
	public GameObject ESkip;
	public GameObject EFind;
}
