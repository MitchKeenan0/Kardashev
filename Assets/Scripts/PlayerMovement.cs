using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public AudioClip boostSound;
	public Transform boostParticles;
	public Collider bodyCollider;
	public float moveSpeed = 1.0f;
	public float moveAcceleration = 1.0f;
	public float maxSpeed = 10.0f;
	public float groundDrag = 10f;
	public float airDrag = 0.01f;
	public float jumpSpeed = 1.0f;
	public float gravity = 9.8f;
	public float airControl = 1f;
	public float boostScale = 5.0f;
	public float boostFalloff = 3f;
	public float boostCooldown = 1.5f;
	public Vector3 moveCommand = Vector3.zero;
	public Vector3 impactMovement = Vector3.zero;
	
	private Rigidbody rb;
	private AudioSource audioSoc;
	private PlayerBody body;
	private Vehicle vh;
	private AbilityChart abilities;
	private RaycastHit groundHit;
	private float moveScale = 1f;
	private float currentForward = 0;
	private float currentLateral = 0;
	private float lastForward = 0;
	private float lastLateral = 0;
	private float timeBoostedLast = 0f;
	private Vector3 motion = Vector3.zero;
	private Vector3 motionRaw = Vector3.zero;
	private Vector3 boostMotion = Vector3.zero;
	private Vector3 jumpMotion = Vector3.zero;
	private bool bActive = true;
	private bool bInputEnabled = true;
	private bool bGrappling = false;
	private bool bInVehicle = false;
	private float grappleSpeed = 0f;
	private bool bGrounded = false;
	private bool bJumping = false;

	private IEnumerator jumpBotCoroutine;
	IEnumerator JumpBot(float intervalTime)
	{
		while (true)
		{
			yield return new WaitForSeconds(intervalTime);
			Jump();
		}
	}

	void Start()
	{
		Time.timeScale = 1f;
		Cursor.visible = false;

		rb = GetComponent<Rigidbody>();
		rb.centerOfMass = Vector3.down * 1.5f;
		rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		abilities = GetComponent<AbilityChart>();
		audioSoc = GetComponent<AudioSource>();
		body = GetComponent<PlayerBody>();

		// Jump bot
		//jumpBotCoroutine = JumpBot(0.1f);
		//StartCoroutine(jumpBotCoroutine);
	}

	void Update()
	{
		if (Time.timeScale > 0f)
		{
			lastForward = currentForward;
			lastLateral = currentLateral;

			if (bInputEnabled)
			{
				currentForward = Input.GetAxisRaw("Vertical");
				currentLateral = Input.GetAxisRaw("Horizontal");

				if (!IsRiding())
				{
					CheckGround();
					if (bActive)
					{
						UpdateBoost();
						UpdateMovement();
					}
				}
				else
				{
					if (vh != null)
					{
						vh.SetMoveInput(currentForward, currentLateral);
						if (Input.GetButtonDown("Jump"))
						{
							vh.JumpVehicle();
						}
					}
				}
			}
			else
			{
				currentForward = 0f;
				currentLateral = 0f;
			}

			// Inform body for rotations
			if (currentForward != lastForward)
			{
				body.SetForward(currentForward);
			}
			if (currentLateral != lastLateral)
			{
				body.SetLateral(currentLateral);
			}
		}
	}

	void FixedUpdate()
	{
		rb.AddForce(motion * Time.fixedDeltaTime * Time.timeScale);

		if (bJumping)
		{
			rb.AddForce(Vector3.up * jumpSpeed);
			abilities.IncreaseAbility(1, 10);
			bJumping = false;
		}

		if (!bGrounded)
		{
			rb.AddForce(Vector3.down * gravity * Time.fixedDeltaTime);
		}
	}

	void SpawnBoost()
	{
		Transform newBoost = Instantiate(boostParticles, transform.position, Quaternion.Euler(rb.velocity));
		newBoost.parent = Camera.main.transform;
		newBoost.localPosition = Vector3.forward * 1.5f;
		Destroy(newBoost.gameObject, 3f);
	}

	void UpdateBoost()
	{
		if ((Input.GetButtonDown("Boost"))/// || (Input.GetButtonDown("Jump") && !bGrounded))
			&& (boostMotion.magnitude <= 1f))
		{
			Boost();
		}

		if (boostMotion.magnitude > 0f)
		{
			boostMotion = Vector3.Lerp(boostMotion, Vector3.zero, Time.smoothDeltaTime * boostFalloff);
		}
	}

	void Boost()
	{
		// New boost to be fed into UpdateMovement
		float topSpeed = (maxSpeed + jumpSpeed);
		if (bGrappling)
		{
			topSpeed += grappleSpeed;
		}

		if (rb.velocity.magnitude <= topSpeed)
		{
			if ((Time.time >= (timeBoostedLast + boostCooldown)) && ((currentForward != 0f) || (currentLateral != 0f)))
			{
				audioSoc.PlayOneShot(boostSound);
				SpawnBoost();

				Vector3 boostRaw = ((Camera.main.transform.forward * currentForward)
				+ (Camera.main.transform.right * currentLateral)).normalized;

				boostRaw.y *= -0.1f;

				Vector3 currentV = rb.velocity;
				Vector3 normalV = currentV.normalized;
				Vector3 normalB = boostRaw.normalized;
				float lateralDot = Vector3.Dot(normalV, normalB);
				if (lateralDot < 0f)
				{
					boostRaw.x += ((currentV.x * -2f) * Time.smoothDeltaTime);
					boostRaw.z += ((currentV.z * -2f) * Time.smoothDeltaTime);
				}

				boostMotion = (boostRaw * boostScale);
				timeBoostedLast = Time.time;

				// Boost ability leveling
				abilities.IncreaseAbility(2, 10);
			}
		}
	}

	void UpdateMovement()
	{
		motionRaw = moveScale * ((Camera.main.transform.forward * currentForward)
			+ (Camera.main.transform.right * currentLateral)).normalized;
		Vector3 movementVector = Vector3.zero;
		if (bGrounded)
			movementVector = motionRaw * (maxSpeed + grappleSpeed) * groundDrag;
		movementVector.y = 0f;
		motion = Vector3.Lerp(motion, movementVector, Time.deltaTime * moveAcceleration);

		if (Input.GetButtonDown("Jump"))
		{
			Jump();
		}

		motion += moveCommand;
		motion += boostMotion;
		motion += impactMovement;

		moveCommand = Vector3.Lerp(moveCommand, Vector3.zero, Time.fixedDeltaTime);
	}

	void Jump()
	{
		if (!bJumping && (bGrounded || bGrappling))
		{
			// This gets fed to FixedUpdate for actual jump
			bJumping = true;
		}	
	}

	void CheckGround()
	{
		if (Physics.Raycast(transform.position, Vector3.down * 20000f, out groundHit))
		{
			if (!groundHit.transform.GetComponent<Vehicle>() 
				&& !groundHit.transform.GetComponent<PlayerBody>())
			{
				bGrounded = (groundHit.distance < 1.25f); /// magic number , must be replaced!
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

	public bool IsRiding()
	{
		return bInVehicle;
	}
	public Vehicle GetVehicle()
	{
		return vh;
	}
	public void SetVehicle(Vehicle value)
	{
		vh = value;
		if (vh != null)
		{
			bInVehicle = true;
			moveCommand = Vector3.zero;
			motion = Vector3.zero;
			bodyCollider.enabled = false;
			rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
			rb.isKinematic = true;
			rb.useGravity = false;
			rb.detectCollisions = false;
			rb.drag = groundDrag;
		}
		else
		{
			bInVehicle = false;
			bodyCollider.enabled = true;
			rb.isKinematic = false;
			rb.useGravity = true;
			rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			rb.detectCollisions = true;
			rb.drag = airDrag;
		}
	}

	public float GetForward()
	{
		return currentForward;
	}
	public float GetLateral()
	{
		return currentLateral;
	}

	public void SetMoveCommand(Vector3 value, bool bOverride)
	{
		if (bInVehicle)
		{
			vh.SetMoveCommand(value, bOverride);
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

	public void SetGrappling(bool value, float topSpeed)
	{
		bGrappling = value;
		grappleSpeed = topSpeed;
	}

	public void SetActive(bool value)
	{
		bActive = value;
		rb.isKinematic = !value;
		rb.detectCollisions = value;

		if (!bActive)
		{
			currentForward = 0;
			currentLateral = 0;
			lastForward = 0;
			lastLateral = 0;
		}
	}

	public void EnableInput(bool value)
	{
		bInputEnabled = value;
	}

	public void SetMoveScale(float value)
	{
		moveScale = value;
	}
}
