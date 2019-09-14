using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Limb : MonoBehaviour
{
	public Vector3 limbRotationAxis;
	public bool bOffsetLimb = true;
	public float maxAngle = 45f;
	public float limbSpeed = 1f;
	public float periodSpeed = 1f;
	public int oppositionOffset;

	//private float currentAngle = 0f;
	private float anglingSpeed;
	private bool bActive = false;
	
	//private int period = 1;

	public void SetLimbActive(bool value, int oppositionValue)
	{
		bActive = value;
		oppositionOffset = oppositionValue;

		anglingSpeed = limbSpeed;
		if (bOffsetLimb && (oppositionOffset == 0))
		{
			anglingSpeed = -limbSpeed;
		}
	}

	void Start()
    {
        
    }

	void Update()
	{
		if (bActive)
		{
			UpdateLimb();
		}
	}

	void UpdateLimb()
	{
		// Motion
		Vector3 sweepingVector = limbRotationAxis * Mathf.Sin(Time.time * periodSpeed) * anglingSpeed;

		// Clamp rotation
		sweepingVector.x = Mathf.Clamp(sweepingVector.x, -maxAngle, maxAngle);
		sweepingVector.y = Mathf.Clamp(sweepingVector.y, -maxAngle, maxAngle);
		sweepingVector.z = Mathf.Clamp(sweepingVector.z, -maxAngle, maxAngle);

		// & set
		transform.localRotation = Quaternion.Euler(sweepingVector);
	}
}
