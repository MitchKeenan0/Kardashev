﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : Tool
{
	public Transform hookHeadPrefab;
	public Transform firePoint;
	public Transform impactParticles;
	public Transform detachParticles;
	public Transform recoveryParticles;
	public float range = 100f;
	public float shotSpeed = 100f;
	public float reelSpeed = 10f;
	public float gravity = 2f;
	public float aimSpeed = 5000f;

	private LineRenderer line;
	private Transform hookTransform;
	private Bullet hookBullet;
	private PlayerMovement movement;

	private Vector3 hitLocation;
	private Vector3 lastHookPosition;
	private Vector3 lerpAimVector;
	private Vector3 targetVector;
	private Vector3 flightVector;
	private RaycastHit[] gunRaycastHits;
	private RaycastHit[] headRaycastHits;

	private float reelLengthRemaining = 0f;
	private bool bHookOut = false;
	private bool bHookRecover = false;
	private bool bReeling = false;


	public override void InitTool(Transform value)
	{
		base.InitTool(value);

		movement = value.GetComponent<PlayerMovement>();
	}


	public override void SetToolActive(bool value)
	{
		base.SetToolActive(value);

		DeactivateReel();

		if (value)
		{
			if (!bHookRecover && !bHookOut)
			{
				FireGrapplingHook();
			}
		}
		else
		{
			DeactivateGrapplingHook();
		}
	}


	public override void SetToolAlternateActive(bool value)
	{
		base.SetToolAlternateActive(value);

		bReeling = value;

		if (!bReeling)
		{
			DeactivateReel();
		}
	}


	void Start()
    {
		line = GetComponent<LineRenderer>();

		hookTransform = Instantiate(hookHeadPrefab, firePoint.position, Quaternion.identity);
		hookTransform.parent = firePoint;
		hookTransform.localPosition = Vector3.zero;
		hookBullet = hookTransform.GetComponent<Bullet>();

		targetVector = lerpAimVector = transform.forward;
		flightVector = firePoint.forward * shotSpeed;
	}


    void Update()
    {
		UpdateAiming();

        if (bHookOut) {
			UpdateLine();
		}

		if (bHookRecover) {
			RecoverHook();
		}

		if ((bHookOut && bReeling) && (hookTransform.parent != null)) {
			ReelPlayer();
		}
	}


	void FireGrapplingHook()
	{
		hookTransform.parent = null;
		hookTransform.position = firePoint.position;
		hookTransform.rotation = firePoint.rotation;

		hookBullet.AddSpeedModifier(shotSpeed, transform, owner);

		bHookOut = true;
		bHookRecover = false;

		line.enabled = true;
	}


	void DeactivateGrapplingHook()
	{
		hookBullet.AddSpeedModifier(0f, transform, owner);

		// Detach effects
		if (hookTransform.parent != null)
		{
			if (detachParticles != null)
			{
				Transform detachEffects = Instantiate(detachParticles, hookTransform.position, Quaternion.identity);
				Destroy(detachEffects.gameObject, 1f);
			}
		}

		hookTransform.parent = null;
		
		bHookRecover = true;
	}


	void RecoverHook()
	{
		float distToRecovery = Vector3.Distance(hookTransform.position, firePoint.position);
		if (distToRecovery >= 1f)
		{
			// Counteracting Lerp's tailing-off with increasing strength
			float lerpSmoother = Mathf.Clamp((range / distToRecovery), 1f, 1000f);

			// Bit of gravity
			Vector3 hookVelocity = hookBullet.GetDeltaVector() * Time.deltaTime;
			hookVelocity.z = 0f;
			hookVelocity.x = 0f;

			hookTransform.position = Vector3.Lerp(hookTransform.position, firePoint.position, Time.deltaTime * (shotSpeed * lerpSmoother));
			if (hookTransform.position.y < firePoint.transform.position.y)
			{
				hookTransform.position += hookVelocity;
			}
		}
		else
		{
			// Hook Recovered
			line.SetPosition(0, transform.position);
			line.SetPosition(1, transform.position);
			line.enabled = false;

			hookTransform.parent = firePoint;
			hookTransform.localPosition = Vector3.zero;
			hookBullet.AddSpeedModifier(0f, transform, owner);

			DeactivateReel();

			if (recoveryParticles != null)
			{
				Transform recoveryEffect = Instantiate(recoveryParticles, firePoint.position, Quaternion.identity);
				Destroy(recoveryEffect.gameObject, 1f);
			}

			bHookOut = false;
			bHookRecover = false;
		}
	}


	public void RegisterHit(GameObject hitObj, Vector3 hitPosition)
	{
		hookBullet = hookTransform.GetComponent<Bullet>();
		hookBullet.AddSpeedModifier(0f, transform, owner);

		if (impactParticles != null)
		{
			Transform impactEffect = Instantiate(impactParticles, hookTransform.position, Quaternion.identity);
			Destroy(impactEffect.gameObject, 2f);
		}

		hookTransform.parent = hitObj.transform;
	}


	void ReelPlayer()
	{
		if (movement != null)
		{
			Vector3 toHook = (hookBullet.transform.position - movement.gameObject.transform.position).normalized;

			movement.SetMoveCommand(toHook * Time.deltaTime * reelSpeed);

			reelLengthRemaining = Vector3.Distance(firePoint.position, hookBullet.transform.position);
			if (reelLengthRemaining <= 3f)
			{
				DeactivateReel();
			}
		}
	}


	void DeactivateReel()
	{
		bReeling = false;
		movement.SetMoveCommand(Vector3.zero);
	}


	void UpdateLine()
	{
		line.SetPosition(0, firePoint.position);
		line.SetPosition(1, hookTransform.position);
	}


	void UpdateAiming()
	{
		lerpAimVector = transform.position + (Camera.main.transform.forward * 100f);

		float dotToTarget = aimSpeed / Mathf.Abs(Vector3.Dot(transform.forward, lerpAimVector.normalized));

		targetVector = Vector3.Lerp(targetVector, lerpAimVector, Time.deltaTime * aimSpeed * dotToTarget);
		transform.LookAt(targetVector);
	}


}
