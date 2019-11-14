﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
	[Header("Body")]
	public Transform body;
	public Transform head;
	public Transform toolArm;
	public Collider bodyCollider;
	public float bodyRotationSpeed = 10f;

	[Header("Movement")]
	public float moveSpeed = 300f;
	public float moveAcceleration = 30f;
	public float maxSpeed = 3000f;
	public float ledgeAssistStrength = 7f;
	public float groundDrag = 20f;
	public float airDrag = 0.02f;
	public float jumpSpeed = 1500f;
	public float gravity = 50f;
	public float airControl = 0.15f;
	public float boostScale = 35000f;
	public float boostFalloff = 15f;
	public float boostCooldown = 0.35f;

	[Header("Tools")]
	public float aimSpeed = 5f;

	[Header("Health")]
	public float maxHealth = 100f;
	public float recoveryTime = 0.3f;

	[Header("Camera")]
	public float normalFOV = 100f;
	public float scopeFOV = 35f;
	public float scopeSensitivity = 0.3f;
	public Vector3 thirdPersonOffset = new Vector3(1.6f, 2f, -20f);

	[Header("Effects")]
	public Transform damageParticles;
	public Transform dropImpactParticles;
	public Transform boostImpactParticles;
	public Transform boostParticles;

	[Header("Sound")]
	public AudioClip boostSound;

	// Private refs..
	private Rigidbody rb;
	private SmoothMouseLook cam;
	private AudioSource audioSource;
	private Vehicle vehicle;
	private AbilityChart abilities;
	private ItemBar itemBar;
	private Menus menus;
	private HUD hud;
	private GameObject equippedItem;
	private GameObject recoverableTool;

	private Vector3 motion = Vector3.zero;
	private Vector3 motionRaw = Vector3.zero;
	private Vector3 boostMotion = Vector3.zero;
	private Vector3 jumpMotion = Vector3.zero;
	private Vector3 impactVector = Vector3.zero;
	private Vector3 moveCommand = Vector3.zero;
	private Vector3 impactMovement = Vector3.zero;
	private Vector3 bodyAimVector = Vector3.zero;
	private Vector3 deltaMovement = Vector3.zero;
	private Vector3 lastPosition = Vector3.zero;
	private Vector3 toolAimVector = Vector3.zero;

	private float moveScale = 1f;
	private float currentForward = 0;
	private float currentLateral = 0;
	private float timeBoostedLast = 0f;
	private float grappleSpeed = 0f;
	private float timeAtPhysical = 0f;
	private float naturalSensitivity = 1f;
	private float targetFOV = 0f;
	private float scopeSpeed = 1f;
	private float ledgeAssistFront = 0f;
	private float ledgeAssistBack = 0f;

	private bool bActive = true;
	private bool bInputEnabled = true;
	private bool bGrappling = false;
	private bool bInVehicle = false;
	private bool bGrounded = false;
	private bool bJumping = false;
	private bool bGroundBoosting = false;
	private bool bPhysical = false;
	private bool bCanRecoverTool = false;
	private bool bCursorInit = false;
	private bool bIsBot = false;

	private RaycastHit groundHit;
	private RaycastHit[] centerHits;
	private RaycastHit[] frontHits;
	private RaycastHit[] leftHits;
	private RaycastHit[] rightHits;

	private List<StructureHarvester> structures;

	private void Start()
    {
		Time.timeScale = 1f;
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 98;

		rb = GetComponent<Rigidbody>();
		abilities = GetComponent<AbilityChart>();
		audioSource = GetComponent<AudioSource>();
		cam = FindObjectOfType<SmoothMouseLook>();
		hud = FindObjectOfType<HUD>();
		menus = FindObjectOfType<Menus>();
		structures = new List<StructureHarvester>();

		naturalSensitivity = cam.sensitivityX;
		Camera.main.fieldOfView = normalFOV;
		targetFOV = normalFOV;
		bCursorInit = false;
	}

    private void Update()
    {
		UpdateBoost();
		UpdateMovement();
		if (!bIsBot)
			UpdateBodyRotation();
		if (equippedItem != null)
			AimTool();
	}

	private void FixedUpdate()
	{
		MovementPhysics();
		GroundAssist();
	}

	void MovementPhysics()
	{
		lastPosition = rb.position;

		// Motion includes boost and external forces, "movecommand"
		rb.AddForce(motion * Time.fixedDeltaTime * Time.timeScale);

		if (bJumping)
		{
			rb.AddForce(Vector3.up * jumpSpeed, ForceMode.Force);
			abilities.IncreaseAbility(1, 10);
			bJumping = false;
		}

		if (!bGrounded)
		{
			rb.AddForce(Vector3.down * gravity * Time.fixedDeltaTime);
		}

		if ((rb.position - lastPosition) != Vector3.zero)
			deltaMovement = rb.position - lastPosition;
	}

	void GroundAssist()
	{
		Vector3 centreOrigin = transform.position + Vector3.up;
		Vector3 forwardOrigin = centreOrigin + body.forward;
		Vector3 leftOrigin = centreOrigin + -body.right;
		Vector3 rightOrigin = centreOrigin + body.right;
		Vector3 down = Vector3.down * 100f;

		float centerDepth = 0f;
		centerHits = Physics.RaycastAll(centreOrigin, down, 10f);
		if (centerHits.Length > 0)
		{
			float shallowDepth = 999f;
			foreach (RaycastHit hit in centerHits)
			{
				if (hit.transform != transform)
				{
					if (hit.distance < shallowDepth)
						shallowDepth = hit.distance;
				}
			}

			centerDepth = shallowDepth;
		}

		float frontDepth = 0f;
		frontHits = Physics.RaycastAll(forwardOrigin, down, 10f);
		if (frontHits.Length > 0)
		{
			float shallowDepth = 999f;
			foreach (RaycastHit hit in frontHits)
			{
				if (hit.transform != transform)
				{
					if (hit.distance < shallowDepth)
						shallowDepth = hit.distance;
				}
			}

			frontDepth = shallowDepth;
		}

		float leftDepth = 0f;
		leftHits = Physics.RaycastAll(leftOrigin, down, 10f);
		if (leftHits.Length > 0)
		{
			float shallowDepth = 999f;
			foreach (RaycastHit hit in leftHits)
			{
				if (hit.transform != transform)
				{
					if (hit.distance < shallowDepth)
						shallowDepth = hit.distance;
				}
			}

			leftDepth = shallowDepth;
		}

		float rightDepth = 0f;
		rightHits = Physics.RaycastAll(rightOrigin, down, 10f);
		if (rightHits.Length > 0)
		{
			float shallowDepth = 999f;
			foreach (RaycastHit hit in rightHits)
			{
				if (hit.transform != transform)
				{
					if (hit.distance < shallowDepth)
						shallowDepth = hit.distance;
				}
			}

			rightDepth = shallowDepth;
		}

		// Assist movement to get over ledges
		if ( ((currentForward > 0f) && (frontDepth < centerDepth))
					|| ((currentLateral != 0f) && ((leftDepth < centerDepth) || (rightDepth < centerDepth))) )
		{
			Vector3 flatDelta = deltaMovement;
			flatDelta.y = 0f;
			
			float forwardHeightDifference = Mathf.Abs(frontDepth - centerDepth);
			float lateralHeightDifference = Mathf.Abs(leftDepth - rightDepth);

			Vector3 moveAssistVector = (Vector3.up * (forwardHeightDifference + lateralHeightDifference)).normalized;

			float groundStateScalar = 1f;
			if (bGrounded)
				groundStateScalar = ledgeAssistStrength;

			float assistScalar = (forwardHeightDifference > lateralHeightDifference) ? forwardHeightDifference : lateralHeightDifference;

			rb.MovePosition(
				rb.position
				+ (flatDelta * ledgeAssistStrength)
				+ (moveAssistVector * assistScalar * ledgeAssistStrength * groundStateScalar * Time.deltaTime)
			);
		}

		// Grounded boolean
		bGrounded = (centerDepth < 2.1f) || (frontDepth < 2.1f) || (leftDepth < 2.1f) || (rightDepth < 2.1f);
		if (bGrounded)
			rb.drag = groundDrag;
		else
			rb.drag = airDrag;
	}

	void UpdateBodyRotation()
	{
		Vector3 bodyAimVector = body.position + (cam.cam.forward * 10f);
		bodyAimVector = Vector3.Lerp(bodyAimVector, bodyAimVector, Time.smoothDeltaTime * bodyRotationSpeed);
		bodyAimVector.y = transform.position.y;
		body.LookAt(bodyAimVector);
	}

	void UpdateMovement()
	{
		if (bIsBot)
		{
			motionRaw = moveScale * ((body.forward * currentForward)
			+ (body.right * currentLateral)).normalized;
		}
		else
		{
			motionRaw = moveScale * ((Camera.main.transform.forward * currentForward)
			+ (Camera.main.transform.right * currentLateral)).normalized;
		}

		if (bInVehicle)
		{
			transform.localPosition = Vector3.up;
			transform.localRotation = Quaternion.identity;
			vehicle.SetMoveInput(currentForward, currentLateral);
		}
		else
		{
			Vector3 movementVector = motionRaw * maxSpeed;
			if (!bGrounded)
				movementVector *= airControl;
			else
				movementVector *= groundDrag;
			movementVector.y = 0f;
			motion = Vector3.Lerp(motion, movementVector, Time.deltaTime * moveAcceleration);
			motion += boostMotion;
			motion += impactMovement;
			motion += moveCommand;
			moveCommand = Vector3.Lerp(moveCommand, Vector3.zero, Time.fixedDeltaTime);
		}
	}

	public void SetForward(float value)
	{
		currentForward = value;
	}

	public void SetLateral(float value)
	{
		currentLateral = value;
	}

	public void Jump()
	{
		if (!bJumping && (bGrounded || bGrappling))
		{
			bJumping = true;
		}
	}

	void UpdateBoost()
	{
		if (boostMotion.magnitude > 0f)
		{
			boostMotion = Vector3.Lerp(boostMotion, Vector3.zero, Time.smoothDeltaTime * boostFalloff);
			if (bGroundBoosting && !bGrounded)
			{
				bGroundBoosting = false;
				boostMotion *= 0.333f;
			}
		}
	}

	public void Boost()
	{
		if ((Time.time >= (timeBoostedLast + boostCooldown)) && ((currentForward != 0f) || (currentLateral != 0f)))
		{
			Vector3 boostRaw = ((Camera.main.transform.forward * currentForward)
			+ (Camera.main.transform.right * currentLateral)).normalized;
			boostRaw.y *= -0.1f;

			// Redirective boost
			Vector3 currentV = rb.velocity;
			Vector3 normalV = currentV.normalized;
			Vector3 normalB = boostRaw.normalized;
			float lateralDot = Vector3.Dot(normalV, normalB);
			if (lateralDot < 0f)
			{
				boostRaw.x += ((currentV.x * -2f) * Time.smoothDeltaTime);
				boostRaw.z += ((currentV.z * -2f) * Time.smoothDeltaTime);
			}

			// Help getting across ground
			if (bGrounded)
			{
				boostRaw *= 3f;
				bGroundBoosting = true;
			}

			boostMotion = (boostRaw * boostScale);
			timeBoostedLast = Time.time;

			// Effects
			if (boostSound != null)
				audioSource.PlayOneShot(boostSound);
			SpawnBoost();

			// Boost ability leveling
			abilities.IncreaseAbility(2, 10);
		}
	}

	void SpawnBoost()
	{
		if (boostParticles != null)
		{
			Transform newBoost = Instantiate(boostParticles, transform.position, Quaternion.Euler(rb.velocity));
			newBoost.parent = Camera.main.transform;
			newBoost.localPosition = Vector3.forward * 1.5f;
			Destroy(newBoost.gameObject, 3f);
		}
	}

	public void SetMoveCommand(Vector3 value, bool bOverride)
	{
		if (bInVehicle && (vehicle != null))
		{
			vehicle.SetMoveCommand(value, bOverride);
		}
		else
		{
			if (bOverride)
			{
				moveCommand = value;
			}
			else
			{
				moveCommand += value;
			}
		}
	}

	void CheckGround()
	{
		if (Physics.Raycast(transform.position, Vector3.down * 20000f, out groundHit))
		{
			if (!groundHit.transform.GetComponent<Vehicle>()
				&& !groundHit.transform.GetComponent<Character>())
			{
				bGrounded = (groundHit.distance < 1.2f);
				if (bGrounded)
					rb.drag = groundDrag;
				else
					rb.drag = airDrag;
			}
		}
	}

	public bool IsGrounded()
	{
		return bGrounded;
	}

	public void PrimaryTrigger(bool down)
	{
		// Trigger down
		if (down)
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
		else
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
	}

	public void AlternateTrigger(bool down)
	{
		if (down)
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
		else
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
	}

	public void Interact()
	{
		// Harvesting structures
		if (structures.Count > 0)
		{
			HarvestArtifact();
		}

		// Getting in/out of vehicles
		else if (vehicle != null)
		{
			if (!bInVehicle && (impactVector == Vector3.zero))
			{
				MountVehicle(true);
			}
			else
			{
				MountVehicle(false);
			}
		}
	}

	public void PickupItem()
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

	public void SetRecovery(bool value, GameObject obj)
	{
		bCanRecoverTool = value;
		if (bCanRecoverTool)
		{
			recoverableTool = obj;
		}
	}

	public void RecallTrigger(bool value)
	{
		if (value)
		{
			if (vehicle == null)
				vehicle = FindObjectOfType<Vehicle>();

			if (!bInVehicle && (vehicle != null))
			{
				hud.SetVehiclePointerActive(vehicle, true);
				hud.SetRecallPromptActive(false);
			}
		}
		else
		{
			if (!bInVehicle && (vehicle != null))
			{
				hud.SetVehiclePointerActive(vehicle, false);
				hud.SetRecallPromptActive(true);
			}
		}
	}

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

	void HarvestArtifact()
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

	void MountVehicle(bool value)
	{
		if (value)
		{
			SetVehicle(true, vehicle);
			vehicle.SetVehicleActive(true);
			hud.SetRecallPromptActive(false);
			///SetThirdPerson(true);
		}
		else
		{
			vehicle.SetVehicleActive(false);
			SetVehicle(false, vehicle);
			hud.SetRecallPromptActive(true);
			///SetThirdPerson(false);
		}
	}

	public void SetVehicle(bool value, Vehicle ride)
	{
		bInVehicle = value;
		vehicle = ride;

		if (value)
		{
			motion = Vector3.zero;
			bodyCollider.enabled = false;
			rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
			rb.isKinematic = true;
			rb.useGravity = false;
			rb.detectCollisions = false;
			rb.drag = groundDrag;
			moveCommand = Vector3.zero;
			transform.parent = vehicle.footMountTransform;
			transform.localPosition = Vector3.up;
			transform.localRotation = vehicle.footMountTransform.rotation;
			///SetBodyOffset(Vector3.up);
			///cam.SetBody(vehicle.transform);
		}
		else if (ride != null)
		{
			if (transform.parent != null)
				transform.parent = null;
			bodyCollider.enabled = true;
			rb.isKinematic = false;
			rb.useGravity = true;
			rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			rb.detectCollisions = true;
			rb.drag = airDrag;
			Vector3 lastVelocity = ride.GetComponent<Rigidbody>().velocity;
			moveCommand += lastVelocity;
			///SetBodyOffset(Vector3.zero);
			///cam.SetBody(transform);
		}
	}

	public void EquipItem(int id)
	{
		// Unequip the current item
		if (equippedItem != null)
		{
			Tool tool = equippedItem.GetComponent<Tool>();
			if (tool != null)
			{
				tool.SetToolAlternateActive(false);
				tool.SetToolActive(false);
			}

			hud.SetToolInfo("", "");
			equippedItem.transform.parent = null;
			equippedItem.gameObject.SetActive(false);
			equippedItem = null;
		}

		// Retrieve new item
		if (hud.GetTool(id) != null)
		{
			GameObject newItem = hud.GetTool(id);
			if (newItem != null)
			{
				newItem.transform.parent = toolArm;
				newItem.transform.localPosition = Vector3.zero;
				newItem.transform.localRotation = Quaternion.identity;

				newItem.SetActive(true);
				equippedItem = newItem;

				// Init tool and HUD info
				Tool newTool = newItem.transform.GetComponent<Tool>();
				if (newTool != null)
				{
					newTool.InitTool(transform);
					if (newTool.GetComponent<ThrowingTool>())
					{
						hud.SetToolInfo(newTool.toolName, newTool.GetComponent<ThrowingTool>().reserveAmmo.ToString());
					}
					else
					{
						hud.SetToolInfo(newTool.toolName, "");
					}
				}
			}
		}
	}

	public void EquipObject(GameObject obj)
	{
		obj.transform.parent = toolArm;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;

		obj.SetActive(true);
		equippedItem = obj;

		Tool newTool = obj.transform.GetComponent<Tool>();
		if (newTool != null)
		{
			newTool.InitTool(transform);
		}
	}

	public void AimTool()
	{
		// Super hacky head rotation
		if (!bIsBot)
		{
			head.transform.LookAt(head.position + (cam.cam.forward * 100f));
		}

		Vector3 lerpAimVector = transform.position + (head.forward * 100f);
		toolAimVector = Vector3.Lerp(toolAimVector, lerpAimVector, Time.deltaTime * aimSpeed);

		// Close range needs some adjustment
		RaycastHit aimHit;
		if (Physics.Linecast(head.position, toolAimVector, out aimHit))
		{
			if (aimHit.distance > 2f)
			{
				toolAimVector = aimHit.point + (Vector3.down * 0.3f);
			}
		}

		toolArm.transform.LookAt(toolAimVector);
	}

	public GameObject GetEquippedItem()
	{
		return equippedItem;
	}

	public void TakeDamage(float value)
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
				if (cam != null)
				{
					cam.gameObject.SetActive(false);
				}
				else
				{
					cam = FindObjectOfType<SmoothMouseLook>();
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

	public void SetBodyOffset(Vector3 value)
	{
		body.localPosition = value;
	}

	public void SetGrappling(bool value, float topSpeed)
	{
		bGrappling = value;
		grappleSpeed = topSpeed;
	}

	public bool IsRiding()
	{
		return bInVehicle;
	}

	public void SetScoped(bool value, float speed)
	{
		scopeSpeed = speed;
		if (value)
		{
			targetFOV = scopeFOV;
			cam.SetSensitivity(scopeSensitivity);
		}
		else
		{
			targetFOV = normalFOV;
			cam.SetSensitivity(naturalSensitivity);
		}
	}

	public void SetBotControl(bool value)
	{
		bIsBot = value;
	}

	public bool IsBot()
	{
		return bIsBot;
	}

	private void OnTriggerEnter(Collider other)
	{
		bool solidHit = (rb != null)
			&& !bInVehicle
			&& !other.gameObject.CompareTag("Player")
			&& !other.gameObject.GetComponent<Vehicle>();
		if (solidHit)
		{
			//Debug.Log("Character landing v: " + Mathf.Abs(controller.velocity.magnitude) + " on " + other.transform.name);

			// Ground slam FX
			if ((rb.velocity.y <= -5f) || (Mathf.Abs(rb.velocity.magnitude) >= 15f))
			{
				if (dropImpactParticles != null)
				{
					Transform newDropImpact = Instantiate(dropImpactParticles, transform.position + (Vector3.up * -1.5f), Quaternion.identity);
					Destroy(newDropImpact.gameObject, 5f);

					if (boostImpactParticles != null)
					{
						if (Mathf.Abs(rb.velocity.magnitude) >= maxSpeed * 0.8f)
						{
							Transform newBoostImpact = Instantiate(boostImpactParticles, transform.position + (Vector3.up * -1.5f), transform.rotation);
							newBoostImpact.parent = transform;
							Destroy(newBoostImpact.gameObject, 5f);
						}
					}
				}
			}
		}
	}
}
