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

    public bool firstUpdate = true;
    // Update is called once per frame
    void Update()
    {
        if (firstUpdate) {
            cards = gameLogic.board.gameHand;
            
            firstUpdate = false;
        }

        

        //TODO: add alternate ways to look at hand, ie 's', down arrow
        if ((invertedScroll ? -1:1) * Input.mouseScrollDelta.y<0 && !lookingDown) {
            lookDown();
            Debug.Log("looked down");
        }
        if ((invertedScroll ? -1 : 1) * Input.mouseScrollDelta.y > 0 && lookingDown) {
            lookUp();
            Debug.Log("looked up");
        } 
    }
    
    public void lookDown() {
        gameObject.GetComponent<CameraController>().targetPosition.position = new Vector3(18.2f, 7, -32);
        gameObject.GetComponent<CameraController>().targetPosition.rotation = Quaternion.Euler(13, -20, 0);
        lookingDown = true;
    }

    public void lookUp() {
        gameObject.GetComponent<CameraController>().targetPosition.position = new Vector3(18, 10, -32);
        gameObject.GetComponent<CameraController>().targetPosition.rotation = Quaternion.Euler(0, 0, 0);
        lookingDown = false;
    }
}
