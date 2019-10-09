using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
	public Transform invitationText;
	public Transform effectsTransform;
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
	private Vector3 lookVector;
	private float forwardInput = 0f;
	private float lateralInput = 0f;
	private float lateralTurn = 0f;
	private bool bActive = false;
	private bool bDoSurfacing = true;
	private float surfacingPauseTime = 0f;


	public void SetVehicleActive(bool value)
	{
		bActive = value;

		// Getting in
		if (bActive)
		{
			rb.isKinematic = true;
			controller.enabled = true;
			invitationText.gameObject.SetActive(false);
			player.SetThirdPerson(true);
			effectsTransform.gameObject.SetActive(true);
		}

		// Getting out
		else
		{
			controller.enabled = false;
			rb.isKinematic = false;
			rb.AddForce(motion);
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
		lateralTurn = lateral;
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

		lookVector = transform.forward;

		SetVehicleActive(false);
	}


	void Update()
	{
		if (controller.enabled)
		{
			// Rotations for input and surfaces
			if (lateralInput != lateralTurn)
			{
				lateralInput = Mathf.Lerp(lateralInput, lateralTurn, Time.smoothDeltaTime * turnAcceleration);
				transform.RotateAround(transform.position, transform.up, lateralInput * turnSpeed);
			}

			UpdateSurfacing();


			// Propulsion
			Vector3 forwardMovement = transform.forward * forwardInput * maxSpeed * gradeClimbSpeed;
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


	void UpdateSurfacing()
	{
		Vector3 surfacingNormal = Vector3.up;
		float targetGrade = 1f;

		// Down ray
		Vector3 downRay = transform.position - (Vector3.up * 100f);
		if (!controller.isGrounded)
		{
			downRay += controller.velocity;
		}

		if (Physics.Raycast(transform.position, downRay, out downHit))
		{
			surfacingNormal = downHit.normal;

			if (controller.isGrounded)
			{
				targetGrade = Mathf.Pow(Mathf.Abs(Vector3.Dot(Vector3.up, downHit.normal)), 10f);
			} else {
				targetGrade = 1f;
			}
		}

		gradeClimbSpeed = Mathf.Lerp(gradeClimbSpeed, targetGrade, Time.smoothDeltaTime * acceleration);

		Quaternion surfaceNormal = Quaternion.FromToRotation(transform.up, surfacingNormal) * transform.rotation;
		float dynamicSurfacingSpeed = Mathf.Clamp(Mathf.Sqrt(controller.velocity.magnitude), 0.1f, 100f);

		transform.rotation = Quaternion.Lerp(transform.rotation, surfaceNormal, Time.smoothDeltaTime * surfaceTurnSpeed * dynamicSurfacingSpeed);
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
