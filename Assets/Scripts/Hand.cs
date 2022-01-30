using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Hand : MonoBehaviour {
    [SerializeField] GameObject runeObj;
    [SerializeField] GameObject InfoText;
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
    //Stores whether the selected card is being placed
    public bool placingCard = false;

    // Start is called before the first frame update
    void Start() {
        gameLogic = gameObject.GetComponent<GameLogic>();
        hand = GameObject.FindWithTag("Hand");
        Physics.queriesHitTriggers = true;
    }

    public bool firstUpdate = true;
    // Update is called once per frame
    void Update() {
        if (firstUpdate) {
            cards = gameLogic.board.gameHand;
            //lookUp(); (ClickForSTARTGAME now does this, and at the right time)
            firstUpdate = false;
        }



        //Looking up and down
        if (((invertedScroll ? -1 : 1) * Input.mouseScrollDelta.y < 0 || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && !lookingDown) {
            lookDown();
            //Debug.Log("looked down");
        }
        if (((invertedScroll ? -1 : 1) * Input.mouseScrollDelta.y > 0 || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && lookingDown) {
            lookUp();
            //Debug.Log("looked up");
        }

        if (Input.GetKeyDown(KeyCode.Return)) {
            gameLogic.EndTurn();
		}
		if (Input.GetKeyDown(KeyCode.O)) // Game begins here
		{
			gameLogic.BeginGame();
		}
        if (Input.GetKeyDown(KeyCode.F)) // Gets a free card without triggering card per turn limit
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
        card.SetRotation(13, -20, 0);

        int i = 0;
        foreach (GameObject modRune in card.ModifierRunes) {
            card.modifierRunesInstance[i] = Instantiate(modRune);
            modRune.transform.localScale = new Vector3(1.2f, 1.2f, 0.3f);
        }
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
            foreach (MeshRenderer mesh in card.transform.GetComponentsInChildren<MeshRenderer>()) {
                mesh.enabled = false;
            }
            card.transform.parent = hand.transform;
            card.TargetPosition = new Vector3((cards.Count < 8 ? -8f : -10f) + ((cards.Count < 8 ? 16f : 20f) / (cards.Count - 1)) * pos, 0.2f, -2.25f);
            Debug.Log(card.transform.position.ToString());
            pos++;
        }
    }
    public void updateInfo() {
        //I feel powerful writing this bit
        foreach (GameObject obj in infoDisplay) {
            Destroy(obj);
        }
        if (currentDisplay != null) {
            GameObject mainRune = Instantiate(currentDisplay.CardRune);
            Destroy(mainRune.GetComponent<RuneBehaviour>());
            mainRune.transform.parent = hand.transform;
            mainRune.transform.localPosition = new Vector3(-10f, 0.2f, 1.5f);
            mainRune.transform.rotation = Quaternion.Euler(13, -20, 0);
            mainRune.transform.localScale = new Vector3(3, 3, 1);
            infoDisplay.Add(mainRune);
            TextMeshPro healthFront = Instantiate(InfoText).GetComponent<TextMeshPro>();
            healthFront.text = ""+currentDisplay.HealthFront;
            healthFront.transform.parent = hand.transform;
            healthFront.transform.localPosition = new Vector3(-6.65f, 0.2f, 2.3f);
            healthFront.transform.rotation = Quaternion.Euler(13, -20, 0);
            infoDisplay.Add(healthFront.transform.gameObject);
            TextMeshPro damageFront = Instantiate(InfoText).GetComponent<TextMeshPro>();
            damageFront.text = "" + currentDisplay.DamageFront;
            damageFront.transform.parent = hand.transform;
            damageFront.transform.localPosition = new Vector3(-5f, 0.2f, 2.3f);
            damageFront.transform.rotation = Quaternion.Euler(13, -20, 0);
            infoDisplay.Add(damageFront.transform.gameObject);
            TextMeshPro healthBack = Instantiate(InfoText).GetComponent<TextMeshPro>();
            healthBack.text = "" + currentDisplay.HealthBack;
            healthBack.transform.parent = hand.transform;
            healthBack.transform.localPosition = new Vector3(-6.65f, 0.2f, 0.75f);
            healthBack.transform.rotation = Quaternion.Euler(13, -20, 0);
            infoDisplay.Add(healthBack.transform.gameObject);
            TextMeshPro damageBack = Instantiate(InfoText).GetComponent<TextMeshPro>();
            damageBack.text = "" + currentDisplay.DamageBack;
            damageBack.transform.parent = hand.transform;
            damageBack.transform.localPosition = new Vector3(-5f, 0.2f, 0.75f);
            damageBack.transform.rotation = Quaternion.Euler(13, -20, 0);
            infoDisplay.Add(damageBack.transform.gameObject);
            TextMeshPro modifierInfoFront = Instantiate(InfoText).GetComponent<TextMeshPro>();
            modifierInfoFront.text = "" + gameObject.GetComponent<PremadeCards>().GetModDesc(currentDisplay.CardModifiers[0]);
            modifierInfoFront.fontSize = 6;
            modifierInfoFront.transform.parent = hand.transform;
            modifierInfoFront.transform.localPosition = new Vector3(3.5f, 0.2f, 2.3f);
            modifierInfoFront.transform.rotation = Quaternion.Euler(13, -20, 0);
            infoDisplay.Add(modifierInfoFront.transform.gameObject);
            TextMeshPro modifierInfoBack = Instantiate(InfoText).GetComponent<TextMeshPro>();
            modifierInfoBack.text = "" + gameObject.GetComponent<PremadeCards>().GetModDesc(currentDisplay.CardModifiers[1]);
            modifierInfoBack.fontSize = 6;
            modifierInfoBack.transform.parent = hand.transform;
            modifierInfoBack.transform.localPosition = new Vector3(3.5f, 0.2f, 0.75f);
            modifierInfoBack.transform.rotation = Quaternion.Euler(13, -20, 0);
            infoDisplay.Add(modifierInfoBack.transform.gameObject);
        } else placingCard = false;
    }
}
