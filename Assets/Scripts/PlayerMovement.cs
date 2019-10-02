using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
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
		moveCommand = value;

		if (bOverrideVelocity)
		{
			moveCommand += (-controller.velocity * Time.smoothDeltaTime);
		}

		// Gravity
		if (moveCommand != Vector3.zero)
		{
			moveCommand += Vector3.up * -gravity * Time.smoothDeltaTime;
		}
	}

	public void AddMoveCommand(Vector3 value)
	{
		moveCommand += value;
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
			moveCommand = Vector3.zero;
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
		Application.targetFrameRate = 70;

		controller = GetComponent<CharacterController>();
		body = GetComponent<PlayerBody>();
	}


	void Update()
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

		UpdateBoost();

		if (bActive)
		{
			UpdateMovement();
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


	void UpdateBoost()
	{
		// New boost to be fed into UpdateMovement
		if (Input.GetButtonDown("Boost") && (controller.velocity.magnitude <= (maxSpeed * 1.1f)))
		{
			if (Time.time >= (timeBoostedLast + boostCooldown))
			{
				Vector3 boostRaw = ((Camera.main.transform.forward * currentForward)
				+ (transform.right * currentLateral)).normalized;
				boostRaw.y *= -0.15f;   ///boostRaw.y = 0f;

				Vector3 currentV = controller.velocity;
				Vector3 normalV = currentV.normalized;
				Vector3 normalB = boostRaw.normalized;
				float lateralDot = Vector3.Dot(normalV, normalB);
				if (lateralDot < 0f)
				{
					boostRaw.x += ((currentV.x * lateralDot) * 2 * Time.smoothDeltaTime);
					boostRaw.z += ((currentV.z * lateralDot) * 2 * Time.smoothDeltaTime);
				}

				boostMotion = (boostRaw * boostScale);
				timeBoostedLast = Time.time;
			}
		}

		// Graceful end-of-Boost
		if (boostMotion.magnitude > 0f)
		{
			boostMotion = Vector3.Lerp(boostMotion, Vector3.zero, Time.smoothDeltaTime * boostFalloff);
		}
	}


	void UpdateMovement()
	{
		// Decelleration
		if ((currentForward == 0.0f) && (currentLateral == 0.0f))
		{
			motionRaw = -controller.velocity * decelSpeed;
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
			// Ground control and jumping
			if (Input.GetButtonDown("Jump"))
			{
				motion.y = jumpSpeed;
			}

			// Interp pass for 'smooth moves'
			motion = Vector3.Lerp(motion, motionRaw * maxSpeed, Time.smoothDeltaTime * moveAcceleration);
			
			// Clamp Max Speed if not boosting
			if (boostMotion.magnitude < 1f)
			{
				motion = Vector3.ClampMagnitude(motion, maxSpeed);
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

		if (bActive)
		{
			controller.Move(motion * Time.smoothDeltaTime);
		}
	}
}
