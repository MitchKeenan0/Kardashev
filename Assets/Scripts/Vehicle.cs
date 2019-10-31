using System.Collections;
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
	private Vector3 rawMotion = Vector3.zero;
	private Vector3 movementVector = Vector3.zero;
	private Vector3 motion = Vector3.zero;
	private Vector3 interpNormal = Vector3.zero;
	private Vector3 moveCommand = Vector3.zero;
	private Quaternion inputRotation;
	private Quaternion surfaceNormal;
	private Quaternion moveRotation;
	private float forwardInput = 0f;
	private float lateralInput = 0f;
	private float lateralTurn = 0f;
	private float groundDistance = 0f;
	private float dynamicSurfacingSpeed = 1f;
	private float surfacingPointElevation = 0f;
	private bool bActive = false;

	public void SetMoveCommand(Vector3 value, bool bOverride)
	{
		if (bOverride)
		{
			moveCommand = value * 0.6f;
		}
		else
		{
			moveCommand += value * 0.6f;
		}
	}

	public void SetVehicleActive(bool value)
	{
		bActive = value;

		// Getting in
		if (bActive)
		{
			rb.isKinematic = true;
			controller.enabled = true;
			effectsTransform.gameObject.SetActive(true);
			invitationText.gameObject.SetActive(false);
		}

		// Getting out
		else
		{
			Vector3 previousMotion = controller.velocity * 10f;
			controller.enabled = false;
			rb.isKinematic = false;

			rb.AddForce(previousMotion);
			Debug.Log("Residual motion: " + previousMotion.magnitude);

			motion = Vector3.zero;
			movementVector = Vector3.zero;
			rawMotion = Vector3.zero;
			moveRotation = transform.rotation;
			EnableGroundEffects(false);
			effectsTransform.gameObject.SetActive(false);
			if ((player != null) && (Vector3.Distance(transform.position, player.transform.position) <= 10f))
			{
				invitationText.gameObject.SetActive(true);
			}
			var em = thrustParticles.emission;
			em.enabled = false;
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
		moveRotation = transform.rotation;

		SetVehicleActive(false);
	}

	void Update()
	{
		if (bActive && (Time.timeScale > 0f))
		{
			SurfaceRotations();
			UpdateMovement();
			InputRotation();
		}
	}

	private void LateUpdate()
	{
		if (bActive)
		{
			dynamicSurfacingSpeed = Mathf.Clamp(Mathf.Sqrt(controller.velocity.magnitude), turnAcceleration, turnSpeed);
			Quaternion finalRotation = surfaceNormal * moveRotation * inputRotation;
			transform.rotation = Quaternion.Lerp(transform.rotation, finalRotation, Time.smoothDeltaTime * turnSpeed * dynamicSurfacingSpeed);
		}
	}

	void SurfaceRotations()
	{
		float targetGrade = 1f;
		float previousSurfaceElevation = surfacingPointElevation;
		Vector3 downRay = (transform.up * -500f) + (controller.velocity * 250f);
		Vector3 origin = transform.position;

		if (Physics.Raycast(origin, downRay, out downHit, downRay.magnitude))
		{
			bool groundHit = !downHit.transform.gameObject.GetComponent<Vehicle>() 
				&& !downHit.transform.gameObject.GetComponent<PlayerMovement>() 
				&& (downHit.transform.gameObject != gameObject) 
				&& (downHit.transform != transform);
			if (groundHit)
			{
				interpNormal = Vector3.Lerp(interpNormal, downHit.normal, Time.smoothDeltaTime * surfaceTurnSpeed);
				groundDistance = downHit.distance;
				surfacingPointElevation = (downHit.point - origin).y;

				if (controller.isGrounded){
					targetGrade = Mathf.Pow(Mathf.Abs(Vector3.Dot(Vector3.up, downHit.normal)), 10f);
				} else {
					targetGrade = Mathf.Lerp(targetGrade, 1f, Time.smoothDeltaTime);
				}
				gradeClimbSpeed = Mathf.Lerp(gradeClimbSpeed, targetGrade, Time.smoothDeltaTime * acceleration);
			}
		}
		else if (Physics.Raycast(origin, Vector3.down * 1000f, out downHit, 15000f))
		{
			bool groundHit = !downHit.transform.gameObject.GetComponent<Vehicle>()
				&& !downHit.transform.gameObject.GetComponent<PlayerMovement>()
				&& (downHit.transform.gameObject != gameObject)
				&& (downHit.transform != transform);
			if (groundHit)
			{
				interpNormal = Vector3.Lerp(interpNormal, downHit.normal, Time.smoothDeltaTime * surfaceTurnSpeed);
				groundDistance = downHit.distance;
				surfacingPointElevation = (downHit.point - origin).y;

				if (controller.isGrounded){
					targetGrade = Mathf.Pow(Mathf.Abs(Vector3.Dot(Vector3.up, downHit.normal)), 10f);
				}
				else{
					targetGrade = Mathf.Lerp(targetGrade, 1f, Time.smoothDeltaTime);
				}
				gradeClimbSpeed = Mathf.Lerp(gradeClimbSpeed, targetGrade, Time.smoothDeltaTime * acceleration);
			}
		}

		// Savoring 'off the cliff' movement
		if (surfacingPointElevation >= previousSurfaceElevation)
		{
			surfaceNormal = Quaternion.FromToRotation(Vector3.up, interpNormal);
		}
		else
		{
			surfaceNormal = Quaternion.Lerp(surfaceNormal, Quaternion.FromToRotation(Vector3.up, interpNormal), Time.deltaTime * surfaceTurnSpeed);
		}
	}

	void UpdateMovement()
	{
		if (bActive && controller.enabled)
		{
			rawMotion = ((forwardInput * Camera.main.transform.forward).normalized
									+ (lateralInput * Camera.main.transform.right)).normalized;
			
			rawMotion.y = 0f;
			movementVector = rawMotion * maxSpeed;
			//if (forwardInput == 0f && lateralInput == 0f)
			//{
			//	movementVector = controller.velocity * -0.9f;
			//}

			motion = Vector3.Lerp(motion, movementVector, Time.smoothDeltaTime * acceleration);
			///motion += transform.forward * forwardInput * moveSpeed;

			// Levitation
			if (forwardInput != 0f)
			{
				float dist = groundDistance;
				if (dist <= levitationRange)
				{
					float scalar = Mathf.Clamp(controller.velocity.magnitude * 0.005f, 0.1f, 1f);
					motion += (Vector3.up * scalar * levitationSpeed);
					if (forwardInput != 0f) {
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

			if (!controller.isGrounded)
			{
				motion += (Vector3.down * gravity);
			}

			motion += moveCommand;

			// Move it move it
			controller.Move(motion * Time.deltaTime * Time.timeScale);

			// Rotation
			Vector3 moveVector = transform.forward;
			if (rawMotion.magnitude != 0f)
			{
				moveVector = controller.velocity.normalized;
			}
			moveVector.y = 0f;

			moveRotation = Quaternion.Lerp(moveRotation,
					Quaternion.LookRotation(moveVector, Vector3.up),
					15f * Time.smoothDeltaTime);

			// Thrust FX
			if ((forwardInput != 0f) || (lateralInput != 0f))
			{
				var em = thrustParticles.emission;
				em.enabled = true;
			}
			else
			{
				var em = thrustParticles.emission;
				em.enabled = false;
			}
		}
	}

	void InputRotation()
	{
		inputRotation = Quaternion.Lerp(inputRotation, Quaternion.Euler(0f, 0f, (lateralInput * forwardInput) * -10f), Time.smoothDeltaTime * turnAcceleration);
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
					Vector3 groundEffectRotation = (controller.velocity - transform.position);
					groundParticles.transform.rotation = transform.rotation;
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
