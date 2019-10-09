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
			CheckSurfaceNormal();

			// Propulsion
			Vector3 forwardMovement = transform.forward * forwardInput * maxSpeed;
			motion = Vector3.Lerp(motion, forwardMovement, Time.smoothDeltaTime * acceleration);

			// Gravity
			if (!controller.isGrounded)
			{
				motion += Vector3.up * (-gravity * Time.smoothDeltaTime);
			}

			if (forwardInput == 0f && downHit.distance <= 2f && Vector3.Dot(Vector3.up, downHit.normal) < 0.7f)
			{
				// Drift
				motion += (downHit.normal + (Vector3.up * -2f)).normalized * 0.15f;
			}

			// Rotation
			if (lateralInput != lateralTurn)
			{
				lateralInput = Mathf.Lerp(lateralInput, lateralTurn, Time.smoothDeltaTime * turnAcceleration);
			}

			controller.Move(motion * gradeClimbSpeed * Time.smoothDeltaTime);
			transform.RotateAround(transform.position, transform.up, lateralInput * turnSpeed);
		}
	}


	void CheckSurfaceNormal()
	{
		Vector3 surfacingNormal = Vector3.zero;

		// Forward ray
		if (!controller.isGrounded)
		{
			if (Physics.Raycast(transform.position, transform.position + (controller.velocity * 11f), out forwardHit))
			{
				surfacingNormal += forwardHit.normal;
			}
		}

		// Down ray
		if (Physics.Raycast(transform.position, transform.position + (Vector3.up * -999f), out downHit))
		{
			float targetGrade = 0f;

			if (downHit.distance <= 2f)
			{
				surfacingNormal += downHit.normal;
				targetGrade = Mathf.Pow(Mathf.Abs(Vector3.Dot(Vector3.up, downHit.normal)), 10f);
			}
			else
			{
				targetGrade = 1f;
			}

			gradeClimbSpeed = Mathf.Lerp(gradeClimbSpeed, targetGrade, Time.smoothDeltaTime * acceleration);
		}

		
		
		if (downHit.distance >= 10f)
		{
			surfacingNormal = Vector3.up;
		}

		Quaternion surfaceNormal = Quaternion.FromToRotation(transform.up, surfacingNormal) * transform.rotation;
		transform.rotation = Quaternion.Lerp(transform.rotation, surfaceNormal, Time.smoothDeltaTime * surfaceTurnSpeed);
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
