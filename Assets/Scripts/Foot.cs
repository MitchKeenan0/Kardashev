using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Foot : MonoBehaviour
{
	public float footSpeed = 10f;
	public float downForce = 100f;
	public float groundDrag = 1000f;
	public float airDrag = 1f;
	private Rigidbody rb;
	private PhysicLimbCoord limbCoord;
	private RaycastHit[] groundCheck;
	private Vector3 surfaceNormal;
	private Vector3 stepTargetPosition;
	private Vector3 toStep;
	private bool bStepping = false;
	private bool bGrounded = false;
	private float timeAtStepStart = 0f;
	private float altitude;

    void Start()
    {
		rb = GetComponent<Rigidbody>();
		limbCoord = GetComponentInParent<PhysicLimbCoord>();
		surfaceNormal = Vector3.up;
    }

	void Update()
	{
		GetHeightFromGround();
	}

	void FixedUpdate()
	{
		if (bGrounded)
		{
			if (bStepping)
			{
				toStep = (stepTargetPosition - transform.position).normalized;
				toStep.y = transform.position.y;
				rb.AddForce(toStep * footSpeed);
				Debug.DrawRay(transform.position, toStep * footSpeed, Color.green);
				float distToStep = Vector3.Distance(transform.position, stepTargetPosition);
				if (distToStep < 10f)
				{
					EndStep();
				}
			}
			else
			{
				rb.AddForce(Vector3.down * downForce);
				Debug.DrawRay(transform.position, (Vector3.down * downForce), Color.red);
			}
		}
	}

	public void BeginStep(Vector3 stepPosition)
	{
		bStepping = true;
		timeAtStepStart = Time.time;
		stepTargetPosition = transform.position + stepPosition;
		stepTargetPosition.y = transform.position.y;
		rb.drag = airDrag;
	}

	void EndStep()
	{
		Debug.Log(transform.name + " Ending step");
		bStepping = false;
		rb.drag = groundDrag;
	}

	public bool IsStepping()
	{
		return bStepping;
	}

	void GetHeightFromGround()
	{
		groundCheck = Physics.RaycastAll(transform.position, Vector3.down * 9999f);
		if (groundCheck.Length > 0)
		{
			foreach (RaycastHit hit in groundCheck)
			{
				if ((hit.transform != transform)
					&& !hit.transform.IsChildOf(limbCoord.transform))
				{
					altitude = hit.distance;
					bGrounded = hit.distance < 1f;
				}
			}
		}
	}
}
