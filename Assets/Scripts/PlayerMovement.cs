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
	public Vector3 moveCommand = Vector3.zero;

	private CharacterController controller;
	private PlayerBody body;
	private float currentForward = 0;
	private float currentLateral = 0;
	private float lastForward = 0;
	private float lastLateral = 0;
	private Vector3 motion = Vector3.zero;
	private Vector3 motionRaw = Vector3.zero;
	private bool bActive = true;

	public float GetForward()
	{
		return currentForward;
	}
	public float GetLateral()
	{
		return currentLateral;
	}

	public void SetMoveCommand(Vector3 value)
	{
		moveCommand = value;
	}

	public void SetActive(bool value)
	{
		bActive = value;
		motionRaw = Vector3.zero;
	}

	private void Start()
	{
		controller = GetComponent<CharacterController>();
		body = GetComponent<PlayerBody>();
	}

	private void Update()
	{
		lastForward = currentForward;
		lastLateral = currentLateral;

		currentForward = Input.GetAxisRaw("Vertical");
		currentLateral = Input.GetAxisRaw("Horizontal");

		if (bActive)
		{
			// Padding to assist rotation
			if ((currentLateral != 0f) && (Mathf.Abs(currentForward) < 0.1f))
			{
				currentForward = 0.015f;
			}
			
			// Actual Movement
			UpdateMovement();
		}

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
			motion = Vector3.Lerp(motion, motionRaw * maxSpeed, Time.deltaTime * moveAcceleration);
			motion = Vector3.ClampMagnitude(motion, maxSpeed);
		}

		// Gravity
		motion.y -= gravity * Time.deltaTime;

		// Exterior forces
		motion += moveCommand;

		controller.Move(motion * Time.deltaTime);
	}
}
