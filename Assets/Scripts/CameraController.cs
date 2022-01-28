using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float rotateSpeed = 90f;
    public Transform targetPosition;
    Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        targetPosition = gameObject.transform;
        camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        camera.transform.position = Vector3.MoveTowards(camera.transform.position, targetPosition.position, Time.deltaTime*moveSpeed);
        camera.transform.rotation = Quaternion.RotateTowards(camera.transform.rotation, targetPosition.rotation, Time.deltaTime*rotateSpeed);
    }

    //looking up
    //18 10 -32 position
    //0 0 0 rotation

    //looking down
    //18.2 7 -32 position
    //13 -20 0 rotation
}
