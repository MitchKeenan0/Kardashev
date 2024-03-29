﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : Tool
{
	public Transform hookHeadPrefab;
	public Transform firePoint;
	public Transform fireParticles;
	public Transform impactParticles;
	public Transform reelingParticles;
	public Transform detachParticles;
	public Transform recoveryParticles;
	public float shotSpeed = 100f;
	public float reelSpeed = 10f;
	public float aimSpeed = 5000f;
	public float tightness = 0.5f;

	private LineRenderer line;
	private ConfigurableJoint joint;
	private Transform hookTransform;
	private Bullet hookBullet;
	private Character player;
	private Rigidbody playerRb;
	private GrappleBullet grapp;

	private Vector3 hitLocation;
	private Vector3 lastHookPosition;
	private RaycastHit[] gunRaycastHits;
	private RaycastHit grappleHit;
	private RaycastHit[] headRaycastHits;

	private float reelLengthRemaining = 0f;
	private bool bHitscanning = false;
	private bool bHookOut = false;
	private bool bLatchedOn = false;
	private bool bHookRecover = false;
	private bool bReeling = false;

	private IEnumerator nearMissCoroutine; // come back to this later

	public override void InitTool(Transform value)
	{
		base.InitTool(value);

		player = value.GetComponent<Character>();
		playerRb = value.GetComponent<Rigidbody>();

		if (hookTransform == null)
			hookTransform = Instantiate(hookHeadPrefab, firePoint.position, Quaternion.identity);
		hookTransform.parent = firePoint;
		hookTransform.localPosition = Vector3.zero;
		hookTransform.rotation = firePoint.rotation;
		hookBullet = hookTransform.GetComponent<Bullet>();
		hookBullet.enabled = false;
		grapp = hookTransform.gameObject.GetComponent<GrappleBullet>();
		grapp.SetReelActiveEffects(false);
		hookTransform.gameObject.SetActive(true);

		bHookOut = false;
		bHookRecover = false;
		bHitscanning = false;
		bReeling = false;
	}

	void Start()
    {
		line = GetComponent<LineRenderer>();
		line.enabled = false;
	}

    void Update()
    {
		if (!bHookOut)
		{
			hookTransform.position = firePoint.position;
		}

		if (!bLatchedOn && bHitscanning && bHookOut)
		{
			RaycastForGrapplePoint();
		}

		if (bLatchedOn && (hookTransform.parent != null))
		{
			ConstrainPlayer();
		}

		if (bHookRecover)
		{
			RecoverHook();
		}

		if (bHookOut && bReeling && (hookTransform.parent != null))
		{
			ReelPlayer();
		}
	}

	private void LateUpdate()
	{
		if (bHookOut)
		{
			UpdateLine();
		}
	}

	void ConstrainPlayer()
	{
		float distance = Vector3.Distance(owner.position, hookTransform.position);
		if ((distance > 5f) && (distance > (reelLengthRemaining + 1f)))
		{
			Vector3 toConstraint = (hookTransform.position - owner.position).normalized;
			float beyondTolerance = distance - reelLengthRemaining;
			player.SetMoveCommand(toConstraint * tightness * beyondTolerance, true);
		}
		else
		{
			player.SetMoveCommand(Vector3.zero, true);
		}
	}

	void RaycastForGrapplePoint()
	{
		Vector3 deltaRay = (hookTransform.forward * hookBullet.bulletSpeed * Time.smoothDeltaTime * 5f);
		Vector3 origin = hookTransform.position + (deltaRay * -0.1f);
		gunRaycastHits = Physics.RaycastAll(origin, deltaRay, deltaRay.magnitude);
		int numHits = gunRaycastHits.Length;
		for (int i = 0; i < numHits; i++)
		{
			if (!bLatchedOn)
			{
				RaycastHit thisHit = gunRaycastHits[i];
				if (!thisHit.collider.isTrigger)
				{
					Transform hitTransform = thisHit.transform;
					if (hitTransform != owner)
					{
						// Check against back hits due to raycast offsetting ^^
						Vector3 camPosition = Camera.main.transform.position;
						Vector3 camForward = Camera.main.transform.forward;
						Vector3 toHit = (thisHit.point - camPosition).normalized;
						float dotToHit = Vector3.Dot(camForward, toHit);
						if (dotToHit > 0.1f)
						{
							RegisterHit(thisHit.transform.gameObject, thisHit.point);
							bHitscanning = false;
						}
					}
				}
			}
		}
	}

	void FireGrapplingHook()
	{
		hookTransform.parent = firePoint;
		hookTransform.localPosition = Vector3.zero;
		hookTransform.localRotation = Quaternion.identity;
		hookTransform.position = firePoint.position;
		hookTransform.rotation = firePoint.rotation;
		hookTransform.parent = null;

		Transform newFireParticles = Instantiate(fireParticles, firePoint.position, firePoint.rotation);
		Destroy(newFireParticles.gameObject, 5f);

		bHookOut = true;
		bHitscanning = true;
		bHookRecover = false;
		bLatchedOn = false;

		hookBullet.enabled = true;
		hookBullet.AddSpeedModifier(shotSpeed, transform, owner);
	}

	void DeactivateGrapplingHook()
	{
		hookTransform.localScale = Vector3.one;
		hookBullet.AddSpeedModifier(0f, transform, owner);
		line.enabled = false;
		bHitscanning = false;
		bLatchedOn = false;
		bHookRecover = true;
		player.SetGrappling(false, 0f);

		// Detach effects
		if ((hookTransform.parent != null)
			&& (hookTransform.parent != firePoint))
		{
			if (detachParticles != null)
			{
				Transform detachEffects = Instantiate(detachParticles, hookTransform.position, Quaternion.identity);
				Destroy(detachEffects.gameObject, 1f);
			}
		}
	}

	void RecoverHook()
	{
		float distToRecovery = Vector3.Distance(hookTransform.position, firePoint.position);
		if (distToRecovery > 10f)
		{
			// Counteracting Lerp's tailing-off with increasing strength
			float lerpSmoother = Mathf.Clamp((350f / distToRecovery), 1f, 1000f);

			// Bit of gravity
			Vector3 hookVelocity = hookBullet.GetDeltaVector() * Time.smoothDeltaTime;
			hookVelocity.z = 0f;
			hookVelocity.x = 0f;

			hookTransform.position = Vector3.Lerp(hookTransform.position, firePoint.position, Time.smoothDeltaTime * 2f*(shotSpeed * lerpSmoother));
		}
		else
		{
			// Hook Recovered
			DockGrappler();

			if (recoveryParticles != null)
			{
				Transform recoveryEffect = Instantiate(recoveryParticles, firePoint.position, Quaternion.identity);
				Destroy(recoveryEffect.gameObject, 1f);
			}
		}
	}

	public void RegisterHit(GameObject hitObj, Vector3 hitPosition)
	{
		if (!bLatchedOn)
		{
			hookBullet = hookTransform.GetComponent<Bullet>();
			hookBullet.AddSpeedModifier(0f, transform, owner);
			hookBullet.enabled = false;
			line.enabled = true;

			if (impactParticles != null)
			{
				Transform impactEffect = Instantiate(impactParticles, hookTransform.position, Quaternion.identity);
				Destroy(impactEffect.gameObject, 2f);
			}

			hookTransform.parent = hitObj.transform;
			hookTransform.position = hitPosition;

			reelLengthRemaining = Vector3.Distance(hookBullet.transform.position, playerRb.transform.position);

			bLatchedOn = true;
			player.SetGrappling(true, reelSpeed);
		}
	}

	void ReelPlayer()
	{
		if (player != null)
		{
			Vector3 toHookFull = hookTransform.position - playerRb.transform.position;
			Vector3 toHookNormal = toHookFull.normalized;
			Vector3 velocity = playerRb.velocity.normalized;
			
			// Little boost to keep us moving along the ground
			if (playerRb != null)
			{
				if (player.IsGrounded() && !player.IsRiding())
				{
					toHookNormal += Vector3.up;
				}
			}

			// Close-in nice and easy
			Vector3 reelingMotion = toHookNormal * reelSpeed;
			if (toHookFull.magnitude <= 100f)
			{
				reelingMotion *= (toHookFull.magnitude / 100f);
			}

			// Move component will parse foot vs traffix
			player.SetMoveCommand(reelingMotion, true);

			reelLengthRemaining = (hookTransform.position - playerRb.transform.position).magnitude;
		}
	}

	public void DeactivateGrappler()
	{
		DockGrappler();
		hookTransform.gameObject.SetActive(false);
		player.SetMoveCommand(Vector3.zero, true);
	}

	public void DockGrappler()
	{
		line.SetPosition(0, transform.position);
		line.SetPosition(1, transform.position);
		line.enabled = false;

		hookBullet.AddSpeedModifier(0f, transform, owner);
		hookBullet.SetLifetime(0f);
		hookBullet.enabled = false;

		hookTransform.parent = firePoint;
		hookTransform.localPosition = Vector3.zero;
		hookTransform.rotation = firePoint.rotation;
		hookTransform.localScale = Vector3.one;
		lastHookPosition = hookTransform.position;

		bHookOut = false;
		bHookRecover = false;
		bHitscanning = false;
		bReeling = false;
	}

	void DeactivateReel()
	{
		bReeling = false;
		player.SetMoveCommand(Vector3.zero, true);
	}

	void UpdateLine()
	{
		line.SetPosition(0, firePoint.position);
		line.SetPosition(1, hookTransform.position);
	}

	public override void SetToolActive(bool value)
	{
		base.SetToolActive(value);

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

		// Reel is called in Update
		bReeling = value;

		if (!value)
		{
			DeactivateReel();
		}

		if ((bHookOut && (hookTransform.parent != null))
			|| (value == false))
		{
			grapp.SetReelActiveEffects(value);
		}
	}

	public bool IsHookOut()
	{
		return bHookOut;
	}

	public bool IsReeling()
	{
		return bReeling;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!bLatchedOn)
		{
			Debug.Log("Triggered");
			hookBullet.AddSpeedModifier(0f, transform, owner);
			RaycastForGrapplePoint();
		}
	}

}
