using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodySystem : MonoBehaviour
{
	public Transform Head;
	public Transform RightArm;
	public float lookSpeed = 2f;
	public float bodyTurnSpeed = 10f;
	public float turnWeight = 0.3f;
	public Transform weaponPrefab1;
	public Vector3 weapon1Offset;

	private CharacterController controller;
	private PlayerMovement movement;
	private Gun rightGun;
	private Vector3 lookVector;
	private Vector3 lerpAimVector;
	private Vector3 headVector;
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
			Transform newWeapon = Instantiate(weaponPrefab1, RightArm.position + weapon1Offset, RightArm.rotation);
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
			Vector3 onScreenOffset = transform.position + (Camera.main.transform.forward * 100f);
			bool bMoving = false;

			// Forward/Strafe towards velocity,
			if ((playerForward >= 0.1f) || (playerLateral != 0.0f)) {
				lookVector = Vector3.Lerp(lookVector, controller.velocity + onScreenOffset, Time.deltaTime * bodyTurnSpeed);
				bMoving = true;
			}

			// Backward towards camera
			if (playerForward <= -0.1f) {
				lookVector = Vector3.Lerp(lookVector, Camera.main.transform.forward + onScreenOffset, Time.deltaTime * bodyTurnSpeed);
				bMoving = true;
			}

			// Residual 'idle' rotation
			if (!bMoving)
			{
				Vector3 idleVector = transform.position + (transform.forward * 100.0f);
				idleVector.y = transform.position.y;

				lookVector = Vector3.Lerp(lookVector, idleVector, Time.deltaTime * bodyTurnSpeed);
			}

			lookVector.y = transform.position.y;
			transform.LookAt(lookVector);

			if (Head != null)
			{
				lerpAimVector = transform.position + (Camera.main.transform.forward * 100f);
				
				//float dotToTarget = lookSpeed / Mathf.Abs(Vector3.Dot(transform.forward, lerpAimVector.normalized));

				headVector = Vector3.Lerp(headVector, lerpAimVector, Time.deltaTime * lookSpeed); // * dotToTarget
				Head.transform.LookAt(headVector);
			}
		}
	}


}
