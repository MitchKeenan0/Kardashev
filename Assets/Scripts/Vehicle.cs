using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
	public Transform invitationText;
	public Transform effectsTransform;
	public Transform footMountTransform;
	public float moveSpeed = 1f;
	public float acceleration = 10f;
	public float maxSpeed = 100f;
	public float turnSpeed = 1f;
	public float turnAcceleration = 5f;
	public float surfaceTurnSpeed = 5f;
	public float gradeClimbSpeed = 0f;
	public float gravity = 9f;
	public bool bCanJump = true;
	public float jumpSpeed = 10f;
	public Vector3 centerOfMass;
	public float maxAirTime = 5f;

	private CharacterController controller;
	private Rigidbody rb;
	private PlayerBody player;
	private RaycastHit downHit;
	private RaycastHit forwardHit;
	private Vector3 motion;
	private Quaternion inputRotation;
	private Quaternion surfaceNormal;
	private float forwardInput = 0f;
	private float lateralInput = 0f;
	private float lateralTurn = 0f;
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
			rb.isKinematic = false;
			rb.AddForce(lastVelocity);
			motion = Vector3.zero;
			effectsTransform.gameObject.SetActive(false);
			if ((player != null) && (Vector3.Distance(transform.position, player.transform.position) <= 5f))
			{
				invitationText.gameObject.SetActive(true);

				player.SetThirdPerson(false);
			}
		}
	}


	public void SetMoveInput(float forward, float lateral)
	{
		forwardInput = forward;
		lateralInput = lateral;
	}


	public void JumpVehicle()
	{
		if (bCanJump && controller.isGrounded)
		{
			motion.y += jumpSpeed;
		}
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

		inputRotation = transform.rotation;
		surfaceNormal = transform.rotation;

		SetVehicleActive(false);
	}

	void Update()
	{
		if (bActive)
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
			dynamicSurfacingSpeed = Mathf.Clamp(Mathf.Sqrt(controller.velocity.magnitude), 1f, 10f);
			transform.rotation = Quaternion.Lerp(transform.rotation, (surfaceNormal * inputRotation), Time.smoothDeltaTime * turnSpeed * dynamicSurfacingSpeed);
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
				lateralTurn = Mathf.Lerp(lateralTurn, 2f*lateralInput, Time.smoothDeltaTime * turnAcceleration);
			}

			Quaternion turnRotation = Quaternion.AngleAxis(lateralTurn, transform.up);
			inputRotation = turnRotation;
		}
	}


	void SurfaceRotations()
	{
		float targetGrade = 1f;
		Vector3 surfaceNormalVector = Vector3.up;
		Vector3 downRay = (Vector3.up * -50f);
		if (forwardInput != 0f)
		{
			downRay += (controller.velocity * 3f);
		}

		if (Physics.Raycast(transform.position, downRay, out downHit))
		{
			bool groundHit = !downHit.transform.gameObject.GetComponent<PlayerMovement>() && (downHit.transform != transform);
			if (groundHit)
			{
				surfaceNormalVector = downHit.normal;
				if (controller.isGrounded)
				{
					targetGrade = Mathf.Pow(Mathf.Abs(Vector3.Dot(Vector3.up, downHit.normal)), 10f);
				} else {
					targetGrade = Mathf.Lerp(targetGrade, 1f, Time.smoothDeltaTime);
				}
				gradeClimbSpeed = Mathf.Lerp(gradeClimbSpeed, targetGrade, Time.smoothDeltaTime * acceleration);
			}
		}

		surfaceNormal = Quaternion.FromToRotation(transform.up, surfaceNormalVector) * transform.rotation;
	}


	void UpdateMovement()
	{
		if (bActive && controller.enabled)
		{
			// Propulsion
			Vector3 forwardMovement = transform.forward * moveSpeed * maxSpeed * forwardInput * gradeClimbSpeed;
			motion = Vector3.Lerp(motion, forwardMovement, Time.smoothDeltaTime * acceleration);

			// Gravity
			if (!controller.isGrounded)
			{
				motion += Vector3.up * (-gravity * Time.smoothDeltaTime);
			}

			if (controller.isGrounded && (Vector3.Dot(Vector3.up, downHit.normal) < 0.1f))
			{
				// Drifting
				motion += (downHit.normal + (Vector3.up * -2f)).normalized;
			}

			controller.Move(motion * Time.smoothDeltaTime);
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
			}
		}
	}

}
