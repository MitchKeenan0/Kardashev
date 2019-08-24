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
	private float currentForward = 0;
	private float currentLateral = 0;
	private Vector3 motion = Vector3.zero;
	private Vector3 motionRaw = Vector3.zero;

	private void Start()
	{
		controller = GetComponent<CharacterController>();
	}

	private void Update()
	{
		currentForward = Input.GetAxis("Vertical");
		currentLateral = Input.GetAxis("Horizontal");

		if (controller.isGrounded)
		{
			if (Input.GetButtonDown("Jump"))
			{
				motion.y = jumpSpeed;
			}

			if ((currentForward == 0.0f) && (currentLateral == 0.0f))
			{
				motionRaw = -controller.velocity * decelSpeed;
			}
			else
			{
				motionRaw = ((Camera.main.transform.forward * currentForward)
					+ (Camera.main.transform.right * currentLateral)).normalized;
			}
			
			motion = Vector3.Lerp(motion, motionRaw * moveSpeed, Time.deltaTime * moveAcceleration);
			motion = Vector3.ClampMagnitude(motion, maxSpeed);
		}

		motion.y -= gravity * Time.deltaTime;

		controller.Move(motion * Time.deltaTime);
	}
}
