﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

[AddComponentMenu("Camera-Control/Smooth Mouse Look")]
public class SmoothMouseLook : MonoBehaviour
{
	public Transform testSpherePrefab;
	public Transform body;
	public Transform cam;
	public Vector3 bodyOffset;
	public float eyeHeight = 0.7f;
	public float camChaseSpeed = 3f;
	public float fittingSpeed = 1f;

	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseXAndY;
	public float sensitivityX = 1f;
	public float sensitivityY = 1f;

	public float minimumX = -360f;
	public float maximumX = 360f;

	public float minimumY = -60f;
	public float maximumY = 60f;

	float rotationX = 0f;
	float rotationY = 0f;

	private List<float> rotArrayX = new List<float>();
	float rotAverageX = 0f;

	private List<float> rotArrayY = new List<float>();
	float rotAverageY = 0f;

	public float frameCounter = 20;
	public float distance = 0f;

	Quaternion originalRotation;
	RaycastHit[] blockingHits;
	public Slider sensitivitySlider;
	private float fittingTargetDistance = 0f;
	private float chaseSpeed;
	private float slowChaseSpeed;

	private PostProcessProfile postProcessProfile;
	DepthOfField dof;

	public void OptionsSensitivity(float value)
	{
		float clamped = Mathf.Clamp(value, 0.1f, 100f);
		sensitivityX = sensitivitySlider.value;
		sensitivityY = sensitivitySlider.value;
	}

	public void SetSensitivity(float value)
	{
		sensitivityX = value;
		sensitivityY = value;
	}

	public void SetBody(Transform value)
	{
		body = value;
	}

	public void SetOffset(Vector3 offset)
	{
		if (offset != bodyOffset)
		{
			bodyOffset = offset;

			if (distance == 0f)
			{
				distance = offset.z;
				fittingTargetDistance = offset.z;
			}

			if (offset == Vector3.zero)
			{
				distance = 0f;
				bodyOffset.y = eyeHeight;
			}

			cam.localPosition = bodyOffset;
		}
	}

	void Start()
	{
		Rigidbody rb = GetComponent<Rigidbody>();
		if (rb)
			rb.freezeRotation = true;
		originalRotation = transform.localRotation;

		if (FindObjectOfType<PostProcessVolume>())
		{
			postProcessProfile = FindObjectOfType<PostProcessVolume>().profile;
			if (postProcessProfile != null)
			{
				postProcessProfile.TryGetSettings(out dof);
			}
		}

		chaseSpeed = camChaseSpeed;
		slowChaseSpeed = camChaseSpeed * 0.1f;
		fittingTargetDistance = distance;

		InitCamera();
		SetOffset(bodyOffset);
	}

	void InitCamera()
	{
		if (body != null)
		{
			transform.position = body.position;
		}
	}

	void SetDepthOfField()
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.position, transform.forward * 5000f, out hit, 5000f))
		{
			if (dof != null && !hit.transform.CompareTag("Vehicle"))
			{
				dof.focusDistance.value = hit.distance;
			}
		}
	}
	
	void Update()
	{
		if (Time.timeScale != 0f)
		{
			// Camera control
			if (axes == RotationAxes.MouseXAndY)
			{
				rotAverageY = 0f;
				rotAverageX = 0f;

				rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
				rotationX += Input.GetAxis("Mouse X") * sensitivityX;

				rotArrayY.Add(rotationY);
				rotArrayX.Add(rotationX);

				if (rotArrayY.Count >= frameCounter)
				{
					rotArrayY.RemoveAt(0);
				}
				if (rotArrayX.Count >= frameCounter)
				{
					rotArrayX.RemoveAt(0);
				}

				for (int j = 0; j < rotArrayY.Count; j++)
				{
					rotAverageY += rotArrayY[j];
				}
				for (int i = 0; i < rotArrayX.Count; i++)
				{
					rotAverageX += rotArrayX[i];
				}

				rotAverageY /= rotArrayY.Count;
				rotAverageX /= rotArrayX.Count;

				rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);
				rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);

				Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
				Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);

				transform.rotation = originalRotation * xQuaternion * yQuaternion;
			}
			else if (axes == RotationAxes.MouseX)
			{
				rotAverageX = 0f;

				rotationX += Input.GetAxis("Mouse X") * sensitivityX;

				rotArrayX.Add(rotationX);

				if (rotArrayX.Count >= frameCounter)
				{
					rotArrayX.RemoveAt(0);
				}
				for (int i = 0; i < rotArrayX.Count; i++)
				{
					rotAverageX += rotArrayX[i];
				}
				rotAverageX /= rotArrayX.Count;

				rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);

				Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);
				transform.rotation = originalRotation * xQuaternion;
			}
			else
			{
				rotAverageY = 0f;

				rotationY += Input.GetAxis("Mouse Y") * sensitivityY;

				rotArrayY.Add(rotationY);

				if (rotArrayY.Count >= frameCounter)
				{
					rotArrayY.RemoveAt(0);
				}
				for (int j = 0; j < rotArrayY.Count; j++)
				{
					rotAverageY += rotArrayY[j];
				}
				rotAverageY /= rotArrayY.Count;

				rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);

				Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
				transform.rotation = originalRotation * yQuaternion;
			}

			//SetDepthOfField();

			if (distance != 0f)
			{
				UpdateBlocking();
			}

			//UpdateCameraPosition();
		}
	}

	private void LateUpdate()
	{
		UpdateCameraPosition();
	}

	void UpdateCameraPosition()
	{
		if (body != null)
		{
			if (Mathf.Abs(bodyOffset.z) > 0f)
			{
				transform.position = Vector3.Lerp(transform.position, body.position + Vector3.up, Time.smoothDeltaTime * camChaseSpeed);
			}
			else
			{
				transform.position = body.position + (Vector3.up * eyeHeight);
				cam.localPosition = Vector3.zero;
			}
		}
	}
	
	void UpdateBlocking()
	{
		float shortestCameraDistance = distance;
		float newCameraDistance = fittingTargetDistance;
		Vector3 camPos = transform.position + (transform.forward * distance);
		Vector3 bodyPos = body.position;
		Vector3 direction = (camPos - bodyPos).normalized * Mathf.Abs(distance);

		blockingHits = Physics.RaycastAll(bodyPos, direction, Mathf.Abs(distance));
		if (blockingHits.Length >= 1){
			foreach (RaycastHit hit in blockingHits){
				if ((hit.transform != transform)
					&& (hit.transform != body) 
					&& (!hit.transform.GetComponent<Vehicle>())
					&& (!hit.transform.GetComponent<Tool>()))
				{
					float testDistance = -Mathf.Clamp((hit.distance * 0.95f), 1f, Mathf.Abs(distance));
					if (testDistance > shortestCameraDistance)
					{
						shortestCameraDistance = testDistance;
						newCameraDistance = shortestCameraDistance * 0.5f;
					}
				}
			}
		}
		else
		{
			newCameraDistance = distance;
		}

		if (!Mathf.Approximately(newCameraDistance, fittingTargetDistance))
		{
			fittingTargetDistance = Mathf.Lerp(fittingTargetDistance, newCameraDistance, Time.smoothDeltaTime * fittingSpeed);
			Vector3 newOffset = bodyOffset;
			newOffset.z = fittingTargetDistance;
			SetOffset(newOffset);
		}
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		angle = angle % 360;
		if ((angle >= -360F) && (angle <= 360F))
		{
			if (angle < -360F)
			{
				angle += 360F;
			}
			if (angle > 360F)
			{
				angle -= 360F;
			}
		}
		return Mathf.Clamp(angle, min, max);
	}
}
