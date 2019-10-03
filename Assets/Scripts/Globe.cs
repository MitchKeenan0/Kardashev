using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globe : MonoBehaviour
{
	public float rotationSpeed = 0.15f;
	public Transform pitchFrame;
	public Transform globe;
	
	private Vector3 residentV = Vector3.zero;
	private Quaternion lastRotation;
	private bool bSpinningEnabled = false;
	

    void Start()
    {
		lastRotation = transform.rotation;
		transform.eulerAngles = new Vector3(0.0f, Random.Range(-3f, 3f), 0.0f);
		residentV = Vector3.right * 1.0f;
	}

	void Update()
	{
		if (!bSpinningEnabled && (residentV.magnitude > 0.0f))
		{
			WatchResidualSpin();
		}
	}



	public void ToggleSpinning(bool value)
	{
		bSpinningEnabled = value;
	}


	public void RotateGlobe(Vector3 deltaV)
	{
		residentV = Vector3.Lerp(residentV, deltaV, Time.deltaTime);

		Vector3 rotationEuler = Vector3.zero;
		rotationEuler.y = -residentV.x;
		rotationEuler.x = residentV.y;

		RotateLateralFrame(rotationEuler.y);
		RotatePitchFrame(residentV.y);
	}


	public void RotateLateralFrame(float rotationDirection)
	{
		float angle = rotationDirection * rotationSpeed;
		transform.Rotate(Vector3.up, angle, Space.World);
	}

	public void RotatePitchFrame(float rotationDirection)
	{
		float angle = rotationDirection * rotationSpeed;
		Vector3 pitchRotation = pitchFrame.rotation.eulerAngles;
		pitchFrame.Rotate(new Vector3(Mathf.Clamp(angle, -70, 70), 0, 0), Space.World);
	}


	void WatchResidualSpin()
	{
		residentV = Vector3.Lerp(residentV, Vector3.zero, Time.deltaTime * 2.0f);
		RotateGlobe(residentV);
	}
}
