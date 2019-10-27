using System.Collections;
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
	public float tightness = 0.5f;

	private LineRenderer line;
	private ConfigurableJoint joint;
	private Transform hookTransform;
	private Bullet hookBullet;
	private PlayerMovement movement;
	private CharacterController controller;

	private Vector3 hitLocation;
	private Vector3 lastHookPosition;
	private Vector3 lerpAimVector;
	private Vector3 targetVector;
	private Vector3 flightVector;
	private RaycastHit[] gunRaycastHits;
	private RaycastHit grappleHit;
	private RaycastHit[] headRaycastHits;

	private float reelLengthRemaining = 0f;
	private bool bHitscanning = false;
	private bool bHookOut = false;
	private bool bLatchedOn = false;
	private bool bHookRecover = false;
	private bool bReeling = false;

	// Used when player is riding a vehicle
	public void SetControllerComponent(CharacterController value)
	{
		controller = value;
	}

	public override void InitTool(Transform value)
	{
		base.InitTool(value);

		if (hookTransform == null)
		{
			movement = value.GetComponent<PlayerMovement>();
			controller = value.GetComponent<CharacterController>();

			hookTransform = Instantiate(hookHeadPrefab, firePoint.position, Quaternion.identity);
			hookTransform.parent = firePoint;
			hookTransform.localPosition = Vector3.zero;
			hookTransform.rotation = firePoint.rotation;
			hookBullet = hookTransform.GetComponent<Bullet>();
			hookBullet.enabled = false;
		}

		hookTransform.gameObject.SetActive(true);
		bHookOut = false;
		bHookRecover = false;
		bHitscanning = false;
		bReeling = false;
	}


	public override void SetToolActive(bool value)
	{
		base.SetToolActive(value);

		DeactivateReel();

		if (value)
		{
			if (!bHookRecover && !bHookOut)
			{
				bHitscanning = true;
			}
		}
		else
		{
			bHitscanning = false;
			DeactivateGrapplingHook();
		}
	}


	public override void SetToolAlternateActive(bool value)
	{
		base.SetToolAlternateActive(value);

		bReeling = value;

		if (!value)
		{
			DeactivateReel();
		}

		movement.SetGrappling(value, reelSpeed);
	}

	public bool IsHookOut()
	{
		return bHookOut;
	}

	public bool IsReeling()
	{
		return bReeling;
	}


	void Start()
    {
		line = GetComponent<LineRenderer>();
		targetVector = lerpAimVector = transform.forward;
		flightVector = firePoint.forward * shotSpeed;
	}


    void Update()
    {
		UpdateAiming();

		if (bHitscanning && !bHookOut)
		{
			RaycastForGrapplePoint();
		}

        if (bHookOut)
		{
			UpdateLine();

			if (!bLatchedOn)
			{
				UpdateHookFlight();
			}

			if (!bReeling && (hookTransform.parent != null))
			{
				ConstrainPlayer();
			}
		}

		if (bHookRecover)
		{
			RecoverHook();
		}

		if ((bHookOut && bReeling) && (hookTransform.parent != null))
		{
			ReelPlayer();
		}
	}


	void ConstrainPlayer()
	{
		float distance = Vector3.Distance(controller.transform.position, hookBullet.transform.position);
		if (distance > (reelLengthRemaining + 0.1f))
		{
			Vector3 toConstraint = hookBullet.transform.position - controller.transform.position;
			float beyondTolerance = distance - reelLengthRemaining;
			Vector3 constrain = toConstraint * tightness * beyondTolerance * Time.smoothDeltaTime;
			if (movement.IsRiding())
			{
				movement.GetVehicle().SetMoveCommand(constrain, true);
			}
			else
			{
				movement.SetMoveCommand(constrain, true);
			}
		}
		else
		{
			if (!movement.IsRiding())
				movement.SetMoveCommand(Vector3.zero, true);
			else
				movement.GetVehicle().SetMoveCommand(Vector3.zero, true);
		}

		float currentDistance = Vector3.Distance(owner.position, hookBullet.transform.position);
		if (currentDistance < reelLengthRemaining)
		{
			reelLengthRemaining = currentDistance;
		}
	}


	void RaycastForGrapplePoint()
	{
		gunRaycastHits = Physics.RaycastAll(firePoint.position, firePoint.forward * range);
		int numHits = gunRaycastHits.Length;
		for (int i = 0; i < numHits; i++)
		{
			RaycastHit thisHit = gunRaycastHits[i];
			if (!thisHit.collider.isTrigger)
			{
				Transform hitTransform = thisHit.transform;
				if (hitTransform != owner)
				{
					grappleHit = thisHit;
					FireGrapplingHook(grappleHit);
				}
			}
		}
	}


	void FireGrapplingHook(RaycastHit hit)
	{
		hookTransform.parent = null;

		hookBullet.enabled = true;
		hookBullet.AddSpeedModifier(shotSpeed, transform, owner);

		bHookOut = true;
		bHookRecover = false;
		bLatchedOn = false;

		line.enabled = true;

		movement.SetGrappling(true, reelSpeed);
	}

	void UpdateHookFlight()
	{
		Vector3 toHit = (grappleHit.point - hookBullet.transform.position).normalized;
		float distToHit = Vector3.Distance(hookBullet.transform.position, grappleHit.point);
		float dotToHit = Vector3.Dot(toHit, hookBullet.transform.forward);
		bool bHit = (distToHit < 3f) || (dotToHit <= 0f);
		if (bHit)
		{
			RegisterHit(grappleHit.transform.gameObject, grappleHit.point);
		}

		Debug.Log("Dist: " + distToHit + "  dot: " + dotToHit + " at " + Time.time);
	}


	void DeactivateGrapplingHook()
	{
		hookBullet.AddSpeedModifier(0f, transform, owner);

		// Detach effects
		if ((hookTransform.parent != null)
			&& (hookTransform.parent != firePoint))
		{
			hookTransform.parent = null;
			hookTransform.localScale = Vector3.one;

			if (detachParticles != null)
			{
				Transform detachEffects = Instantiate(detachParticles, hookTransform.position, Quaternion.identity);
				Destroy(detachEffects.gameObject, 1f);
			}
		}

		bLatchedOn = false;
		bHookRecover = true;
		movement.SetGrappling(false, 0f);
	}


	void RecoverHook()
	{
		float distToRecovery = Vector3.Distance(hookTransform.position, firePoint.position);
		if (distToRecovery > 5f)
		{
			// Counteracting Lerp's tailing-off with increasing strength
			float lerpSmoother = Mathf.Clamp((range / distToRecovery), 1f, 1000f);

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
		hookBullet = hookTransform.GetComponent<Bullet>();
		hookBullet.AddSpeedModifier(0f, transform, owner);

		if (impactParticles != null)
		{
			Transform impactEffect = Instantiate(impactParticles, hookTransform.position, Quaternion.identity);
			Destroy(impactEffect.gameObject, 2f);
		}

		hookTransform.parent = hitObj.transform;
		hookTransform.position = hitPosition;

		reelLengthRemaining = Vector3.Distance(hookBullet.transform.position, owner.position);

		bLatchedOn = true;
	}


	void ReelPlayer()
	{
		if (movement != null)
		{
			Vector3 toHookFull = hookBullet.transform.position - controller.transform.position;
			Vector3 toHookNormal = toHookFull.normalized;
			Vector3 velocity = controller.velocity.normalized;
			
			// Little boost to keep us moving along the ground
			if (controller != null)
			{
				if (controller.isGrounded && !movement.IsRiding())
				{
					toHookNormal += Vector3.up;
				}
			}

			// Movement and updating new constraint length
			Vector3 reelingMotion = toHookNormal * reelSpeed;
			if (movement.IsRiding()){
				movement.GetVehicle().SetMoveCommand(reelingMotion, true);
			}
			else{
				movement.SetMoveCommand(reelingMotion, true);
			}

			reelLengthRemaining = (hookBullet.transform.position - controller.transform.position).magnitude;
		}
	}

	public void DeactivateGrappler()
	{
		DockGrappler();
		hookTransform.gameObject.SetActive(false);
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
		lastHookPosition = transform.position;

		bHookOut = false;
		bHookRecover = false;
		bHitscanning = false;
		bReeling = false;
	}


	void DeactivateReel()
	{
		bReeling = false;
		if (movement.GetVehicle())
		{
			movement.GetVehicle().SetMoveCommand(Vector3.zero, false);
		}
		else
		{
			movement.SetMoveCommand(Vector3.zero, false);
		}
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

		targetVector = Vector3.Lerp(targetVector, lerpAimVector, Time.smoothDeltaTime * aimSpeed * dotToTarget);
		transform.LookAt(targetVector);
	}


}
