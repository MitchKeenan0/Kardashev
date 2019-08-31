using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public float moveSpeed = 1.0f;
	public float moveAcceleration = 1.0f;
	public float maxSpeed = 10.0f;
	public float decelSpeed = 1.0f;
	public float jumpSpeed = 1.0f;
	public float gravity = 9.8f;

	private CharacterController controller;
	private BodySystem body;
	private float currentForward = 0;
	private float currentLateral = 0;
	private float lastForward = 0;
	private float lastLateral = 0;
	private Vector3 motion = Vector3.zero;
	private Vector3 motionRaw = Vector3.zero;

	public float GetForward()
	{
		return currentForward;
	}
	public float GetLateral()
	{
		return currentLateral;
	}

	private void Start()
	{
		controller = GetComponent<CharacterController>();
		body = GetComponent<BodySystem>();
	}

	private void Update()
	{
		lastForward = currentForward;
		lastLateral = currentLateral;

		currentForward = Input.GetAxis("Vertical");
		currentLateral = Input.GetAxis("Horizontal");

		// Padding to assist rotation
		if ((currentLateral != 0f) && (Mathf.Abs(currentForward) < 0.1f))
		{
			currentForward = 0.015f;
		}

		UpdateMovement();

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


	void UpdateMovement()
	{
		// Ground control
		if (controller.isGrounded)
		{
			if (Input.GetButtonDown("Jump"))
			{
				motion.y = jumpSpeed;
			}

			// Decelleration
			if ((currentForward == 0.0f) && (currentLateral == 0.0f))
			{
				motionRaw = -controller.velocity * decelSpeed;
			}
			else
			{
				// Acceleration
				motionRaw = ((Camera.main.transform.forward * currentForward)
					+ (Camera.main.transform.right * currentLateral)).normalized;
			}

			// Interp pass for 'smooth moves'
			motion = Vector3.Lerp(motion, motionRaw * moveSpeed, Time.deltaTime * moveAcceleration);
			motion = Vector3.ClampMagnitude(motion, maxSpeed);
		}

		// Gravity
		motion.y -= gravity * Time.deltaTime;

		controller.Move(motion * Time.deltaTime);
	}
}
