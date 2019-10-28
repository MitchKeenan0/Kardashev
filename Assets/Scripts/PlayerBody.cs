using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBody : MonoBehaviour
{
	public Transform Body;
	public Transform Head;
	public Transform RightArm;
	public GameObject pauseScreen;
	public GameObject optionsScreen;
	public GameObject deathScreen;
	public GameObject fadeBlackScreen;
	public float lookSpeed = 2f;
	public float bodyTurnSpeed = 10f;
	public float turnWeight = 0.3f;
	public float recoveryTime = 0.3f;
	public float normalFOV = 90f;
	public float scopeFOV = 50f;
	public float maxHealth = 100f;
	public Vector3 thirdPersonOffset;
	public Transform weaponPrefab1;
	public Vector3 weapon1Offset;
	public Transform damageParticles;
	public Transform dropImpactParticles;
	public Transform boostImpactParticles;

	private CharacterController controller;
	private PlayerMovement movement;
	private Rigidbody rb;
	private CameraController camControl;
	private GrapplingHook grapplingHook;
	private ItemBar itemBar;
	private GameObject equippedItem;
	private Vehicle vehicle;
	private Vehicle ownedVehicle;
	private GameObject recoverableTool;
	private PlayerMenus menus;

	private Vector3 lookVector;
	private Vector3 lerpAimVector;
	private Vector3 headVector;
	private float playerForward = 0f;
	private float playerLateral = 0f;
	private bool bPhysical = false;
	private bool bRiding = false;
	private bool bCanRecoverTool = false;
	private bool bCanGroundSlam;
	private float timeAtPhysical = 0f;
	private Vector3 impactVector;
	private RaycastHit groundHit;

	private List<StructureHarvester> structures;
	public void SetStructure(StructureHarvester str, bool value)
	{
		if (value && !structures.Contains(str))
		{
			structures.Add(str);
		}
		
		if (!value && structures.Contains(str))
		{
			structures.Remove(str);
		}
	}
	
	public void SetRecovery(bool value, GameObject obj)
	{
		bCanRecoverTool = value;
		if (bCanRecoverTool)
		{
			recoverableTool = obj;
		}
	}

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
		if (!bPhysical && !bRiding)
		{
			movement.SetActive(false);

			impactVector = vector * force * 20f*Time.smoothDeltaTime;
			movement.impactMovement = impactVector;
			bPhysical = true;
			timeAtPhysical = Time.time;
		}

		if (bDamage)
		{
			TakeDamage(force);
		}
	}

	// Called on trigger enter from vehicle
	public void SetVehicle(Vehicle ride)
	{
		vehicle = ride;
		ownedVehicle = ride;
	}

	public Vehicle GetVehicle()
	{
		return vehicle;
	}

	public bool IsRiding()
	{
		return bRiding;
	}

	public void SetThirdPerson(bool value)
	{
		GameObject cam = FindObjectOfType<SmoothMouseLook>().gameObject;

		if (value)
		{
			cam.GetComponent<SmoothMouseLook>().SetOffset(thirdPersonOffset);
		}
		else
		{
			cam.GetComponent<SmoothMouseLook>().SetOffset(Vector3.zero);
		}
	}

	public void SetBodyOffset(Vector3 value)
	{
		Body.localPosition = value;
	}


	void Start()
	{
		Time.timeScale = 1f;
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 70;

		structures = new List<StructureHarvester>();

		controller = GetComponentInParent<CharacterController>();
		movement = GetComponentInParent<PlayerMovement>();
		menus = GetComponentInChildren<PlayerMenus>();

		itemBar = FindObjectOfType<ItemBar>();
		camControl = FindObjectOfType<CameraController>();
		rb = GetComponent<Rigidbody>();
		rb.isKinematic = true;

		lookVector = transform.position + transform.forward;
		transform.LookAt(lookVector);

		pauseScreen.SetActive(false);
		optionsScreen.SetActive(false);
		deathScreen.SetActive(false);
		fadeBlackScreen.SetActive(false);

		//EquipItem(4);
	}

	void Update()
	{
		if (Time.timeScale > 0f)
		{
			// Update vehile marker
			if (!bRiding && (ownedVehicle != null))
			{
				menus.UpdateVehiclePointer(ownedVehicle.transform.position);
			}

			UpdateInput();
			UpdateRotation();
			ItemSelectEvents();

			if (bRiding && (transform.localPosition != Vector3.zero))
			{
				transform.localPosition = Vector3.zero;
			}
			else
			{
				UpdateGroundState();
			}

			// Updating slam
			if (bPhysical)
			{
				impactVector = Vector3.Lerp(impactVector, Vector3.zero, Time.smoothDeltaTime);
				Vector3 moveVector = new Vector3(movement.GetLateral(), 0.0f, movement.GetForward()) * Time.smoothDeltaTime;
				if ((impactVector.magnitude > 0.05f) && (moveVector.magnitude < impactVector.magnitude))
				{
					if (!controller.isGrounded)
					{
						impactVector.y = Mathf.Lerp(impactVector.y, -movement.gravity * Time.smoothDeltaTime, 3 * Time.smoothDeltaTime);
					}

					movement.impactMovement = impactVector;
				}
				else
				{
					impactVector = Vector3.zero;
					bPhysical = false;
					movement.SetActive(true);
					movement.impactMovement = Vector3.zero;
				}
			}
		}
	}

	void UpdateInput()
	{
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
			else
			{
				EquipItem(1);
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
				if (tool != null && (Time.timeScale == 1f))
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
				if (tool != null && (Time.timeScale == 1f))
				{
					tool.SetToolAlternateActive(false);
				}
			}
		}

		// Interact
		if (Input.GetButtonDown("Interact"))
		{

			// Getting in/out of vehicles
			if (vehicle != null)
			{
				if (!bRiding && (impactVector == Vector3.zero))
				{
					bRiding = true;
					vehicle.SetVehicleActive(true);
					SetThirdPerson(true);
					SetMovementVehicle(true, vehicle);

					if (grapplingHook != null)
					{
						grapplingHook.SetControllerComponent(vehicle.GetComponent<CharacterController>());
					}
				}
				else
				{
					bRiding = false;
					vehicle.SetVehicleActive(false);
					SetThirdPerson(false);
					SetMovementVehicle(false, null);

					if (grapplingHook != null)
					{
						grapplingHook.SetControllerComponent(controller);
					}
				}
			}

			// Harvesting structures
			else if (structures.Count > 0)
			{
				if (structures.Count == 1)
				{
					structures[0].Disperse();
				}
				else
				{
					// Get the structure closest to center-screen
					Vector3 centerScreen = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)).direction.normalized;
					float lowestAngle = 180f;
					StructureHarvester nearestStr = null;
					int numStr = structures.Count;
					for (int i = 0; i < numStr; i++)
					{
						Vector3 toStr = (structures[i].transform.position - transform.position).normalized;
						float angleToStr = Vector3.Angle(centerScreen, toStr);
						if (angleToStr < lowestAngle)
						{
							lowestAngle = angleToStr;
							nearestStr = structures[i];
						}
					}

					if (nearestStr != null)
					{
						nearestStr.Disperse();
					}
				}
			}
		}

		// Pickup thowing tools
		if (Input.GetButtonDown("Pickup"))
		{
			if ((recoverableTool != null) && recoverableTool.GetComponent<Spear>())
			{
				Collider[] nearbyObjs = Physics.OverlapSphere(transform.position, 15f);
				foreach (Collider col in nearbyObjs)
				{
					Spear spr = col.transform.GetComponent<Spear>();
					if (spr != null)
					{
						spr.RecoverSpear();
					}
				}
			}
		}

		// Recall vehicle
		if (Input.GetButton("Recall"))
		{
			if (ownedVehicle == null)
			{
				ownedVehicle = FindObjectOfType<Vehicle>();
			}

			if (ownedVehicle != null)
			{
				menus.SetVehiclePointerActive(true);
			}

			if (ownedVehicle != null)
			{
				ownedVehicle.GetComponent<Rigidbody>().AddForce(
					(Vector3.up + (transform.position - ownedVehicle.transform.position))
					* Time.smoothDeltaTime * 15f);
			}
		}

		if (Input.GetButtonUp("Recall"))
		{
			menus.SetVehiclePointerActive(false);
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
			Tool tool = equippedItem.GetComponent<Tool>();
			if (tool != null)
			{
				if ((grapplingHook != null) && (tool.gameObject == grapplingHook.gameObject))
				{
					grapplingHook.DeactivateGrappler();
				}

				tool.SetToolAlternateActive(false);
				tool.SetToolActive(false);
			}
			equippedItem.transform.parent = null;
			equippedItem.gameObject.SetActive(false);
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
				newItem.SetActive(true);
				equippedItem = newItem;

				Tool newTool = newItem.transform.GetComponent<Tool>();
				if (newTool != null)
				{
					newTool.InitTool(transform);

					// Update name for HUD
					EquippedInfo info = FindObjectOfType<EquippedInfo>();
					if (info != null)
					{
						info.SetToolName(newTool.toolName);
						if (newTool.GetComponent<ThrowingTool>())
						{
							int reserve = newTool.GetComponent<ThrowingTool>().reserveAmmo;
							info.SetToolReserve(newTool.GetComponent<ThrowingTool>().reserveAmmo.ToString());
						}
						else
						{
							info.SetToolReserve("");
						}
					}
				}

				if (newTool.GetComponent<GrapplingHook>())
				{
					grapplingHook = newTool.GetComponent<GrapplingHook>();
				}
			}
		}
	}

	void SetMovementVehicle(bool value, Vehicle ride)
	{
		bRiding = value;
		vehicle = ride;
		movement.SetVehicle(ride);

		if (value)
		{
			SetBodyOffset(Vector3.up * (controller.height * 0.5f));
			movement.SetMoveScale(0f);

			transform.parent = vehicle.footMountTransform;
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
		}
		else
		{
			if (transform.parent != null)
			{
				Vector3 offset = (Camera.main.transform.position - transform.position).normalized;
				transform.position += offset * 3f;
				transform.parent = null;
			}

			SetBodyOffset(Vector3.zero);
			movement.SetMoveScale(1f);
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

			int newHealth = Mathf.FloorToInt(Mathf.Clamp(healthBar.CurrentHealth() - value, 0, maxHealth));
			healthBar.SetHealth(newHealth);

			// Health gone = kerploded
			if (newHealth <= 0f)
			{
				// Close player control
				impactVector = Vector3.zero;
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
				rotationSpeedScalar = (1f - dotToLook);
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
		if (Physics.Raycast(transform.position, Vector3.down * 10f, out groundHit))
		{
			if ((groundHit.transform != transform) && controller.isGrounded)
			{
				Vector3 surfaceNormal = groundHit.normal;
				float angleToSurface = Vector3.Angle(Vector3.up, surfaceNormal);
				if (angleToSurface > 50f)
				{
					Vector3 down = (-Vector3.up + (groundHit.normal * 0.5f)).normalized;
					movement.SetMoveCommand(down, true);
					movement.SetMoveScale(0.2f);
					//Debug.Log("Falling at " + Time.time);
				}
				else
				{
					movement.SetMoveCommand(Vector3.zero, true);
					movement.SetMoveScale(1f);
				}
			}
		}

		if (controller.isGrounded)
		{
			bCanGroundSlam = false;
		}
		else
		{
			bCanGroundSlam = true;
		}
	}


	private void OnTriggerEnter(Collider other)
	{
		bool solidHit = (controller != null)
			&& bCanGroundSlam
			&& !bRiding
			&& !other.gameObject.CompareTag("Player")
			&& !other.gameObject.GetComponent<Vehicle>();
		if (solidHit)
		{
			//Debug.Log("Character landing v: " + Mathf.Abs(controller.velocity.magnitude) + " on " + other.transform.name);

			// Ground slam FX
			if ((controller.velocity.y <= -5f) || (Mathf.Abs(controller.velocity.magnitude) <= 15f))
			{
				Transform newDropImpact = Instantiate(dropImpactParticles, transform.position + (Vector3.up * -1.5f), Quaternion.identity);
				Destroy(newDropImpact.gameObject, 5f);

				if (Mathf.Abs(controller.velocity.magnitude) >= (movement.maxSpeed) * 0.8f)
				{
					Transform newBoostImpact = Instantiate(boostImpactParticles, transform.position + (Vector3.up * -1.5f), transform.rotation);
					newBoostImpact.parent = transform;
					Destroy(newBoostImpact.gameObject, 15f);
				}
			}
		}
	}


}
