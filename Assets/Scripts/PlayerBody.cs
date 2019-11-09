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
	public GameObject loadingPanel;
	public float lookSpeed = 2f;
	public float bodyTurnSpeed = 10f;
	public float turnWeight = 0.3f;
	public float recoveryTime = 0.3f;
	public float normalFOV = 90f;
	public float scopeFOV = 50f;
	public float scopeSensitivity = 0.5f;
	public float maxHealth = 100f;
	public Vector3 thirdPersonOffset;
	public Transform weaponPrefab1;
	public Vector3 weapon1Offset;
	public Transform damageParticles;
	public Transform dropImpactParticles;
	public Transform boostImpactParticles;
	
	private PlayerMovement movement;
	private Rigidbody rb;
	private SmoothMouseLook camControl;
	private GrapplingHook grapplingHook;
	private ItemBar itemBar;
	private GameObject equippedItem;
	private Vehicle vehicle;
	private Vehicle ownedVehicle;
	private GameObject recoverableTool;
	private PlayerMenus menus;
	private EquippedInfo info;
	private HUDAnimator hud;
	private SmoothMouseLook mouseLook;
	private AbilityChart abilities;

	private Vector3 lookVector;
	private Vector3 lerpAimVector;
	private Vector3 headVector;
	private float playerForward = 0f;
	private float playerLateral = 0f;
	private bool bPhysical = false;
	private bool bRiding = false;
	private bool bCanRecoverTool = false;
	private bool bCanGroundSlam = false;
	private float timeAtPhysical = 0f;
	private Vector3 impactVector = Vector3.zero;
	private RaycastHit groundHit;
	private bool bCursorInit = false;
	private float targetFOV = 0f;
	private float scopeSpeed = 1f;
	private float naturalSensitivity = 1f;

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

	public Vector3 GetVelocity()
	{
		if (bRiding)
		{
			return vehicle.GetComponent<Rigidbody>().velocity;
		}
		else
		{
			return rb.velocity;
		}
	}

	public void TakeSlam(Vector3 vector, float force, bool bDamage)
	{
		if (!bPhysical && !bRiding)
		{
			//movement.SetActive(false);
			impactVector = vector * force;
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
		if (value)
			mouseLook.SetOffset(thirdPersonOffset);
		else
			mouseLook.SetOffset(Vector3.zero);
	}

	public void SetBodyOffset(Vector3 value)
	{
		Body.localPosition = value;
	}

	public void SetScoped(bool value, float speed)
	{
		scopeSpeed = speed;
		if (value)
		{
			targetFOV = scopeFOV;
			mouseLook.SetSensitivity(scopeSensitivity);
		}
		else
		{
			targetFOV = normalFOV;
			mouseLook.SetSensitivity(naturalSensitivity);
		}
	}

	public GameObject GetEquippedItem()
	{
		return equippedItem;
	}


	void Start()
	{
		Time.timeScale = 1f;
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 98;

		structures = new List<StructureHarvester>();
		
		rb = GetComponent<Rigidbody>();
		movement = GetComponentInParent<PlayerMovement>();
		menus = GetComponentInChildren<PlayerMenus>();

		info = GetComponentInChildren<EquippedInfo>();
		itemBar = GetComponentInChildren<ItemBar>();
		camControl = FindObjectOfType<SmoothMouseLook>();
		hud = GetComponentInChildren<HUDAnimator>();
		mouseLook = FindObjectOfType<SmoothMouseLook>();
		naturalSensitivity = mouseLook.sensitivityX;

		lookVector = transform.position + transform.forward;
		transform.LookAt(lookVector);

		pauseScreen.SetActive(false);
		optionsScreen.SetActive(false);
		deathScreen.SetActive(false);
		fadeBlackScreen.SetActive(false);
		bCursorInit = false;

		normalFOV = Camera.main.fieldOfView;
		targetFOV = normalFOV;

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
			ItemSelectEvents();
			//if (!bRiding)
			//	UpdateRotation();
			if (bRiding)
			{
				transform.localRotation = Quaternion.Euler(Vector3.zero);
			}

			if (bRiding && (transform.localPosition != Vector3.zero))
			{
				transform.localPosition = Vector3.zero;
			}
			//else
			//{
			//	UpdateGroundState();
			//}

			if (!bCursorInit)
			{
				Cursor.lockState = CursorLockMode.Locked;
				bCursorInit = true;
			}

			// Updating slam
			if (!bRiding && bPhysical)
			{
				impactVector = Vector3.Lerp(impactVector, Vector3.zero, Time.smoothDeltaTime);
				Vector3 moveVector = new Vector3(movement.GetLateral(), 0.0f, movement.GetForward()) * Time.smoothDeltaTime;
				if ((impactVector.magnitude > 0.05f) && (moveVector.magnitude < impactVector.magnitude))
				{
					if (!movement.IsGrounded())
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

			// Scoping lerp
			float fov = Camera.main.fieldOfView;
			if (fov != targetFOV)
			{
				fov = Mathf.Lerp(fov, targetFOV, Time.smoothDeltaTime * 1.6f * scopeSpeed);
				Camera.main.fieldOfView = fov;
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
			// Harvesting structures
			if (structures.Count > 0)
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

			// Getting in/out of vehicles
			else if (vehicle != null)
			{
				if (!bRiding && (impactVector == Vector3.zero))
				{
					bRiding = true;
					SetMovementVehicle(true, vehicle);
					camControl.SetBody(vehicle.transform);
					vehicle.SetVehicleActive(true);
					SetThirdPerson(true);
					menus.SetRecallPromptActive(false);
				}
				else
				{
					bRiding = false;
					vehicle.SetVehicleActive(false);
					camControl.SetBody(transform);
					SetMovementVehicle(false, vehicle);
					SetThirdPerson(false);
					menus.SetRecallPromptActive(true);
				}

				/// set camera target here
			}
		}

		// Pickup thowing tools
		if (Input.GetButtonDown("Pickup"))
		{
			if ((recoverableTool != null) && recoverableTool.GetComponent<Spear>())
			{
				Collider[] nearbyObjs = Physics.OverlapSphere(transform.position, 15f);
				int gotSpears = 0;
				foreach (Collider col in nearbyObjs)
				{
					Spear spr = col.transform.GetComponent<Spear>();
					if (spr != null)
					{
						spr.RecoverSpear();
						gotSpears++;
					}
				}

				if (hud != null)
				{
					hud.SetSpearScore(gotSpears);
					hud.PlayAnimation("GetSpear");
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

			if (!bRiding && (ownedVehicle != null))
			{
				menus.SetVehiclePointerActive(ownedVehicle, true);
				menus.SetRecallPromptActive(false);
			}
		}

		if (Input.GetButtonUp("Recall"))
		{
			if (!bRiding && (ownedVehicle != null))
			{
				menus.SetVehiclePointerActive(ownedVehicle, false);
				menus.SetRecallPromptActive(true);
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

			if (info != null)
			{
				info.SetToolName("");
				info.SetToolReserve("");
			}

			equippedItem = null;
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
		Vector3 lastVelocity = ride.GetComponent<Rigidbody>().velocity;
		bRiding = value;
		vehicle = ride;

		if (value)
		{
			movement.SetVehicle(ride);
		}
		else
		{
			movement.SetVehicle(null);
		}

		if (value)
		{
			SetBodyOffset(Vector3.up);
			transform.parent = vehicle.footMountTransform;
			transform.localPosition = Vector3.up * 0.2f;
			transform.localRotation = vehicle.footMountTransform.rotation;
		}
		else
		{
			if (transform.parent != null)
			{
				transform.parent = null;
			}

			SetBodyOffset(Vector3.zero);

			movement.SetMoveCommand(lastVelocity * 10f, false);
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
				if (camControl != null)
				{
					camControl.gameObject.SetActive(false);
				}
				else
				{
					camControl = FindObjectOfType<SmoothMouseLook>();
				}

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
		if (rb != null)
		{
			Vector3 onScreenOffset = transform.position + (Camera.main.transform.forward * 100f);
			lookVector = Vector3.Lerp(lookVector, rb.velocity + onScreenOffset, Time.smoothDeltaTime * bodyTurnSpeed);
			lookVector.y = transform.position.y;
			transform.LookAt(lookVector);

			if (Head != null)
			{
				lerpAimVector = transform.position + (Camera.main.transform.forward * 100f);

				headVector = Vector3.Lerp(headVector, lerpAimVector, Time.smoothDeltaTime * lookSpeed);
				Head.transform.LookAt(headVector);
			}

			Debug.Log("Updating rotation at " + Time.time);
		}
	}


	void UpdateGroundState()
	{
		if (!bRiding && !grapplingHook.IsHookOut() && Physics.Raycast(transform.position, Vector3.down * 10f, out groundHit))
		{
			if ((groundHit.transform != transform) && movement.IsGrounded())
			{
				Vector3 surfaceNormal = groundHit.normal;
				float angleToSurface = Vector3.Angle(Vector3.up, surfaceNormal);
				if (angleToSurface > 80f)
				{
					Vector3 down = (-Vector3.up + (groundHit.normal * 0.5f)).normalized;
					movement.SetMoveCommand(down, true);
					movement.SetMoveScale(0.2f);
				}
				else
				{
					movement.SetMoveCommand(Vector3.zero, true);
					movement.SetMoveScale(1f);
				}
			}
		}

		if (movement.IsGrounded())
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
		bool solidHit = (rb != null)
			&& bCanGroundSlam
			&& !bRiding
			&& !other.gameObject.CompareTag("Player")
			&& !other.gameObject.GetComponent<Vehicle>();
		if (solidHit)
		{
			//Debug.Log("Character landing v: " + Mathf.Abs(controller.velocity.magnitude) + " on " + other.transform.name);

			// Ground slam FX
			if ((rb.velocity.y <= -5f) || (Mathf.Abs(rb.velocity.magnitude) <= 15f))
			{
				Transform newDropImpact = Instantiate(dropImpactParticles, transform.position + (Vector3.up * -1.5f), Quaternion.identity);
				Destroy(newDropImpact.gameObject, 5f);

				if (Mathf.Abs(rb.velocity.magnitude) >= (movement.maxSpeed) * 0.8f)
				{
					Transform newBoostImpact = Instantiate(boostImpactParticles, transform.position + (Vector3.up * -1.5f), transform.rotation);
					newBoostImpact.parent = transform;
					Destroy(newBoostImpact.gameObject, 15f);
				}
			}
		}
	}


}
