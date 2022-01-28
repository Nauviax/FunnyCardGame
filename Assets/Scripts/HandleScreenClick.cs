using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleScreenClick : MonoBehaviour
{

    Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        Debug.Log("Aquired Camera");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                IClickable clickable = (IClickable)hit.collider.gameObject.GetComponent(typeof(IClickable));
                
                if (clickable != null) {
                    clickable.onClick();
                } else if (hit.collider.gameObject.transform.parent != null) {
                    //this is horrifying
                    //the reason for this is because the collider is part of a child of the card class, which contains the IClickable interface.
                    clickable = (IClickable)hit.collider.gameObject.transform.parent.gameObject.GetComponent(typeof(IClickable));

                    if (clickable != null) {
                        clickable.onClick();
                    }
                }
            }
        }
    }
}
