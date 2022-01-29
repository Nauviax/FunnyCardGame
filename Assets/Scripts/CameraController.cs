using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    float moveSpeed = 50f;
    float rotateSpeed = 90f;
    public Vector3 targetPosition;
    public Quaternion targetRotation;
    Camera camera;


	private void Start()
	{
		camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
		targetPosition = camera.transform.position;
		targetRotation = camera.transform.rotation;
	}

	void Update()
    {
        camera.transform.position = Vector3.MoveTowards(camera.transform.position, targetPosition, Time.deltaTime*moveSpeed);
        camera.transform.rotation = Quaternion.RotateTowards(camera.transform.rotation, targetRotation, Time.deltaTime*rotateSpeed);
		moveSpeed -= (moveSpeed - 50) * 0.003f; // Decay back to normal speed (50) (Feel free to make this work nicer)
	}
	public void Begin() // Called when GAME begins
	{
		moveSpeed *= 10; // Initial burst of speed to get to hand
	}

	//looking up
	//18 10 -32 position
	//0 0 0 rotation

	//looking down
	//18.2 7 -32 position
	//16 5 -27 new position
	//13 -20 0 rotation
}
