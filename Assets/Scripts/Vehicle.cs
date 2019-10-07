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
	public float surfacingSpeed = 5f;
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
	private Vector3 loolVector;
	private float forwardInput = 0f;
	private float lateralInput = 0f;
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
			if (Vector3.Distance(transform.position, player.transform.position) < 5f)
			{
				invitationText.gameObject.SetActive(true);
			}
			player.SetThirdPerson(false);
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

		loolVector = transform.forward;
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
			motion += Vector3.up * (-gravity * Time.smoothDeltaTime);

			controller.Move(motion * Time.smoothDeltaTime);

			// Rotation
			transform.RotateAround(transform.position, transform.up, lateralInput * Time.smoothDeltaTime * turnSpeed);
		}
	}


	void CheckSurfaceNormal()
	{
		Vector3 normal = Vector3.zero;

		// Down ray
		if (Physics.Raycast(transform.position, transform.position + (Vector3.up * -999f), out downHit))
		{
			normal += downHit.normal;
		}

		// Forward ray
		if (Physics.Raycast(transform.position, transform.position + (controller.velocity * 9f), out forwardHit))
		{
			normal += forwardHit.normal;
		}

		
		if (downHit.distance >= 10f)
		{
			normal = Vector3.up;
		}

		Quaternion surfaceNormal = Quaternion.FromToRotation(transform.up, normal) * transform.rotation;
		transform.rotation = Quaternion.Lerp(transform.rotation, surfaceNormal, Time.smoothDeltaTime * surfacingSpeed);
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
