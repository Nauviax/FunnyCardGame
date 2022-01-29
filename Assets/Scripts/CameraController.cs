using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    float moveSpeed = 30f;
    float rotateSpeed = 90f;
    public Vector3 targetPosition;
    public Quaternion targetRotation;
    Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        
        camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();

        targetPosition = camera.transform.position;
        targetRotation = camera.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        camera.transform.position = Vector3.MoveTowards(camera.transform.position, targetPosition, Time.deltaTime*moveSpeed);
        camera.transform.rotation = Quaternion.RotateTowards(camera.transform.rotation, targetRotation, Time.deltaTime*rotateSpeed);
    }

    //looking up
    //18 10 -32 position
    //0 0 0 rotation

    //looking down
    //18.2 7 -32 position
    //16 5 -27 new position
    //13 -20 0 rotation
}
