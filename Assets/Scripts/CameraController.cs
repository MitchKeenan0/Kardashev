using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public Transform lookAt;
	public float distance = 10.0f;
	public float offsetX = 5.0f;
	public float offsetY = 5.0f;
	public float sensitivityX = 1.0f;
	public float sensitivityY = 1.0f;
	public float YAngleMin = -70.0f;
	public float YAngleMax = 70.0f;

	private Camera cam;
	private float currentX;
	private float currentY;
	private float deltaTime;
	private float lerpX;
	private float lerpY;


    private void Start()
    {
		cam = Camera.main;
    }

    
    private void Update()
    {
		currentX += Input.GetAxis("Mouse X");
		currentY -= Input.GetAxis("Mouse Y");
		currentY = Mathf.Clamp(currentY, YAngleMin, YAngleMax);

		deltaTime = Time.deltaTime;
		lerpX = Mathf.Lerp(lerpX, currentX, deltaTime * sensitivityX);
		lerpY = Mathf.Lerp(lerpY, currentY, deltaTime * sensitivityY);
	}

	private void LateUpdate()
	{
		Vector3 dir = new Vector3(0,0,-distance);
		Quaternion rotation = Quaternion.Euler(lerpY, lerpX, 0);
		transform.position = lookAt.position + (rotation * dir);
		transform.LookAt(lookAt.position);
	}
}
