using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Foot : MonoBehaviour
{
	private Rigidbody rb;
	private PhysicLimbCoord limbCoord;
	private Vector3 surfaceNormal;
	private bool bStepping = false;
	private float timeAtStepStart = 0f;

    void Start()
    {
		rb = GetComponent<Rigidbody>();
		limbCoord = GetComponentInParent<PhysicLimbCoord>();
		surfaceNormal = Vector3.up;
    }

	public void BeginStep()
	{
		bStepping = true;
		timeAtStepStart = Time.time;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (bStepping && 
			!collision.transform.IsChildOf(transform.parent)
			&& ((Time.time - timeAtStepStart) > 0.5f))
		{
			limbCoord.SetStepping(false);
			bStepping = false;
			Debug.Log("Stepped");
		}
	}
}
