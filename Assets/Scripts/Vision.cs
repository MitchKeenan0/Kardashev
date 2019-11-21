using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vision : MonoBehaviour
{
	public float fieldOfView = 70.0f;
	public float recognitionRange = 1000f;
	public int penetrativePower = 0;

	private Transform visionTarget;


	public void SetVisionTarget(Transform target)
	{
		visionTarget = target;
	}

    public bool CheckLineOfSight(Transform eyeTransform)
	{
		bool bTargetSpotted = false;
		Vector3 toTarget = eyeTransform.position + (eyeTransform.forward * recognitionRange);
		if (visionTarget != null)
			toTarget = visionTarget.position - eyeTransform.position;
		float angle = Vector3.Angle(eyeTransform.forward, toTarget);
		float distance = toTarget.magnitude;
		if (angle <= fieldOfView 
			&& distance <= recognitionRange)
		{
			Vector3 origin = eyeTransform.position;
			Vector3 direction = toTarget;
			RaycastHit visionHit;
			if (Physics.Raycast(origin, direction, out visionHit))
			{
				if (visionHit.transform == visionTarget)
				{
					Character possibleCharacter = visionTarget.GetComponent<Character>();
					if (!possibleCharacter || (possibleCharacter && possibleCharacter.IsAlive()))
					bTargetSpotted = true;
				}
			}
		}

		return bTargetSpotted;
	}
}
