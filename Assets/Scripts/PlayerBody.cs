using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBody : MonoBehaviour
{
	public Transform Head;
	public Transform RightArm;
	public float lookSpeed = 2f;
	public float bodyTurnSpeed = 10f;
	public float turnWeight = 0.3f;
	public float recoveryTime = 0.3f;
	public float normalFOV = 90f;
	public float scopeFOV = 50f;
	public Transform weaponPrefab1;
	public Vector3 weapon1Offset;

	private CharacterController controller;
	private PlayerMovement movement;
	private Rigidbody rb;
	private Gun rightGun;
	private ItemBar itemBar;
	private GameObject equippedItem;

	private Vector3 lookVector;
	private Vector3 lerpAimVector;
	private Vector3 headVector;
	private float playerForward = 0f;
	private float playerLateral = 0f;
	private bool bPhysical = false;
	private float timeAtPhysical = 0f;
	private Vector3 impactVector;


	public void SetForward(float value)
	{
		playerForward = value;
	}

	public void SetLateral(float value)
	{
		playerLateral = value;
	}

	public void TakeSlam(Vector3 vector, float force)
	{
		if (!bPhysical)
		{
			movement.SetActive(false);
			impactVector = vector * force * Time.deltaTime;
			bPhysical = true;
			timeAtPhysical = Time.time;

			// Damage
			HealthBar healthBar = GetComponentInChildren<HealthBar>();
			if (healthBar != null)
			{
				int newHealth = Mathf.FloorToInt(healthBar.CurrentHealth() - (force * 2));
				healthBar.SetHealth(newHealth);
			}
		}
	}



	void Start()
	{
		controller = GetComponentInParent<CharacterController>();
		movement = GetComponentInParent<PlayerMovement>();
		itemBar = FindObjectOfType<ItemBar>();

		rb = GetComponent<Rigidbody>();
		rb.isKinematic = true;

		lookVector = transform.position + transform.forward;
		transform.LookAt(lookVector);

		//InitArmament();
	}

	//void InitArmament()
	//{
	//	if (weaponPrefab1 != null)
	//	{
	//		Transform newWeapon = Instantiate(weaponPrefab1, RightArm.position + weapon1Offset, RightArm.rotation);
	//		newWeapon.SetParent(RightArm);
	//		rightGun = newWeapon.GetComponent<Gun>();
	//		rightGun.InitGun(gameObject.transform);
	//	}
	//}


	void Update()
	{
		UpdateRotation();

		ItemSelectEvents();

		// Receiving slams
		if (bPhysical)
		{
			impactVector = Vector3.Lerp(impactVector, Vector3.zero, 2*Time.deltaTime);

			Vector3 moveVector = new Vector3(movement.GetLateral(), 0.0f, movement.GetForward()) * Time.deltaTime;

			if ((impactVector.magnitude >= 0.025f) && (moveVector.magnitude <= impactVector.magnitude))
			{
				// Gravity mid-air
				if (!controller.isGrounded)
				{
					impactVector.y = Mathf.Lerp(impactVector.y, (-movement.gravity * Time.deltaTime), 3*Time.deltaTime);
				}

				controller.Move(impactVector);
			}
			else
			{
				impactVector = Vector3.zero;
				bPhysical = false;
				movement.SetActive(true);
			}
		}


		// Trigger down
		if (Input.GetMouseButtonDown(0))
		{
			if (equippedItem != null)
			{
				Tool tool = equippedItem.GetComponent<Tool>();
				if (tool != null)
				{
					//tool.InitTool(transform);
					tool.SetToolActive(true);
				}
			}
		}

		// Trigger up
		else if (Input.GetMouseButtonUp(0))
		{
			if (equippedItem != null)
			{
				Tool tool = equippedItem.GetComponent<Tool>();
				if (tool != null)
				{
					tool.SetToolActive(false);
				}
			}
		}

		// Alt trigger down
		if (Input.GetMouseButtonDown(1))
		{
			if (equippedItem != null)
			{
				Tool tool = equippedItem.GetComponent<Tool>();
				if (tool != null)
				{
					//tool.InitTool(transform);
					tool.SetToolAlternateActive(true);
				}
			}
		}

		// Alt trigger up
		if (Input.GetMouseButtonUp(1))
		{
			if (equippedItem != null)
			{
				Tool tool = equippedItem.GetComponent<Tool>();
				if (tool != null)
				{
					tool.SetToolAlternateActive(false);
				}
			}
		}

	}
	

	void ItemSelectEvents()
	{
		if (Input.GetButtonDown("1"))
		{
			EquipItem(1);
		}
		if (Input.GetButtonDown("2"))
		{
			EquipItem(2);
		}
		if (Input.GetButtonDown("3"))
		{
			EquipItem(3);
		}
		if (Input.GetButtonDown("4"))
		{
			EquipItem(4);
		}
		if (Input.GetButtonDown("5"))
		{
			EquipItem(5);
		}
		if (Input.GetButtonDown("6"))
		{
			EquipItem(6);
		}
	}

	void EquipItem(int id)
	{
		// Dequip the current item
		if (equippedItem != null)
		{
			equippedItem.transform.parent = null;
			equippedItem.transform.position = Vector3.up * -5000;
		}

		// Retrieve new item
		if (itemBar.GetItem(id - 1) != null)
		{
			GameObject newItem = itemBar.GetItem(id - 1);
			if (newItem != null)
			{
				newItem.transform.parent = RightArm;
				newItem.transform.localPosition = Vector3.zero;
				newItem.transform.localRotation = Quaternion.identity;
				equippedItem = newItem;

				Tool newTool = newItem.transform.GetComponent<Tool>();
				if (newTool)
				{
					newTool.InitTool(transform);
				}
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
			if ((playerForward >= 0.1f) || (playerLateral != 0.0f))
			{
				lookVector = Vector3.Lerp(lookVector, controller.velocity + onScreenOffset, Time.deltaTime * bodyTurnSpeed);
				bMoving = true;
			}

			// Towards camera -- moving back, or looking around
			float dotToLook = Vector3.Dot(transform.forward, Camera.main.transform.forward);
			bool craningLook = (dotToLook <= 0.6f);
			if ((playerForward <= -0.1f) || craningLook)
			{
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



	//private void OnTriggerEnter(Collider other)
	//{
	//	transform.parent = other.transform;
	//	transform.localScale = Vector3.one;
	//	transform.localRotation = Quaternion.identity;

	//	if (transform.parent != null)
	//	{
	//		Debug.Log("Player attached to " + transform.parent.name + " at " + Time.time);
	//	}

	//	//if (other.CompareTag("Damage"))
	//	//{
	//	//	Vector3 oofVector = Vector3.up + (transform.position - other.transform.position).normalized;
	//	//	TakeSlam(oofVector.normalized, 1f);
	//	//}
	//}
}
