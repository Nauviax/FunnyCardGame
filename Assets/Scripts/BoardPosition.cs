using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardPosition : MonoBehaviour, IClickable
{
    Hand hand;
    [SerializeField] int position;

    // Start is called before the first frame update
    void Start()
    {
        hand = GameObject.FindWithTag("GameLogic").GetComponent<Hand>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onClick() {
        if (hand.placingCard) {
            GameObject.FindWithTag("GameLogic").GetComponent<GameLogic>().PlaceCard(hand.currentDisplay, position);
            hand.currentDisplay = null;
            hand.updateInfo();
        }
    }
}
