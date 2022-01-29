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
            gameLogic.PlaceCard(cards[0], 0);
        }
        if (Input.GetKeyDown(KeyCode.W)) {
            gameLogic.PlaceCard(cards[0], 1);
        }
        if (Input.GetKeyDown(KeyCode.E)) {
            gameLogic.PlaceCard(cards[0], 2);
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            gameLogic.PlaceCard(cards[0], 3);
        }
        if (Input.GetKeyDown(KeyCode.Return)) {
            gameLogic.EndTurn();
		}
		if (Input.GetKeyDown(KeyCode.O)) // Game begins here
		{
			gameLogic.BeginGame();
		}
        if (Input.GetKeyDown(KeyCode.F)) // Game begins here
        {
            gameLogic.FreeCardGet(false);
        }
    }
    
    public void lookDown() {
        gameObject.GetComponent<CameraController>().targetPosition = new Vector3(16, 5, -27);
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
        card.cardRuneInstance = Instantiate(card.CardRune);
        card.cardRuneInstance.transform.localScale = new Vector3(1.2f, 1.2f, 0.3f);
        card.cardRuneInstance.transform.rotation = Quaternion.Euler(13, -20, 0);
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
            foreach(MeshRenderer mesh in card.transform.GetComponentsInChildren<MeshRenderer>()) {
                mesh.enabled = false;
            }
            card.transform.parent = hand.transform;
            card.TargetPosition = new Vector3((cards.Count<8?-8f:-10f) + ((cards.Count < 8 ? 16f : 20f) / (cards.Count-2))*pos, 0.2f, -2.25f);
            Debug.Log(card.transform.position.ToString());
            pos++;
        }
    }
}
