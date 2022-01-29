using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    [SerializeField] GameObject runeObj;
    //hand position is 12 0 17

    public List<Card> cards;
    //TODO: include this if there ever is a setting page
    public bool invertedScroll = false;
    public bool lookingDown = false;
    GameLogic gameLogic;
    GameObject hand;

    // Start is called before the first frame update
    void Start()
    {
        gameLogic = gameObject.GetComponent<GameLogic>();
        hand = GameObject.FindWithTag("Hand");
    }

    public bool firstUpdate = true;
    // Update is called once per frame
    void Update()
    {
        if (firstUpdate) {
            cards = gameLogic.board.gameHand;
            lookUp();
            firstUpdate = false;
        }

        

        //TODO: add alternate ways to look at hand, ie 's', down arrow
        if ((invertedScroll ? -1:1) * Input.mouseScrollDelta.y < 0 && !lookingDown) {
            lookDown();
            Debug.Log("looked down");
        }
        if ((invertedScroll ? -1 : 1) * Input.mouseScrollDelta.y > 0 && lookingDown) {
            lookUp();
            Debug.Log("looked up");
        }


        if (Input.GetKeyDown(KeyCode.Q)) {
            gameLogic.PlaceCard(cards[0], 1);
        }
        if (Input.GetKeyDown(KeyCode.Return)) {
            gameLogic.EndTurn();
        }
    }
    
    public void lookDown() {
        gameObject.GetComponent<CameraController>().targetPosition = new Vector3(18.2f, 7, -32);
        gameObject.GetComponent<CameraController>().targetRotation = Quaternion.Euler(13, -20, 0);
        gameObject.GetComponent<HandController>().targetPosition = new Vector3(12, 0, -17);
        lookingDown = true;
    }

    public void lookUp() {
        gameObject.GetComponent<CameraController>().targetPosition = new Vector3(18, 10, -32);
        gameObject.GetComponent<CameraController>().targetRotation = Quaternion.Euler(0, 0, 0);
        gameObject.GetComponent<HandController>().targetPosition = new Vector3(15, -9, -25);
        lookingDown = false;
    }

    public void addCard(Card card) {
		//TODO: just put code for the rune type through here, may need to add another parameter
		GameObject cardRunePrefab = card.CardRune;
		// = Instantiate(runeObj);	// LACHLAN I BROKE YOUR SHIT !!! <--------  Each card (script) now has a reference to it's cardRune prefab, can the hand hold an instance of it? !!! LACHLAN LOOK
        updateHand();
    }
    public void removeCard(Card card) {
        
        updateHand();
    }
    public void updateHand() {
        //this is where jfkghnz.b
    }
}
