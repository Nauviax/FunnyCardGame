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
        if (inHand) {
            hand.currentDisplay = card;
            hand.updateInfo();
        }

    }

    public void onClick() {
        throw new System.NotImplementedException();
    }
}
