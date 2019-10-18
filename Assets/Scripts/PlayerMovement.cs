using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public Transform boostParticles;
	public float moveSpeed = 1.0f;
	public float moveAcceleration = 1.0f;
	public float maxSpeed = 10.0f;
	public float decelSpeed = 1.0f;
	public float jumpSpeed = 1.0f;
	public float gravity = 9.8f;
	public float airControl = 1f;
	public float boostScale = 5.0f;
	public float boostFalloff = 3f;
	public float boostCooldown = 1.5f;
	public Vector3 moveCommand = Vector3.zero;

	private CharacterController controller;
	private PlayerBody body;
	private Vehicle ride;
	private float moveScale = 1f;
	private float currentForward = 0;
	private float currentLateral = 0;
	private float lastForward = 0;
	private float lastLateral = 0;
	private float timeBoostedLast = 0f;
	private Vector3 motion = Vector3.zero;
	private Vector3 motionRaw = Vector3.zero;
	private Vector3 boostMotion = Vector3.zero;
	private bool bActive = true;
	private bool bInputEnabled = true;
	private bool bGrappling = false;
	private bool bInVehicle = false;
	private float grappleSpeed = 0f;

	// Used for transitioning in/out of vehicles
	public void SetInVehicle(bool value, Vehicle vehicle)
	{
		bInVehicle = value;

		if (bInVehicle)
		{
			ride = vehicle;
			
			transform.parent = vehicle.footMountTransform;
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			body.SetBodyOffset(Vector3.up * (controller.height * 0.5f));

			SetActive(false);
		}
		else
		{
			if (transform.parent != null)
			{
				// Distancing from vehicle for clean exit
				Vector3 offset = (Camera.main.transform.position - transform.position).normalized;
				transform.position += offset * 3f;
				transform.parent = null;
			}

			body.SetBodyOffset(Vector3.zero);

			SetActive(true);
		}
	}


	public float GetForward()
	{
		return currentForward;
	}
	public float GetLateral()
	{
		return currentLateral;
	}

	public void SetMoveCommand(Vector3 value, bool bOverrideVelocity)
	{
		if (bOverrideVelocity)
		{
			moveCommand = value;
		}
		else
		{
			moveCommand += value;
		}
	}

	public void SetGrappling(bool value, float speed)
	{
		bGrappling = value;
		grappleSpeed = speed;
	}

	public void SetActive(bool value)
	{
		bActive = value;
		motionRaw = Vector3.zero;

		if (!bActive)
		{
			currentForward = 0;
			currentLateral = 0;
			lastForward = 0;
			lastLateral = 0;
			motion = Vector3.zero;
			motionRaw = Vector3.zero;
			//moveCommand = Vector3.zero;
		}
	}

	public void EnableInput(bool value)
	{
		bInputEnabled = value;
	}

	public void SetMoveScale(float value)
	{
		moveScale = value;
	}


	void Start()
	{
		//Application.targetFrameRate = 99;

		Cursor.visible = false;

		controller = GetComponent<CharacterController>();
		body = GetComponent<PlayerBody>();
	}


	void Update()
	{
		if (Time.timeScale > 0f)
		{
			lastForward = currentForward;
			lastLateral = currentLateral;

			if (bInputEnabled)
			{
				currentForward = Input.GetAxisRaw("Vertical");
				currentLateral = Input.GetAxisRaw("Horizontal");
			}
			else
			{
				currentForward = 0f;
				currentLateral = 0f;
			}

			if (!bInVehicle)
			{
				UpdateBoost();

				if (bActive)
				{
					UpdateMovement();
				}
			}
			else
			{
				if (ride != null)
				{
					ride.SetMoveInput(currentForward, currentLateral);
				}

				if (Input.GetButtonDown("Jump"))
				{
					ride.JumpVehicle();
				}
			}

			// Inform body for rotations
			if (currentForward != lastForward)
			{
				body.SetForward(currentForward);
			}
			if (currentLateral != lastLateral)
			{
				body.SetLateral(currentLateral);
			}
		}
	}


	void SpawnBoost()
	{
		Transform newBoost = Instantiate(boostParticles, transform.position, Quaternion.Euler(controller.velocity));
		newBoost.parent = Camera.main.transform;
		newBoost.localPosition = Vector3.forward * 1.5f;
		Destroy(newBoost.gameObject, 3f);
	}


	void UpdateBoost()
	{
		if (Input.GetButtonDown("Boost") || (Input.GetButtonDown("Jump") && !controller.isGrounded))
		{
			if (boostMotion.magnitude <= 1f)
			{
				Boost();
			}
		}

		// Graceful end-of-Boost
		if (boostMotion.magnitude > 0f)
		{
			boostMotion = Vector3.Lerp(boostMotion, Vector3.zero, Time.smoothDeltaTime * boostFalloff);
		}
	}

	void Boost()
	{
		// New boost to be fed into UpdateMovement
		float topSpeed = (maxSpeed + jumpSpeed);
		if (bGrappling)
		{
			topSpeed += grappleSpeed;
		}

		if (controller.velocity.magnitude <= topSpeed)
		{
			if ((Time.time >= (timeBoostedLast + boostCooldown)) && ((currentForward != 0f) || (currentLateral != 0f)))
			{
				SpawnBoost();

				Vector3 boostRaw = ((Camera.main.transform.forward * currentForward)
				+ (transform.right * currentLateral)).normalized;

				boostRaw.y *= -0.1f;

				Vector3 currentV = controller.velocity;
				Vector3 normalV = currentV.normalized;
				Vector3 normalB = boostRaw.normalized;
				float lateralDot = Vector3.Dot(normalV, normalB);
				if (lateralDot < 0f)
				{
					boostRaw.x += ((currentV.x * -10f) * Time.smoothDeltaTime);
					boostRaw.z += ((currentV.z * -10f) * Time.smoothDeltaTime);
				}

				boostMotion = (boostRaw * boostScale) + (Vector3.up * -gravity);
				timeBoostedLast = Time.time;
			}
		}
	}


	void UpdateMovement()
	{
		// Decelleration
		if ((currentForward == 0.0f) && (currentLateral == 0.0f))
		{
			motionRaw = motion * decelSpeed;
		}
		else
		{
			// Acceleration
			motionRaw = ((Camera.main.transform.forward * currentForward)
				+ (transform.right * currentLateral)).normalized;
		}

		motionRaw *= moveScale;

		if (!controller.isGrounded)
		{
			motionRaw *= airControl;
		}
		else
		{
			// Interp pass for 'smooth moves'
			motion = Vector3.Lerp(motion, motionRaw * maxSpeed, Time.smoothDeltaTime * moveAcceleration);

			// Clamp Max Speed if not boosting
			if (boostMotion.magnitude < 1f)
			{
				motion = Vector3.ClampMagnitude(motion, maxSpeed);
			}
		}

		// Jump
		if (controller.isGrounded || bGrappling)
		{
			// Ground control and jumping
			if (Input.GetButtonDown("Jump"))
			{
				motion.y = jumpSpeed;
			}
		}

		// Exterior forces
		motion += moveCommand;

		if (boostMotion.magnitude > 1f)
		{
			motion += boostMotion * Time.smoothDeltaTime;
		}
		else
		{
			// Gravity
			motion.y -= gravity * Time.smoothDeltaTime;
		}

		if (bActive && !bInVehicle)
		{
			controller.Move(motion * Time.smoothDeltaTime);
		}
	}
}
