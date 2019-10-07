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
	public Transform[] limbs;
	public Transform slamEffects;
	public Transform groundSlamEffects;

	private float patienceTimer = 0f;
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

    
    void Start()
    {
		controller = GetComponent<CharacterController>();
		lookVector = transform.forward;

		if (limbs.Length > 0)
		{
			SetMoving(true);
		}

		if (target == null)
		{
			target = FindObjectOfType<PlayerMovement>().transform;
		}

		SetAttackingMode(false);
    }
    
    void Update()
    {
		VisionCheck();

		if (target != null)
		{
			UpdateMovement();
		}

		if (bActivated)
		{
			UpdateRotation();

			float disToTarget = Vector3.Distance(transform.position, target.position);
			if (disToTarget <= 1f)
			{
				SetAttackingMode(false);
			}
			if (disToTarget >= 100f)
			{
				SetAttackingMode(true);
			}
		}
	}

	void VisionCheck()
	{
		if (Physics.Linecast(transform.position, target.position, out visionHit))
		{
			if (visionHit.transform == target)
			{
				bActivated = true;
				bVisionCheck = true;
			}
			else
			{
				bVisionCheck = false;

				if (bAttacking)
				{
					patienceTimer += Time.deltaTime;
					if (patienceTimer >= 3f)
					{
						moveCommand = (Vector3.up * jumpSpeed) + (transform.forward * 5f * transform.localScale.magnitude);
						patienceTimer = 0f;
					}
				}
			}
		}
	}

	void SetAttackingMode(bool value)
	{
		bAttacking = value;
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

			// Strafe
			//if ((target != null) && target.GetComponent<CharacterController>())
			//{
			//	targetVelocity = target.GetComponent<CharacterController>().velocity * 0.3f;
			//}
			//Vector3 toTarget = (target.position + targetVelocity) - transform.position;
			//float dotToTarget = Vector3.Dot(transform.right, toTarget.normalized);
			//float strafeDir = Mathf.Clamp(dotToTarget * 2, -1f, 1f);
			//moveVector += transform.right * strafeDir * turnSpeed * Time.deltaTime;
		}

		// Gravity
		moveVector += (Vector3.up * -gravity);

		// Exterior forces
		if (moveCommand.magnitude > 0f)
		{
			moveVector += moveCommand;
			moveCommand = Vector3.Lerp(moveCommand, Vector3.zero, Time.smoothDeltaTime * 0.5f);
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
		moveVector *= Time.timeScale;
		controller.Move((moveVector * moveSpeed) * Time.smoothDeltaTime);
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
			newVector = transform.position + controller.velocity;
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
		if ((other.transform.parent != transform) && (other.gameObject != gameObject) && !other.CompareTag("Damage"))
		{
			// Ground slam
			if ((controller.velocity.y <= -(gravity * 0.5f)) && (groundSlamEffects != null))
			{

				// Upscaling mechanic
				transform.localScale *= growthScale;
				impactRange *= growthScale;

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
						Transform newSlamEffects = Instantiate(slamEffects, other.ClosestPoint(transform.position), Quaternion.identity);
						Destroy(newSlamEffects.gameObject, 5f);

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
			}
		}
	}
}
