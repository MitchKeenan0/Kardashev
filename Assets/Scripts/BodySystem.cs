using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodySystem : MonoBehaviour
{
	public Transform RightArm;
	public float lookSpeed = 2.0f;
	public float turnWeight = 0.3f;
	public Transform weaponPrefab1;

	private CharacterController controller;
	private PlayerMovement movement;
	private Gun rightGun;
	private Vector3 lookVector;
	private float playerForward;
	private float playerLateral;
    

	public void SetForward(float value)
	{
		playerForward = value;
	}

	public void SetLateral(float value)
	{
		playerLateral = value;
	}

    void Start()
    {
		controller = GetComponentInParent<CharacterController>();
		movement = GetComponentInParent<PlayerMovement>();
		lookVector = transform.position + transform.forward;
		transform.LookAt(lookVector);

		InitArmament();
	}

	void InitArmament()
	{
		if (weaponPrefab1 != null)
		{
			Transform newWeapon = Instantiate(weaponPrefab1, RightArm.position, RightArm.rotation);
			newWeapon.SetParent(RightArm);
			rightGun = newWeapon.GetComponent<Gun>();
			rightGun.InitGun(transform);
		}
	}


    void Update()
    {
		UpdateRotation();

		if (Input.GetMouseButtonDown(0))
		{
			if (rightGun != null)
			{
				rightGun.FireBullet();
			}
		}
    }


	void UpdateRotation()
	{
		if (controller != null)
		{
			// Active 'move' rotation
			if ((playerForward != 0.0f) || (playerLateral != 0.0f))
			{
				lookVector = Vector3.Lerp(lookVector, controller.transform.position + controller.velocity, Time.deltaTime * lookSpeed);
			}
			else
			{
				// Residual 'idle' rotation
				Vector3 idleVector = transform.position + (transform.forward * 100.0f);
				idleVector.y = transform.position.y;

				lookVector = Vector3.Lerp(lookVector, idleVector, Time.deltaTime * lookSpeed);
			}

			lookVector.y = transform.position.y;
			transform.LookAt(lookVector);
		}
	}


}
