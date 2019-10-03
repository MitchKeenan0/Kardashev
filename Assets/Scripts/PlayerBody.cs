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
	public Transform damageParticles;
	public Transform dropImpactParticles;
	public Transform boostImpactParticles;
	public Transform pauseScreen;

	private CharacterController controller;
	private PlayerMovement movement;
	private Rigidbody rb;
	private CameraController camControl;
	private Gun rightGun;
	private ItemBar itemBar;
	private GameObject equippedItem;

	private Vector3 lookVector;
	private Vector3 lerpAimVector;
	private Vector3 headVector;
	private float playerForward = 0f;
	private float playerLateral = 0f;
	private bool bPhysical = false;
	private bool bPaused = false;
	private float timeAtPhysical = 0f;
	private Vector3 impactVector;
	private RaycastHit[] groundHits;


	public void SetForward(float value)
	{
		playerForward = value;
	}

	public void SetLateral(float value)
	{
		playerLateral = value;
	}

	public void TakeSlam(Vector3 vector, float force, bool bDamage)
	{
		if (!bPhysical)
		{
			movement.SetActive(false);

			impactVector = vector * force * 2*Time.smoothDeltaTime;
			bPhysical = true;
			timeAtPhysical = Time.time;

			if (bDamage)
			{
				TakeDamage(force);
			}
		}
	}


	void Start()
	{
		controller = GetComponentInParent<CharacterController>();
		movement = GetComponentInParent<PlayerMovement>();

		pauseScreen.gameObject.SetActive(false);

		itemBar = FindObjectOfType<ItemBar>();
		camControl = FindObjectOfType<CameraController>();
		rb = GetComponent<Rigidbody>();
		rb.isKinematic = true;

		lookVector = transform.position + transform.forward;
		transform.LookAt(lookVector);

		EquipItem(1);
	}


	void Update()
	{
		UpdateGroundState();

		UpdateRotation();

		ItemSelectEvents();

		// Receiving slams
		if (bPhysical)
		{
			impactVector = Vector3.Lerp(impactVector, Vector3.zero, 2 * Time.smoothDeltaTime);

			Vector3 moveVector = new Vector3(movement.GetLateral(), 0.0f, movement.GetForward()) * Time.smoothDeltaTime;

			if ((impactVector.magnitude >= 0.1f) && (moveVector.magnitude <= impactVector.magnitude))
			{
				// Gravity mid-air
				if (!controller.isGrounded)
				{
					impactVector.y = Mathf.Lerp(impactVector.y, (-movement.gravity * Time.smoothDeltaTime), 3 * Time.smoothDeltaTime);
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


		// Pause
		if (Input.GetButtonDown("Cancel"))
		{
			bPaused = !bPaused;
			pauseScreen.gameObject.SetActive(bPaused);

			if (bPaused)
			{
				Time.timeScale = 0f;
			}
			else
			{
				Time.timeScale = 1f;
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


	void TakeDamage(float value)
	{
		HealthBar healthBar = GetComponentInChildren<HealthBar>();
		if (healthBar != null)
		{
			// Damage
			Transform newDamageEffect = Instantiate(damageParticles, transform.position, Quaternion.identity);
			newDamageEffect.parent = gameObject.transform;
			Destroy(newDamageEffect.gameObject, 2f);

			int newHealth = Mathf.FloorToInt(healthBar.CurrentHealth() - value);
			healthBar.SetHealth(newHealth);

			// Health gone = kerploded
			if (newHealth <= 0f)
			{
				// Close player control
				movement.SetActive(false);
				movement.SetMoveCommand(Vector3.zero, true);
				camControl.SetActive(false);

				GameSystem game = FindObjectOfType<GameSystem>();
				if (game != null)
				{
					game.PlayerDied();
				}

				// Kerplosion
				MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
				foreach (MeshRenderer mesh in meshes)
				{
					GameObject meshGO = mesh.gameObject;
					meshGO.transform.parent = null;
					meshGO.transform.position += Random.insideUnitSphere * 0.6f;
					meshGO.transform.rotation *= Random.rotation;
				}

				Time.timeScale = 0f;
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
			if ((playerForward > 0f) || (playerLateral != 0.0f))
			{
				lookVector = Vector3.Lerp(lookVector, controller.velocity + onScreenOffset, Time.smoothDeltaTime * bodyTurnSpeed);
				bMoving = true;
			}

			// Towards camera -- moving back, or looking around
			float rotationSpeedScalar = 1f;
			float dotToLook = Vector3.Dot(transform.forward, Camera.main.transform.forward);
			bool craningLook = (dotToLook <= 0.99f);
			if (craningLook)
			{
				rotationSpeedScalar = dotToLook; // (1f - dotToLook);
			}
			if ((playerForward <= -0.1f) || craningLook)
			{
				lookVector = Vector3.Lerp(lookVector, Camera.main.transform.forward + onScreenOffset, Time.smoothDeltaTime * bodyTurnSpeed * rotationSpeedScalar);
				bMoving = true;
			}

			// Residual 'idle' rotation
			if (!bMoving)
			{
				Vector3 idleVector = transform.position + (transform.forward * 100.0f);
				idleVector.y = transform.position.y;

				lookVector = Vector3.Lerp(lookVector, idleVector, Time.smoothDeltaTime * bodyTurnSpeed);
			}

			lookVector.y = transform.position.y;
			transform.LookAt(lookVector);

			if (Head != null)
			{
				lerpAimVector = transform.position + (Camera.main.transform.forward * 100f);

				headVector = Vector3.Lerp(headVector, lerpAimVector, Time.smoothDeltaTime * lookSpeed);
				Head.transform.LookAt(headVector);
			}
		}
	}


	void UpdateGroundState()
	{
		// This ensures player won't get hung up on steep terrain

		bool canFall = true;
		GrapplingHook grappler = equippedItem.GetComponent<GrapplingHook>();
		if ((grappler != null) && (grappler.IsHookOut()))
		{
			canFall = false;
		}
		
		if (canFall)
		{
			groundHits = Physics.RaycastAll(transform.position, Vector3.up * -9999f);
			if (groundHits.Length > 0)
			{
				int numHits = groundHits.Length;
				for (int i = 0; i < numHits; i++)
				{
					RaycastHit thisHit = groundHits[i];
					if ((thisHit.transform != transform) && controller.isGrounded)
					{
						Vector3 surfaceNormal = thisHit.normal;
						float angleToSurface = Vector3.Angle(Vector3.up, surfaceNormal);
						if (angleToSurface > 50f)
						{
							Vector3 down = (-Vector3.up + (thisHit.normal * 0.5f)).normalized;
							movement.SetMoveCommand(down, true);
							movement.SetMoveScale(0.2f);
						}
						else
						{
							movement.SetMoveCommand(Vector3.zero, false);
							movement.SetMoveScale(1f);
						}
					}
				}
			}
		}
	}


	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.GetComponent<Terrain>())
		{
			// Ground slam FX
			if ((controller.velocity.y <= -10f) || (Mathf.Abs(controller.velocity.magnitude) >= 15f))
			{
				Transform newDropImpact = Instantiate(dropImpactParticles, transform.position + (Vector3.up * -1.5f), Quaternion.identity);
				Destroy(newDropImpact.gameObject, 5f);

				if (Mathf.Abs(controller.velocity.magnitude) >= movement.maxSpeed)
				{
					Transform newBoostImpact = Instantiate(boostImpactParticles, transform.position + (Vector3.up * -1.5f), transform.rotation);
					newBoostImpact.parent = transform;
					Destroy(newBoostImpact.gameObject, 15f);
				}
			}

			// Clear move command
			movement.SetMoveCommand(Vector3.zero, false);

			// Deactivate grappler reeling
			//GrapplingHook grappler = equippedItem.GetComponent<GrapplingHook>();
			//if (grappler != null)
			//{
			//	grappler.SetToolAlternateActive(false);
			//}
		}
	}


}
