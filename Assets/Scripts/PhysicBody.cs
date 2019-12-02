using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicBody : MonoBehaviour
{
	public Transform head;
	public float height = 3f;
	public float buoyancy = 5f;
	public float internalAngularDrag = 1f;
	private Rigidbody rb;
	private Rigidbody headRb;
	private PhysicLimbCoord limbCoord;
	private RaycastHit[] groundCheck;
	private float altitude = 0f;
	private Rigidbody affectedPart;
	private Vector3 externalForce;

    void Start()
    {
		rb = GetComponent<Rigidbody>();
		limbCoord = GetComponent<PhysicLimbCoord>();
		headRb = head.GetComponent<Rigidbody>();
		rb.angularDrag = internalAngularDrag;
		Rigidbody[] internalRbs = GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody r in internalRbs)
		{
			r.angularDrag = internalAngularDrag;
		}
    }

    void Update()
    {
		GetHeightFromGround();
    }

	void FixedUpdate()
	{
		HoldAloft();
		ExternalForces();
	}

	void GetHeightFromGround()
	{
		groundCheck = Physics.RaycastAll(transform.position, Vector3.down * 999f);
		if (groundCheck.Length > 0)
		{
			foreach(RaycastHit hit in groundCheck)
			{
				if ((hit.transform != transform)
					&& !hit.transform.IsChildOf(transform))
				{
					altitude = hit.distance;
				}
			}
		}
	}

	void HoldAloft()
	{
		if (altitude < height)
		{
			float differential = height - altitude;
			Vector3 upForce = Vector3.up * 100f * differential * buoyancy;
			headRb.AddForce(upForce);
		}
	}

	void ExternalForces()
	{
		if (affectedPart != null)
		{
			affectedPart.AddForce(externalForce);
			externalForce = Vector3.Lerp(externalForce, Vector3.zero, Time.fixedDeltaTime);
		}
	}

	public void TakeHitTo(Vector3 force, Transform bodyPart)
	{
		if (bodyPart.GetComponent<Rigidbody>())
		{
			affectedPart = bodyPart.GetComponent<Rigidbody>();
			externalForce = force;
		}
	}
}
