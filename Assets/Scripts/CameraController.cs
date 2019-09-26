using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public Transform lookAt;
	public bool bFirstPerson;
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

			if (!bFirstPerson)
			{
				WallCheck();
			}
		}
	}

	void LateUpdate()
	{
		if (bActive)
		{
			// Rotation & positioning
			Vector3 dir = new Vector3(0, 0, -distance);
			Quaternion rotation = Quaternion.Euler(lerpY, lerpX, 0);

			// Camera position
			Vector3 lerpPosition = lookAt.position;
			if (!bFirstPerson)
			{
				Vector3 targetVector = lookAt.position + (rotation * dir);

				targetVector += Camera.main.transform.forward * collisionShrink;
				
				lerpPosition.x = Mathf.Lerp(transform.position.x, targetVector.x, Time.smoothDeltaTime * lagSpeed * 0.2f);
				lerpPosition.y = Mathf.Lerp(transform.position.y, targetVector.y, Time.smoothDeltaTime * lagSpeed);
				lerpPosition.z = Mathf.Lerp(transform.position.z, targetVector.z, Time.smoothDeltaTime * lagSpeed * 0.2f);
			}
			transform.position = lerpPosition;

			// camera Rotation
			Vector3 lookVector = lookAt.position;
			if (bFirstPerson)
			{
				lookVector = rotation.eulerAngles;
			}

			transform.LookAt(lookVector);
		}
	}

	void WallCheck()
	{
		Vector3 behindYou = lookAt.position + (transform.forward * -distance * 1.1f);
		if (Physics.Linecast(lookAt.position, behindYou, out hit))
		{
			float rawDistance = Vector3.Distance(lookAt.position, hit.point);
			float processedDistance = 0f;
			if (rawDistance < distance)
			{
				processedDistance = Mathf.Clamp((rawDistance - distance) * -1f, 1f, 6f);
				collisionShrink = Mathf.Lerp(collisionShrink, processedDistance, Time.smoothDeltaTime*3f);
			}
		}
		else
		{
			collisionShrink = Mathf.Lerp(collisionShrink, 0f, Time.smoothDeltaTime*2f);
		}
	}
}
