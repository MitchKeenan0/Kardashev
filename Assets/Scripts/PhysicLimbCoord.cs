using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicLimbCoord : MonoBehaviour
{
	public float strideSize = 10f;
	public float balanceForce = 150f;
	public float footForce = 15f;
	public float fallRecoveryTime = 1f;
	private Rigidbody rb;
	private PhysicBody physicBody;
	private RaycastHit groundHit;
	private RaycastHit[] groundHitsAll;
	private List<Limb> limbs;
	private List<LimbMember> limbMembers;
	private Foot[] feet;
	private Foot steppingFoot;
	private Vector3 balanceVector;
	private float bodyAltitude = 0f;
	private float groundAngle = 0f;
	private bool bStepping = false;
	private bool bFallen = false;
	private bool bCanFall = true;
	private int footIndex = 0;
	private IEnumerator fallRecoverCoroutine;
	private IEnumerator recoverGraceCotoutine;

	void Start()
    {
		rb = GetComponent<Rigidbody>();
		physicBody = GetComponent<PhysicBody>();
		limbs = new List<Limb>();
		limbMembers = new List<LimbMember>();
		Limb[] arrayLimbs = GetComponentsInChildren<Limb>();
		foreach (Limb lm in arrayLimbs)
			limbs.Add(lm);
		LimbMember[] arrayMems = GetComponentsInChildren<LimbMember>();
		foreach (LimbMember lm in arrayMems)
			limbMembers.Add(lm);
		feet = GetComponentsInChildren<Foot>();
    }
    
    void Update()
    {
		UpdateGroundHit();
    }

	private void FixedUpdate()
	{
		if (bodyAltitude <= 20f)
		{
			BalanceToFurthestLimb();
		}
	}

	void BalanceToFurthestLimb()
	{
		Limb furthestLimb = GetFurthestLimb();
		if (furthestLimb != null)
		{
			Vector3 toFurthestLimb = (furthestLimb.transform.position - transform.position) + (transform.forward);
			toFurthestLimb.y = 0f;

			Vector3 bodyTiltVector = (transform.position + (transform.up * -physicBody.height)) - transform.position;
			bodyTiltVector.y = 0f;

			balanceVector = bodyTiltVector.normalized * balanceForce * Time.fixedDeltaTime;
			rb.AddForce(balanceVector);
			Debug.DrawRay(rb.position, balanceVector, Color.blue);

			// Foot catching
			if (toFurthestLimb.magnitude > 10f)
			{
				AssignFootToBalance();
			}

			// Fallen over
			if (bCanFall)
			{
				float angleToStraight = Vector3.Angle(rb.transform.up, Vector3.up);
				if ((angleToStraight > 30f)
					 || (toFurthestLimb.magnitude > 15f))
				{
					SetFallen(true);
				}
			}
		}
	}

	Limb GetFurthestLimb()
	{
		Limb furthestLimb = limbs[0];
		float outerDist = 0f;
		foreach (Limb lm in limbs)
		{
			Vector3 toLimbFlat = (lm.transform.position - transform.position);
			toLimbFlat.y = 0f;
			float distToLimb = toLimbFlat.magnitude;
			if (distToLimb > outerDist)
			{
				furthestLimb = lm;
				outerDist = distToLimb;
			}
		}
		return furthestLimb;
	}

	void AssignFootToBalance()
	{
		Vector3 reverseBalance = balanceVector.normalized * -1;
		if (feet[footIndex] != null)
		{
			steppingFoot = feet[footIndex];
			if (!steppingFoot.IsStepping())
				steppingFoot.BeginStep(reverseBalance * strideSize);
		}

		footIndex++;
		if (footIndex >= feet.Length)
		{
			footIndex = 0;
		}
	}

	void UpdateGroundHit()
	{
		Vector3 rayStart = transform.position;
		Vector3 rayDirection = rayStart + (transform.up * -1000f);
		groundHitsAll = Physics.RaycastAll(rayStart, rayDirection);
		if (groundHitsAll.Length > 0){
			foreach (RaycastHit grHit in groundHitsAll){
				if ((grHit.transform != transform)
					&& !grHit.transform.IsChildOf(transform))
				{
					groundHit = grHit;
					bodyAltitude = grHit.distance;
					groundAngle = Mathf.Acos(Vector3.Dot(Vector3.up, transform.forward));
				}
			}
		}
	}

	void SetFallen(bool value)
	{
		bFallen = value;
		physicBody.SetFallen(value);
		foreach (LimbMember lm in limbMembers)
		{
			lm.SetFallen(value);
		}

		if (value)
		{
			fallRecoverCoroutine = RecoverFall(fallRecoveryTime);
			StartCoroutine(fallRecoverCoroutine);
		}
	}

	IEnumerator RecoverFall(float recoverTime)
	{
		yield return new WaitForSeconds(recoverTime);
		bCanFall = false;
		SetFallen(false);
		recoverGraceCotoutine = RecoveryGrace(3f);
		StartCoroutine(recoverGraceCotoutine);
	}

	IEnumerator RecoveryGrace(float graceTime)
	{
		yield return new WaitForSeconds(graceTime);
		bCanFall = true;
	}
}
