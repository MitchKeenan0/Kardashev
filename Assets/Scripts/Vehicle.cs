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
	public float activeDrag = 0.1f;
	public float inactiveDrag = 1f;
	public float maxAirTime = 5f;
	
	private Rigidbody rb;
	private AudioSource audioPlayer;
	private Character player;
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
			moveCommand = value * 10f;
		}
		else
		{
			moveCommand += value * 10f;
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
			rb.drag = activeDrag;
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
			if (bGrounded)
				rb.drag = inactiveDrag;
			else
				rb.drag = activeDrag;

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
		if (!bGrounded && (groundDistance > 2f))
		{
			jumpValue *= 0.05f;
		}

		rb.velocity += Vector3.up * jumpValue;
	}

    void Start()
    {
		rb = GetComponent<Rigidbody>();
		rb.drag = inactiveDrag;
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
		if (Time.timeScale > 0f)
		{
			if (Physics.Raycast(transform.position, Vector3.down * 1000f, out groundHit))
			{
				if (!groundHit.transform.GetComponent<Vehicle>()
				&& !groundHit.transform.GetComponent<PlayerBody>()
				&& !groundHit.transform.GetComponent<Tool>())
				{
					groundDistance = groundHit.distance;
					bGrounded = (groundHit.distance < 1.5f);
					if (bGrounded && !bActive && (rb.drag != inactiveDrag))
					{
						rb.drag = inactiveDrag;
					}
				}
			}
		}

		if (bActive)
		{
			SurfaceRotations();
			UpdateMovement();
			InputRotation();
			UpdateSounds();

			// Rotation data update
			dynamicSurfacingSpeed = Mathf.Clamp(Mathf.Sqrt(rb.velocity.magnitude), 1f, turnSpeed);
			Quaternion finalRotation = surfaceNormal * moveRotation * inputRotation;
			transform.rotation = Quaternion.Lerp(transform.rotation, finalRotation, Time.smoothDeltaTime * turnSpeed * dynamicSurfacingSpeed);
		}
	}

	void FixedUpdate()
	{
		if (bActive)
			rb.AddForce(motion * Time.fixedDeltaTime * Time.timeScale);

		if (!bGrounded)
		{
			rb.AddForce(Vector3.down * gravity * Time.fixedDeltaTime);
		}
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

				if (bGrounded){
					targetGrade = Mathf.Pow(Mathf.Abs(Vector3.Dot(Vector3.up, downHit.normal)), 10f);
				} else {
					targetGrade = Mathf.Lerp(targetGrade, 1f, Time.smoothDeltaTime);
				}
				gradeClimbSpeed = Mathf.Lerp(gradeClimbSpeed, targetGrade, Time.smoothDeltaTime * acceleration);
			}
		}

		surfaceNormal = Quaternion.Lerp(surfaceNormal, Quaternion.FromToRotation(Vector3.up, interpNormal), Time.deltaTime * surfaceTurnSpeed);
	}

	void UpdateMovement()
	{
		if (bActive)
		{
			// Normal movement
			if (groundHit.distance <= 3f)
			{
				rawMotion = ((forwardInput * Camera.main.transform.forward).normalized
									+ (lateralInput * Camera.main.transform.right)).normalized;
				rawMotion.y = 0f;
				motion = rawMotion * moveSpeed;
			}
			else
			{
				motion = Vector3.zero;
			}

			// Outside forces
			motion += moveCommand;
			//moveCommand = Vector3.Lerp(moveCommand, Vector3.zero, Time.fixedDeltaTime);

			// Levitation
			if (rb.velocity.magnitude >= 0.1f)
			{
				if (groundDistance <= levitationRange)
				{
					Vector3 lateralVelocity = rb.velocity;
					lateralVelocity.y = 0f;
					float speedScalar = Remap(lateralVelocity.magnitude, 0f, 1000f, 0f, 1f);
					float proximityScalar = Mathf.Clamp(1f / groundDistance, 0.2f, 10f);
					float finalScale = Mathf.Clamp(levitationSpeed * speedScalar * proximityScalar, levitationSpeed * 0.2f, levitationSpeed);
					motion += (Vector3.up * finalScale);
				}
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
					5f * Time.smoothDeltaTime);
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
		inputRotation = Quaternion.Lerp(inputRotation, 
			Quaternion.Euler(forwardInput * -turnAngling * 0.33f, 0f, lateralInput * -turnAngling), 
			Time.smoothDeltaTime * turnAcceleration);
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
			if (other.transform.GetComponent<Character>())
			{
				player = other.transform.GetComponent<Character>();
				player.SetVehicle(false, this);
				invitationText.gameObject.SetActive(true);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!bActive)
		{
			if (other.transform.GetComponent<Character>())
			{
				invitationText.gameObject.SetActive(false);
				player.SetVehicle(false, null);
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
