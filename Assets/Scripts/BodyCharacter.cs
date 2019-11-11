using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyCharacter : MonoBehaviour
{
	public Transform aggressionEffects;
	public float moveSpeed = 10f;
	public float turnSpeed = 10f;
	public float jumpSpeed = 100f;
	public float gravity = 10f;
	public float impactDamage = 10f;
	public float impactRange = 10f;
	public float growthScale = 1.162f;
	public float maxHealth = 100f;
	public Transform[] limbs;
	public Transform slamEffects;
	public Transform groundSlamEffects;
	public Vector3 spawnOffset = Vector3.zero;

	private float patienceTimer = 0f;
	private float health;
	private bool bMoving = false;
	private bool bGrounded = false;
	private bool bActivated = false;
	private bool bAttacking = false;
	private bool bVisionCheck = false;
	private Transform target;
	private CharacterController controller;
	private Vector3 lookVector;
	private Vector3 previousPosition;
	private Vector3 moveCommand = Vector3.zero;
	private Vector3 patrolPosition = Vector3.zero;
	private RaycastHit visionHit;

	public void SetMoveCommand(Vector3 value, bool bAbsolute)
	{
		if (bAbsolute)
		{
			moveCommand = value;
		}
		else
		{
			moveCommand += value;
		}
	}

	public void AddMoveCommand(Vector3 value)
	{
		moveCommand += value;
	}

	public void TakeDamage(float value)
	{
		health -= value;

		if (health <= 0f)
		{
			Spear[] spears = GetComponentsInChildren<Spear>();
			foreach (Spear spr in spears)
			{
				spr.transform.parent = null;
				spr.transform.localScale = Vector3.one;
				spr.SetPhysical(true);
			}

			Destroy(gameObject);
		}
		else
		{
			if (!bAttacking)
			{
				SetAttackingMode(true);
			}
		}
	}

    void Start()
    {
		controller = GetComponent<CharacterController>();
		lookVector = transform.forward;

		health = maxHealth;

		if (limbs.Length > 0)
		{
			SetMoving(true);
		}

		if (target == null)
		{
			PlayerMovement playa = FindObjectOfType<PlayerMovement>();
			if (playa != null)
				target = playa.transform;
		}

		SetAttackingMode(false);

		// Random movespeed trait
		moveSpeed *= Mathf.Pow(Random.Range(1f, 2f), 2f);
	}
    
    void Update()
    {
		if (target == null)
		{
			target = FindObjectOfType<PlayerMovement>().transform;
		}
		else
		{
			float disToTarget = Vector3.Distance(transform.position, target.position);
			if (!bAttacking && (disToTarget <= 1000f))
			{
				SetAttackingMode(true);
			}
			if (disToTarget >= 11000f)
			{
				Destroy(gameObject);
			}
		}

		VisionCheck();
		UpdateMovement();

		if (bActivated)
		{
			UpdateRotation();
		}
	}

	void VisionCheck()
	{
		if (target != null && Physics.Linecast(transform.position, target.position, out visionHit))
		{
			if ((visionHit.distance < 2000f) 
				&& (visionHit.transform == target) || (visionHit.transform == target.parent))
			{
				bActivated = true;
				bVisionCheck = true;
			}
			else
			{
				bVisionCheck = false;
			}
		}
	}

	void SetAttackingMode(bool value)
	{
		bAttacking = value;
		if (bAttacking)
		{
			bActivated = true;
		}

		if (aggressionEffects != null)
		{
			if (aggressionEffects.GetComponent<ParticleSystem>())
			{
				var em = aggressionEffects.GetComponent<ParticleSystem>().emission;
				em.enabled = value;
			}
		}
	}

	void UpdateMovement()
	{
		Vector3 moveVector = Vector3.zero;
		Vector3 targetVelocity = Vector3.zero;

		if (bActivated && bAttacking)
		{
			moveVector = transform.forward * moveSpeed;
		}

		// Gravity
		moveVector += (Vector3.up * -gravity);

		// Exterior forces
		if (moveCommand.magnitude > 0f)
		{
			moveVector += moveCommand;
			moveCommand = Vector3.Lerp(moveCommand, Vector3.zero, 5f*Time.smoothDeltaTime);
		}

		// Falling
		if (GetAltitude() >= 35f && (!controller.isGrounded))
		{
			if (bGrounded)
			{
				bGrounded = false;
			}

			moveVector.x = 0f;
			moveVector.z = 0f;

			foreach (Transform limb in limbs)
			{
				Limb thisLimb = limb.GetComponent<Limb>();
				thisLimb.SetLimbActive(false, thisLimb.oppositionOffset);
			}
		}
		else
		{
			if (!bGrounded)
			{
				foreach (Transform limb in limbs)
				{
					Limb thisLimb = limb.GetComponent<Limb>();
					thisLimb.SetLimbActive(true, thisLimb.oppositionOffset);
				}

				bGrounded = true;
			}
		}

		// Do the Movement!
		controller.Move(moveVector * moveSpeed * Time.smoothDeltaTime * Time.timeScale);
	}

	void UpdateRotation()
	{
		// Rotation
		Vector3 newVector;
		if (target != null)
		{
			newVector = target.position;
		}
		else
		{
			newVector = transform.position + controller.velocity + transform.forward;
		}

		lookVector = Vector3.Lerp(lookVector, newVector, Time.smoothDeltaTime * turnSpeed);
		lookVector.y = transform.position.y;

		transform.LookAt(lookVector);
	}

	void SetMoving(bool value)
	{
		bMoving = value;
		int offset = 0;

		if (value)
		{
			int numLimbs = limbs.Length;
			if (numLimbs > 0)
			{
				for(int i = 0; i < numLimbs; i++)
				{
					Limb limb = limbs[i].GetComponent<Limb>();
					if (limb != null)
					{
						limb.SetLimbActive(true, offset);

						offset++;
						if (offset > 1)
						{
							offset = 0;
						}
					}
				}
			}
		}
	}

	float GetAltitude()
	{
		float result = 0.0f;
		RaycastHit[] hits;
		hits = Physics.RaycastAll(transform.position, Vector3.up * -99999f);
		if (hits.Length > 0)
		{
			int numHits = hits.Length;
			for (int i = 0; i < numHits; i++)
			{
				if (hits[i].transform.GetComponent<Terrain>())
				{
					result = Vector3.Distance(transform.position, hits[i].point);
					break;
				}
			}
		}

		return result;
	}


	private void OnTriggerEnter(Collider other)
	{
		if ((other.transform.parent != transform) && (other.gameObject != gameObject) && !other.CompareTag("Damage") && !other.GetComponent<BodyCharacter>())
		{
			// Ground slam
			if ((controller.velocity.y <= -(gravity * 0.5f)) && (groundSlamEffects != null))
			{

				// Upscaling mechanic
				if (transform.localScale.magnitude < maxHealth)
				{
					transform.localScale *= growthScale;
					impactRange *= growthScale;
				}

				moveCommand = Vector3.zero;

				Transform newGroundSlam = Instantiate(groundSlamEffects, transform.position + (Vector3.up * -5f), Quaternion.identity);
				newGroundSlam.localScale = transform.localScale;

				Destroy(newGroundSlam.gameObject, 5f);

				// Damage player
				PlayerBody player = FindObjectOfType<PlayerBody>();
				if (bVisionCheck)
				{
					// Physics impulse
					Vector3 slamDirection = (player.transform.position - transform.position);
					if (Mathf.Abs(slamDirection.magnitude) <= impactRange)
					{
						slamDirection.y = 0.0f;
						Vector3 slamVector = slamDirection.normalized + Vector3.up;
						player.TakeSlam(slamVector, impactDamage, true);
					}
				}
			}

			// Bodily collide with things
			Collider[] nearColliders = Physics.OverlapSphere(transform.position, 10f);
			int numCols = nearColliders.Length;
			if (numCols > 0)
			{
				for (int i = 0; i < numCols; i++)
				{
					PlayerBody player = nearColliders[i].gameObject.GetComponent<PlayerBody>();
					if (player != null)
					{
						SetAttackingMode(false);
						bActivated = false;

						// Slam visuals
						Transform newSlamEffects = Instantiate(slamEffects, transform.position, Quaternion.identity);
						Destroy(newSlamEffects.gameObject, 5f);

						// Physics impulse
						Vector3 slamDirection = (player.transform.position - transform.position);
						if (Mathf.Abs(slamDirection.magnitude) <= impactRange)
						{
							slamDirection.y = 0.0f;
							Vector3 slamVector = slamDirection.normalized + Vector3.up;
							float dmg = impactDamage * Random.Range(0.8f, 1.2f);
							player.TakeSlam(slamVector, dmg, true);
						}
					}
				}
			}
		}
	}
}
