using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public Transform lookAt;
	public float distance = 7.0f;
	public float lagSpeed = 10.0f;
	public float offsetX = 2.0f;
	public float offsetY = 1.0f;
	public float sensitivityX = 1.0f;
	public float sensitivityY = 1.0f;
	public float YAngleMin = -70.0f;
	public float YAngleMax = 70.0f;

	private Camera cam;
	private Vector3 offset;
	private float currentX;
	private float currentY;
	private float deltaTime;
	private float lerpX = 0f;
	private float lerpY = 0f;


    void Start()
    {
		cam = Camera.main;

		Transform cameraTransform = cam.transform;
		offset = new Vector3(offsetX, offsetY, 0f);

		cameraTransform.parent = transform;
		cameraTransform.localPosition = transform.position + offset;

		transform.position = lookAt.position;
	}
    
    void Update()
    {
		// Taking input from mouse..
		currentX += Input.GetAxis("Mouse X");
		currentY -= Input.GetAxis("Mouse Y");
		currentY = Mathf.Clamp(currentY, YAngleMin, YAngleMax);

		deltaTime = Time.deltaTime;
		lerpX = Mathf.Lerp(lerpX, currentX, deltaTime * sensitivityX);
		lerpY = Mathf.Lerp(lerpY, currentY, deltaTime * sensitivityY);
	}

	void LateUpdate()
	{
		// Rotation & positioning
		Vector3 dir = new Vector3(0,0,-distance);
		Quaternion rotation = Quaternion.Euler(lerpY, lerpX, 0);

		Vector3 lerpPosition = Vector3.Lerp(transform.position, lookAt.position + (rotation * dir), Time.deltaTime * lagSpeed);
		transform.position = lerpPosition;

		//Vector3 playerVelocity = lookAt.GetComponent<CharacterController>().velocity;
		Vector3 lookVector = lookAt.position; /// - (playerVelocity * (0.5f / lagSpeed));
		lookVector.y = lookAt.position.y + (currentY * Time.deltaTime);
		transform.LookAt(lookVector);
	}
}
