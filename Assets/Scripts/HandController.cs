using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    float moveSpeed = 50f;
    public Vector3 targetPosition;
    GameObject hand;

    // Start is called before the first frame update
    void Start()
    {
        
        hand = GameObject.FindWithTag("Hand");
        targetPosition = hand.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        hand.transform.position = Vector3.MoveTowards(hand.transform.position, targetPosition, Time.deltaTime*moveSpeed);
    }

    //looking up
    //15 -9 -25 position

    //looking down
    //12 0 17 position
}
