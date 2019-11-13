﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingTool : Tool
{
	public Transform mockTransform;
	public Transform firePoint;
	public Transform throwingPrefab;
	public Transform throwParticles;
	public float throwPower = 50f;
	public float chargeScale = 1.618f;
	public float maxCharge = 5f;
	public float aimSpeed = 5f;
	public float throwCooldown = 1f;
	public int throwCost = 1;
	public int reserveAmmo = 5;
	public bool bImpartThrowerVelocity = false;

	private Character player;
	private AudioSource audioSoc;
	private Animator animator;
	private EquippedInfo hudInfo;
	private HUD hud;
	private IEnumerator recoverCoroutine;
	private float timeAtTriggerDown = 0f;
	private float timeAtRelease = 0f;
	private bool bCharging = false;
	private bool bAnotherThrowing = false;
	private bool bAltScoping = false;

	private Vector3 lerpAimVector;
	private Vector3 targetVector;

	public override void InitTool(Transform value)
	{
		base.InitTool(value);

		if (animator != null)
		{
			animator.Play("SpearIdle");
		}

		player = owner.GetComponent<Character>();
	}

	void Start()
    {
		animator = GetComponent<Animator>();
		hudInfo = FindObjectOfType<EquippedInfo>();
		hud = FindObjectOfType<HUD>();
		targetVector = lerpAimVector = transform.forward;
	}

	void Update()
	{
		UpdateAiming();

		if (bCharging)
		{
			if (hud != null)
			{
				float currentCharge = Mathf.Clamp((Time.time - timeAtTriggerDown), 0f, maxCharge);
				hud.SetThrowingChargeValue(currentCharge);
			}

			if ((Time.time - timeAtTriggerDown) > 1f)
			{
				if (player != null)
				{
					player.SetScoped(true, 0.6f);
				}
			}
		}
	}

	public override void SetToolActive(bool value)
	{
		base.SetToolActive(value);

		// Trigger down to wind up
		if (value)
		{
			BeginThrowCharge();
		}

		if (Time.time > (timeAtRelease + throwCooldown))
		{
			// Trigger down to release
			if (!value && bCharging && (reserveAmmo > 0))
			{
				FireThrowingTool();
				bAnotherThrowing = false;
			}
		}

		// Store input for another throwing
		else if (value)
		{
			bAnotherThrowing = true;
		}

		if (!value)
		{
			if (player != null)
			{
				player.SetScoped(false, 1f);
			}
		}
	}

	public override void SetToolAlternateActive(bool value)
	{
		base.SetToolAlternateActive(value);

		if (value)
		{
			if (bCharging)
			{
				CancelCharge();
			}
			else
			{
				if (player != null)
				{
					bAltScoping = true;
					player.SetScoped(true, 1.6f);
				}
			}
		}
		else if (bAltScoping)
		{
			if (player != null)
			{
				bAltScoping = false;
				player.SetScoped(false, 1.6f);
			}
		}
	}

	void BeginThrowCharge()
	{
		timeAtTriggerDown = Time.time;
		bCharging = true;

		if (animator != null)
		{
			animator.Play("SpearWindup");
		}

		hud.SetThrowingChargeActive(true);
	}

	void CancelCharge()
	{
		bCharging = false;

		if (animator != null)
		{
			animator.Play("SpearIdle");
		}

		hud.SetThrowingChargeValue(0f);
		hud.SetThrowingChargeActive(false);
	}

	public EquippedInfo GetHudInfo()
	{
		return hudInfo;
	}

	void UpdateAiming()
	{
		lerpAimVector = transform.position + (Camera.main.transform.forward * 100f);
		float dotToTarget = aimSpeed / Mathf.Abs(Vector3.Dot(transform.forward, lerpAimVector.normalized));
		targetVector = Vector3.Lerp(targetVector, lerpAimVector, Time.deltaTime * aimSpeed * dotToTarget);
		transform.LookAt(targetVector);
	}

	void FireThrowingTool()
	{
		foreach (Renderer r in mockTransform.GetComponentsInChildren<Renderer>())
		{
			r.enabled = false;
		}

		bCharging = false;
		float chargePower = Mathf.Clamp((Time.time - timeAtTriggerDown) * chargeScale, 1f, maxCharge);
		Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0f);
		Vector3 fireVelocity = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)).direction * (throwPower * chargePower);

		if (bImpartThrowerVelocity)
		{
			Vector3 throwerVelocity = owner.GetComponent<CharacterController>().velocity;
			fireVelocity += throwerVelocity;
		}

		firePoint.position = mockTransform.position;
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

		hud.SetThrowingChargeValue(0f);
		hud.SetThrowingChargeActive(false);
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
