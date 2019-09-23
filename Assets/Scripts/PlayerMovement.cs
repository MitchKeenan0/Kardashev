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
	public float airControl = 1f;
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
	private bool bStopping = false;

	public float GetForward()
	{
		return currentForward;
	}
	public float GetLateral()
	{
		return currentLateral;
	}

	public void SetMoveCommand(Vector3 value, bool bOverrideVelocity)
	{
		moveCommand = value;

		if (bOverrideVelocity)
		{
			moveCommand += (-controller.velocity * Time.deltaTime);
		}

		// Gravity
		if (moveCommand != Vector3.zero)
		{
			moveCommand += Vector3.up * -gravity * Time.smoothDeltaTime;
		}
	}

	public void AddMoveCommand(Vector3 value)
	{
		moveCommand += value;
	}

	public void SetActive(bool value)
	{
		bActive = value;
		motionRaw = Vector3.zero;

		if (!bActive)
		{
			currentForward = 0;
			currentLateral = 0;
			lastForward = 0;
			lastLateral = 0;
			motion = Vector3.zero;
			motionRaw = Vector3.zero;
			moveCommand = Vector3.zero;
		}
	}

	private void Start()
	{
		Application.targetFrameRate = 70;

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
			//if ((currentLateral != 0f) && (Mathf.Abs(currentForward) < 0.1f))
			//{
			//	currentForward = 0.015f;
			//}
			
			// Actual Movement
			UpdateMovement();
		}

		if (bStopping)
		{
			Vector3 currentV = controller.velocity * Time.deltaTime;
			if (Mathf.Abs(currentV.magnitude) < 0.5f)
			{
				bStopping = false;
				bActive = true;
			}
			else
			{
				controller.Move(-currentV);
			}
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
		// Decelleration
		if ((currentForward == 0.0f) && (currentLateral == 0.0f))
		{
			motionRaw = -controller.velocity * decelSpeed;
		}
		else
		{
			// Acceleration
			motionRaw = ((Camera.main.transform.forward * currentForward)
				+ (transform.right * currentLateral)).normalized; /// Camera.main.transform.right
		}

		if (!controller.isGrounded)
		{
			motionRaw *= airControl;
		}
		else
		{
			// Ground control and jumping
			if (Input.GetButtonDown("Jump"))
			{
				motion.y = jumpSpeed;
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
