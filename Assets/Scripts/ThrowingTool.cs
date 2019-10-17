using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingTool : Tool
{
	public Transform mockTransform;
	public Transform firePoint;
	public Transform throwingPrefab;
	public float throwPower = 50f;
	public float chargeScale = 1.618f;
	public float maxCharge = 5f;
	public float throwCooldown = 1f;
	public int throwCost = 1;
	public int reserveAmmo = 5;
	public bool bImpartThrowerVelocity = false;

	private Animator animator;
	private EquippedInfo hudInfo;
	private IEnumerator recoverCoroutine;
	private float timeAtTriggerDown = 0f;
	private float timeAtRelease = 0f;
	private bool bCharging = false;
	private bool bAnotherThrowing = false;

	public EquippedInfo GetHudInfo()
	{
		return hudInfo;
	}


	public override void InitTool(Transform value)
	{
		base.InitTool(value);

		if (animator != null)
		{
			animator.Play("SpearIdle");
		}
	}


	public override void SetToolActive(bool value)
	{
		base.SetToolActive(value);

		if (Time.time > (timeAtRelease + throwCooldown))
		{
			// Trigger down to wind up
			if (value)
			{
				BeginThrowCharge();
			}
			
			// Trigger down to release
			else if (bCharging && (reserveAmmo > 0))
			{
				FireThrowingTool();
				bAnotherThrowing = false;
			}
		}
		else if (value)
		{
			// Store input for another throwing
			bAnotherThrowing = true;
		}
	}


	public override void SetToolAlternateActive(bool value)
	{
		base.SetToolAlternateActive(value);
		
	}

	void Start()
    {
		animator = GetComponent<Animator>();
		hudInfo = FindObjectOfType<EquippedInfo>();
    }


	void BeginThrowCharge()
	{
		timeAtTriggerDown = Time.time;
		bCharging = true;

		if (animator != null)
		{
			animator.Play("SpearWindup");
		}
	}

    
	void FireThrowingTool()
	{
		foreach (Renderer r in mockTransform.GetComponentsInChildren<Renderer>())
		{
			r.enabled = false;
		}

		bCharging = false;
		float chargePower = Mathf.Clamp((Time.time - timeAtTriggerDown) * chargeScale, 1f, maxCharge);
		Vector3 fireVelocity = Camera.main.transform.forward * (throwPower * chargePower);

		// Adjust for firepoint offset by raycasting
		RaycastHit aimHit;
		Vector3 start = Camera.main.transform.position;
		Vector3 end = start + Camera.main.transform.forward * 99999f;
		if (Physics.Raycast(start, end, out aimHit))
		{
			Vector3 throwVelocity = (aimHit.point - firePoint.position).normalized;
			fireVelocity = throwVelocity * (throwPower * chargePower);
		}

		if (bImpartThrowerVelocity)
		{
			Vector3 throwerVelocity = owner.GetComponent<CharacterController>().velocity;
			fireVelocity += throwerVelocity;
		}

		Transform newThrowingTransform = Instantiate(throwingPrefab, firePoint.position, firePoint.rotation);
		Rigidbody throwingRb = newThrowingTransform.GetComponent<Rigidbody>();
		throwingRb.velocity = fireVelocity;
		timeAtRelease = Time.time;

		if (newThrowingTransform.GetComponent<Spear>())
		{
			newThrowingTransform.GetComponent<Spear>().InitSpear(this, chargePower);
		}

		if (throwCost != 0)
		{
			reserveAmmo -= throwCost;
			if (hudInfo != null)
			{
				hudInfo.SetToolReserve(reserveAmmo.ToString());
			}
		}

		if (reserveAmmo > 0)
		{
			recoverCoroutine = RecoverMock(throwCooldown);
			StartCoroutine(recoverCoroutine);
		}
	}


	IEnumerator RecoverMock(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);

		foreach (Renderer r in mockTransform.GetComponentsInChildren<Renderer>())
		{
			r.enabled = true;
		}
		
		if (bAnotherThrowing)
		{
			BeginThrowCharge();
		}
		else if (animator != null)
		{
			animator.Play("SpearIdle");
		}
	}

	public void RecoverMockFast()
	{
		foreach (Renderer r in mockTransform.GetComponentsInChildren<Renderer>())
		{
			r.enabled = true;
		}

		if (animator != null)
		{
			animator.Play("SpearIdle");
		}
	}


}
