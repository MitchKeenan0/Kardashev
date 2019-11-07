using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public AudioClip boostSound;
	public Transform boostParticles;
	public float moveSpeed = 1.0f;
	public float moveAcceleration = 1.0f;
	public float maxSpeed = 10.0f;
	public float jumpSpeed = 1.0f;
	public float gravity = 9.8f;
	public float airControl = 1f;
	public float boostScale = 5.0f;
	public float boostFalloff = 3f;
	public float boostCooldown = 1.5f;
	public Vector3 moveCommand = Vector3.zero;
	public Vector3 impactMovement = Vector3.zero;

	private CharacterController controller;
	private AudioSource audioSoc;
	private PlayerBody body;
	private Vehicle vh;
	private float moveScale = 1f;
	private float currentForward = 0;
	private float currentLateral = 0;
	private float lastForward = 0;
	private float lastLateral = 0;
	private float timeBoostedLast = 0f;
	private Vector3 motion = Vector3.zero;
	private Vector3 motionRaw = Vector3.zero;
	private Vector3 boostMotion = Vector3.zero;
	private Vector3 jumpMotion = Vector3.zero;
	private bool bActive = true;
	private bool bInputEnabled = true;
	private bool bGrappling = false;
	private bool bInVehicle = false;
	private float grappleSpeed = 0f;
	
	public bool IsRiding()
	{
		return bInVehicle;
	}
	public Vehicle GetVehicle()
	{
		return vh;
	}
	public void SetVehicle(Vehicle value)
	{
		vh = value;
		if (value != null)
		{
			bInVehicle = true;
		}
		else
		{
			bInVehicle = false;
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

	public void SetMoveCommand(Vector3 value, bool bOverride)
	{
		if (bInVehicle)
		{
			vh.SetMoveCommand(value, bOverride);
		}
		else
		{
			if (bOverride)
			{
				moveCommand = value;
			}
			else
			{
				moveCommand += value;
			}
		}
	}

	public void SetGrappling(bool value, float topSpeed)
	{
		bGrappling = value;
		grappleSpeed = topSpeed;
	}

	public void SetActive(bool value)
	{
		bActive = value;

		if (!bActive)
		{
			currentForward = 0;
			currentLateral = 0;
			lastForward = 0;
			lastLateral = 0;
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
		Time.timeScale = 1f;

		Cursor.visible = false;

		controller = GetComponent<CharacterController>();
		audioSoc = GetComponent<AudioSource>();
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

				if (vh != null)
				{
					vh.SetMoveInput(currentForward, currentLateral);
					if (Input.GetButtonDown("Jump")){
						vh.JumpVehicle();
					}
				}
				else
				{
					if (bActive)
					{
						UpdateBoost();
						UpdateMovement();
					}
				}
			}
			else
			{
				currentForward = 0f;
				currentLateral = 0f;
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
		if ((Input.GetButtonDown("Boost") || (Input.GetButtonDown("Jump") && !controller.isGrounded))
			&& (boostMotion.magnitude <= 1f))
		{
			Boost();
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
				audioSoc.PlayOneShot(boostSound);
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

				boostMotion = (boostRaw * boostScale);
				timeBoostedLast = Time.time;
			}
		}
	}


	void UpdateMovement()
	{
		// Reading movement Input
		motionRaw = moveScale * ((Camera.main.transform.forward * currentForward)
			+ (Camera.main.transform.right * currentLateral)).normalized;
		motionRaw.y = 0f;
		Vector3 movementVector = motionRaw * (maxSpeed + grappleSpeed);
		// Acceleration for mid-air and grounded
		float accelerationScalar = moveAcceleration;
		if (!controller.isGrounded){
			accelerationScalar *= 0.1f;
		}
		else{
			accelerationScalar *= 10f;
		}
		motion = Vector3.Lerp(motion, movementVector, Time.deltaTime * accelerationScalar);
		// Jumping and other forces
		if (controller.isGrounded || bGrappling){
			if (Input.GetButtonDown("Jump")){
				jumpMotion = Vector3.up * jumpSpeed;
			}
		}
		if (!controller.isGrounded){
			jumpMotion = Vector3.Lerp(jumpMotion, Vector3.zero, Time.smoothDeltaTime * gravity);
			motion += (Vector3.down * gravity);
		}
		motion += jumpMotion;
		motion += moveCommand;
		motion += boostMotion;
		motion += impactMovement;
		controller.Move(motion * Time.smoothDeltaTime * Time.timeScale);
	}
}
