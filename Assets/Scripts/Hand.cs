using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public List<Card> cards;
    //TODO: include this if there ever is a setting page
    public bool invertedScroll = false;
    public bool lookingDown = false;
    GameLogic gameLogic;

    // Start is called before the first frame update
    void Start()
    {
        gameLogic = gameObject.GetComponent<GameLogic>();
        
    }

    public bool john = true;
    // Update is called once per frame
    void Update()
    {
        if (john) {
            cards = gameLogic.board.gameHand;
            
            john = false;
        }

        gameLogic.FreeCardGet();
        Debug.Log(cards.Count);

        //TODO: add alternate ways to look at hand, ie 's', down arrow
        if ((invertedScroll ? -1:1) * Input.mouseScrollDelta.y<0 && !lookingDown) {
            lookDown();
        }
        if ((invertedScroll ? -1 : 1) * Input.mouseScrollDelta.y > 0 && lookingDown) {
            lookUp();
        } 
    }
    
    public void lookDown() {

    }

    public void lookUp() {

    }
}
