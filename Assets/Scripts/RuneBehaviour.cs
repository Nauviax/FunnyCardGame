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
        Debug.Log("hi it me");
        if (inHand) {
            hand.currentDisplay = card;
            hand.updateInfo();
            Debug.Log("hi it me but cooler");
        }

    }

    public void onClick() {
        throw new System.NotImplementedException();
    }
}
