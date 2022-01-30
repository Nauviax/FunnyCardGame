using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneBehaviour : MonoBehaviour, IClickable {
    Hand hand;
    public bool inHand = false;
    public Card card;

    // Start is called before the first frame update
    void Start() {
        hand = GameObject.FindWithTag("GameLogic").GetComponent<Hand>();

    }

    // Update is called once per frame
    void Update() {

    }

    void OnMouseEnter() {
        if (inHand && !hand.placingCard) {
            hand.currentDisplay = card;
            hand.updateInfo();
        }

    }

    public void onClick()
	{
		if (hand.placingCard && card == hand.currentDisplay) {
            card.SetPos(card.TargetPosition.x, card.TargetPosition.y - 0.5f, card.TargetPosition.z);
			hand.placingCard = false;
        } else if (!hand.placingCard) {
            card.SetPos(card.TargetPosition.x, card.TargetPosition.y + 0.5f, card.TargetPosition.z);
            hand.placingCard = true;
        }

    }
}
