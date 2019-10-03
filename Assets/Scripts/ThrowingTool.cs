using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingTool : Tool
{
	public Transform mockTransform;
	public Transform firePoint;
	public Transform throwingPrefab;
	public float throwPower;
	public float throwCooldown = 1f;

	private Animator animator;
	private IEnumerator recoverCoroutine;
	private float timeAtTriggerDown = 0f;
	private float timeAtRelease = 0f;
	private bool bCharging = false;
	private bool bAnotherThrowing = false;

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
			else if (bCharging)
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
		float chargePower = Mathf.Clamp((Time.time - timeAtTriggerDown) * 1.5f, 1.5f, 5f);
		Vector3 fireVelocity = (Camera.main.transform.forward * (throwPower * chargePower));

		Vector3 transformOffset = owner.position - transform.position;
		fireVelocity += transformOffset;

		//Vector3 ownerVelocity = owner.GetComponent<CharacterController>().velocity * 0.15f;
		//fireVelocity += ownerVelocity;

		Transform newThrowingTransform = Instantiate(throwingPrefab, firePoint.position, firePoint.rotation);

		Rigidbody throwingRb = newThrowingTransform.GetComponent<Rigidbody>();
		throwingRb.velocity = fireVelocity;

		timeAtRelease = Time.time;

		recoverCoroutine = RecoverMock(throwCooldown);
		StartCoroutine(recoverCoroutine);
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


}
