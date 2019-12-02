using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicLimbCoord : MonoBehaviour
{
	public float balanceForce = 150f;
	public float footForce = 15f;
	private Rigidbody rb;
	private PhysicBody physicBody;
	private RaycastHit groundHit;
	private RaycastHit[] groundHitsAll;
	private List<Limb> limbs;
	private Foot[] feet;
	private Foot steppingFoot;
	private Vector3 balanceVector;
	private float bodyAltitude = 0f;
	private float groundAngle = 0f;
	private bool bStepping;

	void Start()
    {
		rb = GetComponent<Rigidbody>();
		physicBody = GetComponent<PhysicBody>();
		limbs = new List<Limb>();
		Limb[] arrayLimbs = GetComponentsInChildren<Limb>();
		foreach (Limb lm in arrayLimbs)
			limbs.Add(lm);
		feet = GetComponentsInChildren<Foot>();
    }
    
    void Update()
    {
		UpdateGroundHit();
    }

	private void FixedUpdate()
	{
		BalanceToFurthestLimb();
		UpdateSteppingFoot();
	}

	void BalanceToFurthestLimb()
	{
		Limb furthestLimb = GetFurthestLimb();
		if (furthestLimb != null)
		{
			Vector3 toFurthestLimb = (furthestLimb.transform.position - transform.position) + (transform.forward);
			toFurthestLimb.y = 0f;
			balanceVector = toFurthestLimb * balanceForce * Time.fixedDeltaTime;
			rb.AddForce(balanceVector);

			Debug.Log("toFurthestLimb: " + toFurthestLimb.magnitude);
			if ((toFurthestLimb.magnitude > 1f)
				&& !bStepping)
			{
				AssignFootToBalance();
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
		Vector3 reverseBalance = balanceVector * -1f;
		float furthestFootDistance = 0f;
		foreach (Foot ft in feet){
			Vector3 toFoot = (reverseBalance - ft.transform.position);
			toFoot.y = 0f;
			float distToFoot = toFoot.magnitude;
			if (distToFoot > furthestFootDistance)
			{
				furthestFootDistance = distToFoot;
				steppingFoot = ft;
				bStepping = true;
				steppingFoot.BeginStep();
			}
		}
	}

	void UpdateSteppingFoot()
	{
		if (steppingFoot != null)
		{
			Vector3 reverseBalance = balanceVector * -1f;
			steppingFoot.GetComponent<Rigidbody>().AddForce(reverseBalance * footForce);
			Debug.DrawRay(steppingFoot.transform.position, reverseBalance, Color.green);
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

	public void SetStepping(bool bValue)
	{
		bStepping = bValue;
		steppingFoot = null;
	}
}
