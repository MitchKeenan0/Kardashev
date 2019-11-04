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
	private AudioSource audioPlayer;
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
	private float groundDistance = 0f;
	private float dynamicSurfacingSpeed = 1f;
	private bool bActive = false;
	private float engineSoundPitch = 1f;
	private float engineSoundVolume = 1f;

	public void SetMoveCommand(Vector3 value, bool bOverride)
	{
		if (bOverride)
		{
			moveCommand = value * 0.33f;
		}
		else
		{
			moveCommand += value * 0.33f;
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

			motion = Vector3.zero;
			movementVector = Vector3.zero;
			rawMotion = Vector3.zero;
			moveRotation = transform.rotation;
			EnableGroundEffects(false);
			effectsTransform.gameObject.SetActive(false);

			if ((player != null) && (Vector3.Distance(transform.position, player.transform.position) <= 15f)){
				invitationText.gameObject.SetActive(true);
			}

			if (groundParticles != null){
				var em = groundParticles.emission;
				em.enabled = false;
			}

			if (thrustParticles != null){
				var em = thrustParticles.emission;
				em.enabled = false;
			}

			audioPlayer.Stop();
		}
	}

	public void SetMoveInput(float forward, float lateral)
	{
		forwardInput = forward;
		lateralInput = lateral;
	}

	public void JumpVehicle()
	{
		float jumpValue = jumpSpeed;

		// Air-jump
		if (!controller.isGrounded && (groundDistance > levitationRange))
		{
			jumpValue *= 0.15f;
		}

		motion.y += jumpValue;
	}

    void Start()
    {
		controller = GetComponent<CharacterController>();
		controller.enabled = false;
		rb = GetComponent<Rigidbody>();
		rb.centerOfMass = centerOfMass;
		rb.inertiaTensor = Vector3.one * 0.1f;
		audioPlayer = GetComponent<AudioSource>();
		invitationText.gameObject.SetActive(false);
		effectsTransform.gameObject.SetActive(false);
		inputRotation = transform.rotation;
		surfaceNormal = transform.rotation;
		moveRotation = transform.rotation;
		groundDistance = levitationRange * 1.5f;
		SetVehicleActive(false);
	}

	void Update()
	{
		if (bActive && (Time.timeScale > 0f))
		{
			SurfaceRotations();
			UpdateMovement();
			InputRotation();
			UpdateSounds();

			if (bActive)
			{
				dynamicSurfacingSpeed = Mathf.Clamp(Mathf.Sqrt(controller.velocity.magnitude), turnAcceleration, turnSpeed);
				Quaternion finalRotation = surfaceNormal * moveRotation * inputRotation;
				transform.rotation = Quaternion.Lerp(transform.rotation, finalRotation, Time.smoothDeltaTime * turnSpeed * dynamicSurfacingSpeed);
			}
		}
	}

	void SurfaceRotations()
	{
		float targetGrade = 1f;
		Vector3 downRay = (transform.up * -1500f) + (controller.velocity * 10f);
		Vector3 origin = transform.position + (Vector3.down * 0.5f);
		///Debug.DrawRay(origin, downRay, Color.white);
		if (Physics.Raycast(origin, downRay, out downHit, downRay.magnitude))
		{
			bool groundHit = !downHit.transform.gameObject.GetComponent<Vehicle>()
				&& !downHit.transform.gameObject.GetComponent<PlayerMovement>()
				&& (downHit.transform.gameObject != gameObject)
				&& (downHit.transform != transform);
			if (groundHit)
			{
				interpNormal = downHit.normal;
				groundDistance = Mathf.Abs((downHit.point - transform.position).y);

				if (controller.isGrounded){
					targetGrade = Mathf.Pow(Mathf.Abs(Vector3.Dot(Vector3.up, downHit.normal)), 10f);
				} else {
					targetGrade = Mathf.Lerp(targetGrade, 1f, Time.smoothDeltaTime);
				}
				gradeClimbSpeed = Mathf.Lerp(gradeClimbSpeed, targetGrade, Time.smoothDeltaTime * acceleration);
			}
		}
		else
		{
			groundDistance = levitationRange * 1.5f;
		}

		surfaceNormal = Quaternion.Lerp(surfaceNormal, Quaternion.FromToRotation(Vector3.up, interpNormal), Time.deltaTime * surfaceTurnSpeed);
	}

	void UpdateMovement()
	{
		if (bActive && controller.enabled)
		{
			rawMotion = ((forwardInput * Camera.main.transform.forward).normalized
									+ (lateralInput * Camera.main.transform.right)).normalized;
			rawMotion.y = 0f;
			movementVector = rawMotion * maxSpeed;
			motion = Vector3.Lerp(motion, movementVector, Time.deltaTime * acceleration);

			// Levitation
			if (forwardInput != 0f)
			{
				float dist = groundDistance;
				if (dist < levitationRange)
				{
					float levitationScalar = Mathf.Clamp((levitationRange - dist), levitationSpeed, levitationSpeed * 5f);
					float speedScalar = Mathf.Clamp(controller.velocity.magnitude * 0.005f, 0.1f, 1f);
					motion += (Vector3.up * speedScalar * levitationSpeed * levitationScalar);
					if (forwardInput != 0f){
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
			controller.Move(motion * Time.smoothDeltaTime * Time.timeScale);

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

	void UpdateSounds()
	{
		float velocity = controller.velocity.magnitude;
		if (velocity < 1f)
		{
			audioPlayer.Stop();
		}
		else if (!audioPlayer.isPlaying)
		{
			audioPlayer.Play();
		}

		float targetPitch = 1f;
		float targetVolume = 1f;
		if (forwardInput != 0f || lateralInput != 0f)
		{
			targetPitch = Remap(velocity, 0f, maxSpeed, 0.1f, 3f);
			targetVolume = Remap(velocity, 0f, maxSpeed, 0.0f, 1f);
		}
		else
		{
			targetPitch = 0.5f;
			targetVolume = 0.1f;
		}

		engineSoundPitch = Mathf.Lerp(engineSoundPitch, targetPitch, Time.deltaTime);
		engineSoundVolume = Mathf.Lerp(engineSoundVolume, targetVolume, Time.deltaTime);
		audioPlayer.pitch = engineSoundPitch;
		audioPlayer.volume = engineSoundVolume;
	}

	float Remap(float value, float from1, float to1, float from2, float to2)
	{
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
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

	private void OnCollisionEnter(Collision collision)
	{
		Vector3 collisionNormal = collision.GetContact(0).normal;
		if (moveCommand != Vector3.zero)
		{
			SetMoveCommand(Vector3.ProjectOnPlane(moveCommand, collisionNormal), true);
			Debug.Log("Schwing");
		}
	}

}
