using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    [SerializeField] GameObject runeObj;
    //hand position is 12 0 17
    //relative to plane, cards should be displayed at z = -2.25 and between x = -4 and x = 4

    List<Card> cards;
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
		if (Input.GetKeyDown(KeyCode.O)) // Game begins here
		{
			gameLogic.BeginGame();
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
        //card.CardRune = Instantiate(card.CardRune);
		updateHand();
    }
    public void removeCard(Card card) {
        updateHand();
    }
    public void updateHand() {
        //this is where cards are told where to go and stuff
        cards = gameLogic.board.gameHand;
        int pos = 0;
        foreach (Card card in cards) {
            Debug.Log("did a card");
            card.transform.parent = hand.GetComponentsInChildren<GameObject>()[0].transform;
            card.TargetPosition = new Vector3(-4 + (8/cards.Count)*pos, 0.2f, -2.25f);
            pos++;
        }
    }
}
