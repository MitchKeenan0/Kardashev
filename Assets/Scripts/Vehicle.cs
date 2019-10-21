﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
	public Transform invitationText;
	public Transform effectsTransform;
	public Transform footMountTransform;
	public ParticleSystem groundParticles;
	public ParticleSystem thrustParticles;
	public float moveSpeed = 1f;
	public float acceleration = 10f;
	public float deceleration = 0.9f;
	public float maxSpeed = 100f;
	public float turnSpeed = 1f;
	public float turnAcceleration = 5f;
	public float surfaceTurnSpeed = 5f;
	public float gradeClimbSpeed = 0f;
	public float gravity = 9f;
	public float levitationRange = 5f;
	public float levitationSpeed = 1f;
	public float jumpSpeed = 10f;
	public Vector3 centerOfMass;
	public float maxAirTime = 5f;

	private CharacterController controller;
	private Rigidbody rb;
	private PlayerBody player;
	private RaycastHit downHit;
	private RaycastHit forwardHit;
	private Vector3 motion;
	private Vector3 interpNormal = Vector3.zero;
	private Quaternion inputRotation;
	private Quaternion surfaceNormal;
	private float forwardInput = 0f;
	private float lateralInput = 0f;
	private float lateralTurn = 0f;
	private float groundDistance = 0f;
	private float dynamicSurfacingSpeed = 1f;
	private bool bActive = false;


	public void SetVehicleActive(bool value)
	{
		bActive = value;

		// Getting in
		if (bActive)
		{
			Vector3 lastVelocity = rb.velocity;
			rb.isKinematic = true;
			controller.enabled = true;
			controller.Move(lastVelocity);
			invitationText.gameObject.SetActive(false);
			player.SetThirdPerson(true);
			effectsTransform.gameObject.SetActive(true);
		}

		// Getting out
		else
		{
			Vector3 lastVelocity = controller.velocity;
			controller.enabled = false;
			
			motion = Vector3.zero;
			effectsTransform.gameObject.SetActive(false);
			if ((player != null) && (Vector3.Distance(transform.position, player.transform.position) <= 10f))
			{
				invitationText.gameObject.SetActive(true);

				player.SetThirdPerson(false);
			}

			EnableGroundEffects(false);

			rb.isKinematic = false;
		}
	}


	public void SetMoveInput(float forward, float lateral)
	{
		forwardInput = forward;
		lateralInput = lateral;
	}


	public void JumpVehicle()
	{
		float jump = jumpSpeed;
		if (!controller.isGrounded && (groundDistance > levitationRange))
		{
			jump *= 0.1f;
		}
		motion.y += jump;
	}


    void Start()
    {
		controller = GetComponent<CharacterController>();
		controller.enabled = false;
		rb = GetComponent<Rigidbody>();
		rb.centerOfMass = centerOfMass;
		rb.inertiaTensor = Vector3.one * 0.1f;

		invitationText.gameObject.SetActive(false);
		effectsTransform.gameObject.SetActive(false);

		if (groundParticles != null)
		{
			var em = groundParticles.emission;
			em.enabled = false;
		}

		if (thrustParticles != null)
		{
			var em = thrustParticles.emission;
			em.enabled = false;
		}

		inputRotation = transform.rotation;
		surfaceNormal = transform.rotation;

		SetVehicleActive(false);
	}

	void Update()
	{
		if (bActive && Time.timeScale > 0f)
		{
			SurfaceRotations();

			UpdateMovement();

			InputRotations();
		}
	}

	private void LateUpdate()
	{
		if (bActive)
		{
			dynamicSurfacingSpeed = Mathf.Clamp(Mathf.Sqrt(controller.velocity.magnitude), turnAcceleration, turnSpeed);
			transform.rotation = Quaternion.Lerp(transform.rotation, (surfaceNormal * inputRotation), Time.smoothDeltaTime * turnSpeed * dynamicSurfacingSpeed);
		}
	}


	void SurfaceRotations()
	{
		float targetGrade = 1f;
		Vector3 surfaceNormalVector = Vector3.up;
		Vector3 downRay = (Vector3.down * 500f) + (controller.velocity * 6.18f);
		if (forwardInput != 0f)
		{
			downRay += (Vector3.down * 50f);
		}

		if (Physics.Raycast(transform.position, downRay, out downHit))
		{
			bool groundHit = !downHit.transform.gameObject.GetComponent<PlayerMovement>() && (downHit.transform.gameObject != gameObject) 
				&& (downHit.transform != transform) && !downHit.transform.gameObject.GetComponent<Vehicle>();
			if (groundHit)
			{
				surfaceNormalVector = downHit.normal;
				interpNormal = Vector3.Lerp(interpNormal, surfaceNormalVector, Time.smoothDeltaTime * surfaceTurnSpeed);
				if (controller.isGrounded)
				{
					targetGrade = Mathf.Pow(Mathf.Abs(Vector3.Dot(Vector3.up, downHit.normal)), 10f);
				} else {
					targetGrade = Mathf.Lerp(targetGrade, 1f, Time.smoothDeltaTime);
				}
				gradeClimbSpeed = Mathf.Lerp(gradeClimbSpeed, targetGrade, Time.smoothDeltaTime * acceleration);

				groundDistance = downHit.distance;
			}
		}

		surfaceNormal = Quaternion.FromToRotation(transform.up, interpNormal) * transform.rotation;
	}


	void UpdateMovement()
	{
		if (bActive && controller.enabled)
		{
			// Propulsion
			Vector3 forwardMovement = Vector3.zero;

			if (forwardInput == 0f && lateralInput == 0f)
			{
				var em = thrustParticles.emission;
				em.enabled = false;

				forwardInput = 0f;
			}

			// Thrust
			if (forwardInput > 0f)
			{
				forwardMovement = transform.forward * moveSpeed * maxSpeed * forwardInput * gradeClimbSpeed;
				var em = thrustParticles.emission;
				em.enabled = true;
			}
			else if (forwardInput < 0f)
			{
				// Brake /
				// Reverse
				forwardMovement = transform.forward * moveSpeed * maxSpeed * forwardInput * gradeClimbSpeed;
			}

			//float speedScalar = Mathf.Clamp(controller.velocity.magnitude * acceleration, 0.1f, turnSpeed);
			motion = Vector3.Lerp(motion, forwardMovement, Time.smoothDeltaTime * acceleration);


			// Levitation
			if (forwardInput != 0f)
			{
				float dist = groundDistance;
				if (dist <= levitationRange)
				{
					float scalar = Mathf.Clamp(controller.velocity.magnitude * Time.smoothDeltaTime, 0.01f, 1f);
					motion += Vector3.up * scalar * levitationSpeed;
					
					if (forwardInput != 0f)
					{
						EnableGroundEffects(true);
					}
				}
				else
				{
					EnableGroundEffects(false);
				}
			}
			else
			{
				EnableGroundEffects(false);
			}

			if (controller.isGrounded && (Vector3.Dot(Vector3.up, downHit.normal) < 0.1f))
			{
				// Drifting
				motion += (downHit.normal + (Vector3.up * -5f)).normalized;
			}

			// Gravity
			if (!controller.isGrounded)
			{
				motion += (Vector3.up * (-gravity * Time.smoothDeltaTime));
			}

			controller.Move(motion * Time.smoothDeltaTime);
		}
	}


	void InputRotations()
	{
		if (Time.timeScale == 1f)
		{
			if ((lateralInput == 0f) && (Mathf.Abs(lateralTurn) <= 0.05f))
			{
				lateralTurn = 0f;
			}
			else if (lateralTurn != lateralInput)
			{
				lateralTurn = Mathf.Lerp(lateralTurn, 2f * lateralInput, Time.smoothDeltaTime * turnAcceleration);
			}

			Quaternion turnRotation = Quaternion.AngleAxis(lateralTurn, transform.up);
			inputRotation = turnRotation;
		}
	}


	void EnableGroundEffects(bool value)
	{
		if (groundParticles != null)
		{
			var em = groundParticles.emission;
			em.enabled = value;

			if (value)
			{
				RaycastHit groundHit;
				Vector3 start = (transform.position) + (Vector3.down * 2f);
				if (Physics.Raycast(start, (Vector3.down * 100f), out groundHit))
				{
					groundParticles.transform.position = groundHit.point;
				}
			}
		}
	}


	private void OnTriggerEnter(Collider other)
	{
		if (!bActive)
		{
			if (other.transform.GetComponent<PlayerBody>())
			{
				player = other.transform.GetComponent<PlayerBody>();
				player.SetVehicle(this);
				invitationText.gameObject.SetActive(true);
			}
		}
	}


	private void OnTriggerExit(Collider other)
	{
		if (!bActive)
		{
			if (other.transform.GetComponent<PlayerBody>())
			{
				invitationText.gameObject.SetActive(false);
				player.SetVehicle(null);
			}
		}
	}

}
