using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodySystem : MonoBehaviour
{
	public float lookSpeed = 2.0f;
	private CharacterController controller;
	private Vector3 lookVector;
    
    void Start()
    {
		controller = GetComponentInParent<CharacterController>();
		lookVector = transform.position + transform.forward;
		transform.LookAt(lookVector);
	}

    
    void Update()
    {
		if (controller != null)
		{
			if ( ((Mathf.Abs(controller.velocity.x) >= 1.0f) || Mathf.Abs(controller.velocity.z) >= 1.0f)
				&& ((controller.velocity.x != 0.0f) || (controller.velocity.z != 0.0f)) )
			{
				lookVector = Vector3.Lerp(lookVector, controller.transform.position + controller.velocity, Time.deltaTime * lookSpeed);
			}
			else
			{
				Vector3 idleVector = lookVector; /// + (Camera.main.transform.forward * 100.0f); ...continuous camera looking
				idleVector.y = transform.position.y + (controller.velocity.y * 0.1f);
				lookVector = Vector3.Lerp(lookVector, idleVector, Time.deltaTime * lookSpeed);
			}

			transform.LookAt(lookVector);
		}
    }


}
