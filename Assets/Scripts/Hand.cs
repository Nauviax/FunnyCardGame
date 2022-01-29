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
    //the card being hovered in your hand
    public Card currentDisplay;
    //Stores prefabs in the info, all neatly lined up to be destroyed when needed.
    public List<GameObject> infoDisplay = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        gameLogic = gameObject.GetComponent<GameLogic>();
        hand = GameObject.FindWithTag("Hand");
        Physics.queriesHitTriggers = true;
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
        gameObject.GetComponent<CameraController>().targetPosition = new Vector3(16, 5, 37);
        gameObject.GetComponent<CameraController>().targetRotation = Quaternion.Euler(13, -20, 0);
        gameObject.GetComponent<HandController>().targetPosition = new Vector3(12, 0, 47);
        lookingDown = true;
    }

    public void lookUp() {
        gameObject.GetComponent<CameraController>().targetPosition = new Vector3(18, 10, 22);
        gameObject.GetComponent<CameraController>().targetRotation = Quaternion.Euler(0, 0, 0);
        gameObject.GetComponent<HandController>().targetPosition = new Vector3(15, -9, 35);
        lookingDown = false;
    }

    public void addCard(Card card) {
        card.cardRuneInstance = Instantiate(card.CardRune);
        card.cardRuneInstance.transform.localScale = new Vector3(1.2f, 1.2f, 0.3f);
        card.cardRuneInstance.GetComponent<RuneBehaviour>().inHand = true;
        card.cardRuneInstance.GetComponent<RuneBehaviour>().card = card;
        card.GetComponentsInChildren<Collider>()[0].enabled = false;
        card.GetComponentsInChildren<Collider>()[1].enabled = false;
        card.SetRotation(13, -20, 0);
        updateHand();
    }
    public void removeCard(Card card) {
        card.cardRuneInstance.GetComponent<RuneBehaviour>().inHand = false;
        card.cardRuneInstance.transform.localScale = new Vector3(3, 3, 1);
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
            card.TargetPosition = new Vector3((cards.Count<8?-8f:-10f) + ((cards.Count < 8 ? 16f : 20f) / (cards.Count-1))*pos, 0.2f, -2.25f);
            Debug.Log(card.transform.position.ToString());
            pos++;
        }
    }
    public void updateInfo() {
        //I feel powerful writing this bit
        foreach(GameObject obj in infoDisplay) {
            Destroy(obj);
        }

        GameObject mainRune = Instantiate(currentDisplay.CardRune);
        Destroy(mainRune.GetComponent<RuneBehaviour>());
        mainRune.transform.parent = hand.transform;
        mainRune.transform.localPosition = new Vector3(-3.5f, 0.2f, 2);
        mainRune.transform.rotation = Quaternion.Euler(13, -20, 0);
        mainRune.transform.localScale = new Vector3(3, 3, 1);
        infoDisplay.Add(mainRune);
    }
}
