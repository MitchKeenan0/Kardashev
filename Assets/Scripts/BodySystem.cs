using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodySystem : MonoBehaviour
{
	public float lookSpeed = 2.0f;
	public float turnWeight = 0.3f;

	private CharacterController controller;
	private PlayerMovement movement;
	private Vector3 lookVector;
	private float playerForward;
	private float playerLateral;
    

    void Start()
    {
		controller = GetComponentInParent<CharacterController>();
		movement = GetComponentInParent<PlayerMovement>();
		lookVector = transform.position + transform.forward;
		transform.LookAt(lookVector);
	}


    void Update()
    {
		UpdateRotation();
    }


	void UpdateRotation()
	{
		if (controller != null)
		{
			playerForward = movement.GetForward();
			playerLateral = movement.GetLateral();

			// Active 'move' rotation
			if ((playerForward != 0.0f) || (playerLateral != 0.0f))
			{
				lookVector = Vector3.Lerp(lookVector, controller.transform.position + controller.velocity, Time.deltaTime * lookSpeed);
			}
			else
			{
				// Residual 'idle' rotation
				Vector3 idleVector = transform.position + (transform.forward * 100.0f);

				if (controller.isGrounded)
				{
					idleVector.y = transform.position.y;
				}

				lookVector = Vector3.Lerp(lookVector, idleVector, Time.deltaTime * lookSpeed);
			}

			transform.LookAt(lookVector);
		}
	}


}
