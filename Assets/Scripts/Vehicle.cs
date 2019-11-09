using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
	public Transform invitationText;
	public Collider invitationCollider;
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
	public float turnAngling = 10f;
	public float surfaceTurnSpeed = 5f;
	public float gradeClimbSpeed = 0f;
	public float gravity = 9f;
	public float levitationRange = 5f;
	public float levitationSpeed = 1f;
	public float jumpSpeed = 10f;
	public Vector3 centerOfMass;
	public float maxAirTime = 5f;

	//private CharacterController controller;
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
	private bool bGrounded = false;
	RaycastHit groundHit;

	public void SetMoveCommand(Vector3 value, bool bOverride)
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

	public void SetVehicleActive(bool value)
	{
		bActive = value;

		// Getting in
		if (bActive)
		{
			effectsTransform.gameObject.SetActive(true);
			invitationText.gameObject.SetActive(false);
			invitationCollider.enabled = false;
		}

		// Getting out
		else
		{
			motion = Vector3.zero;
			movementVector = Vector3.zero;
			rawMotion = Vector3.zero;
			moveRotation = transform.rotation;
			EnableGroundEffects(false);
			effectsTransform.gameObject.SetActive(false);
			invitationCollider.enabled = true;

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
		if (!bGrounded && (groundDistance > levitationRange))
		{
			jumpValue *= 0.15f;
		}

		rb.velocity += Vector3.up * jumpValue;
	}

    void Start()
    {
		rb = GetComponent<Rigidbody>();
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
			if (Physics.Raycast(transform.position, Vector3.down * 1000f, out groundHit))
			{
				if (!groundHit.transform.GetComponent<Vehicle>()
				&& !groundHit.transform.GetComponent<PlayerBody>())
				{
					bGrounded = (groundHit.distance < 1.5f);
				}
			}

			SurfaceRotations();
			UpdateMovement();
			InputRotation();
			UpdateSounds();

			if (bActive)
			{
				dynamicSurfacingSpeed = Mathf.Clamp(Mathf.Sqrt(rb.velocity.magnitude), turnAcceleration, turnSpeed);
				Quaternion finalRotation = surfaceNormal * moveRotation * inputRotation;
				transform.rotation = Quaternion.Lerp(transform.rotation, finalRotation, Time.smoothDeltaTime * turnSpeed * dynamicSurfacingSpeed);
			}
		}
	}

	void FixedUpdate()
	{
		if (bActive)
			MoveForces();
		moveCommand = Vector3.Lerp(moveCommand, Vector3.zero, Time.fixedDeltaTime);
	}

	void MoveForces()
	{
		// Add Levitation
		if (rb.velocity.magnitude > 1f)
		{
			float dist = groundDistance;
			if (dist < levitationRange)
			{
				float speedScalar = Remap(rb.velocity.magnitude, 0f, 1000f, 0f, 1f);
				float finalScale = Mathf.Clamp(levitationSpeed * speedScalar, 0f, gravity);
				motion += (Vector3.up * finalScale);
				///Debug.Log("Velocity: " + rb.velocity.magnitude + "  speed: " + speedScalar + "  final: " + finalScale + "        " + Time.time);
			}
		}

		// Movement
		motion += moveCommand;
		if (!bGrounded)
			motion += (Vector3.down * gravity);
		motion *= Time.timeScale;
		rb.AddForce(motion * Time.fixedDeltaTime);
	}

	void SurfaceRotations()
	{
		float targetGrade = 1f;
		Vector3 downRay = (transform.up * -1500f) + (rb.velocity * 10f);
		Vector3 origin = transform.position;
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

				if (bGrounded){
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
		if (bActive)
		{
			if (groundHit.distance <= 3f)
			{
				rawMotion = ((forwardInput * Camera.main.transform.forward).normalized
									+ (lateralInput * Camera.main.transform.right)).normalized;
				rawMotion.y = 0f;
				motion = rawMotion * moveSpeed;
			}

			// Ground Effects
			if ((forwardInput != 0f) || (lateralInput != 0f)
				&& (groundDistance < levitationRange))
			{
				EnableGroundEffects(true);
				var main = groundParticles.main;
				main.startSize = Remap(rb.velocity.magnitude, 0f, moveSpeed, 0.1f, 5f);
			}
			else
			{
				EnableGroundEffects(false);
			}

			// Rotation
			Vector3 moveVector = transform.forward;
			if (rawMotion.magnitude != 0f)
			{
				moveVector = rb.velocity.normalized;
			}
			moveVector.y = 0f;

			if (moveVector != Vector3.zero)
			{
				moveRotation = Quaternion.Lerp(moveRotation,
					Quaternion.LookRotation(moveVector, Vector3.up),
					15f * Time.smoothDeltaTime);
			}

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
		inputRotation = Quaternion.Lerp(inputRotation, Quaternion.Euler(forwardInput * -turnAngling * 0.33f, 0f, lateralInput * -turnAngling), Time.smoothDeltaTime * turnAcceleration);
	}

	void UpdateSounds()
	{
		float velocity = rb.velocity.magnitude;
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
					Vector3 groundEffectRotation = (rb.velocity - transform.position);
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

	//private void OnCollisionEnter(Collision collision)
	//{
	//	Vector3 collisionNormal = collision.GetContact(0).normal;
	//	if (moveCommand != Vector3.zero)
	//	{
	//		SetMoveCommand(Vector3.ProjectOnPlane(moveCommand, collisionNormal), true);
	//		Debug.Log("Schwing");
	//	}
	//}

}
