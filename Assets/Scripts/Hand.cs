using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public List<Card> cards;
    //TODO: include this if there ever is a setting page
    public bool invertedScroll = false;
    public bool lookingDown = false;

    // Start is called before the first frame update
    void Start()
    {
        cards = gameObject.GetComponent<GameLogic>();
    }

    // Update is called once per frame
    void Update()
    {
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
