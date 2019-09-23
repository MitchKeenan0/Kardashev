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
	private RaycastHit hit;
	private Vector3 offset;
	private bool bActive = true;
	private float currentX;
	private float currentY;
	private float deltaTime;
	private float lerpX = 0f;
	private float lerpY = 0f;
	private float collisionShrink = 0f;

	public void SetActive(bool value)
	{
		bActive = value;
	}

	public void SetCollisionShrink(float value, bool bCumulative)
	{
		if (bCumulative)
		{
			collisionShrink += value;
		}
		else
		{
			collisionShrink = value;
		}
	}


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
		if (bActive)
		{
			// Taking input from mouse..
			currentX += Input.GetAxis("Mouse X");
			currentY -= Input.GetAxis("Mouse Y");
			currentY = Mathf.Clamp(currentY, YAngleMin, YAngleMax);

			deltaTime = Time.smoothDeltaTime;
			lerpX = Mathf.Lerp(lerpX, currentX, deltaTime * sensitivityX);
			lerpY = Mathf.Lerp(lerpY, currentY, deltaTime * sensitivityY);

			WallCheck();
		}
	}

	void LateUpdate()
	{
		if (bActive)
		{
			// Rotation & positioning
			Vector3 dir = new Vector3(0, 0, -distance + collisionShrink);
			Quaternion rotation = Quaternion.Euler(lerpY, lerpX, 0);

			// Camera position
			Vector3 targetVector = lookAt.position + (rotation * dir);
			Vector3 lerpPosition;
			lerpPosition.x = Mathf.Lerp(transform.position.x, targetVector.x, Time.smoothDeltaTime * lagSpeed * 0.2f);
			lerpPosition.y = Mathf.Lerp(transform.position.y, targetVector.y, Time.smoothDeltaTime * lagSpeed);
			lerpPosition.z = Mathf.Lerp(transform.position.z, targetVector.z, Time.smoothDeltaTime * lagSpeed * 0.2f);
			transform.position = lerpPosition;

			// camera Rotation
			Vector3 lookVector = lookAt.position;
			lookVector.y = lookAt.position.y + (currentY * Time.smoothDeltaTime);
			transform.LookAt(lookVector);
		}
	}

	void WallCheck()
	{
		if (Physics.Linecast(lookAt.position, transform.position, out hit))
		{
			float rawDistance = Vector3.Distance(transform.position, hit.point);
			if (rawDistance > (collisionShrink + 0.1f))
			{
				collisionShrink = Mathf.Lerp(collisionShrink, rawDistance, Time.deltaTime);
			}
		}
		else
		{
			collisionShrink = Mathf.Lerp(collisionShrink, 0f, Time.deltaTime * 2f);
		}
	}
}
