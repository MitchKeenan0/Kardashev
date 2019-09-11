using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyCharacter : MonoBehaviour
{
	public float moveSpeed = 10f;
	public float turnSpeed = 10f;
	public float gravity = 10f;
	public float slip = 0.001f;
	public Transform[] limbs;
	public Transform slamEffects;

	private bool bMoving = false;
	private Transform target;
	private CharacterController controller;
	private Vector3 lookVector;
	private Vector3 previousPosition;

    
    void Start()
    {
		controller = GetComponent<CharacterController>();
		lookVector = transform.forward;
		SetMoving(true);

		if (target == null)
		{
			target = FindObjectOfType<PlayerMovement>().transform;
		}
    }
    
    void Update()
    {
		UpdateMovement();
    }

	void UpdateMovement()
	{
		Vector3 moveVector = transform.forward;
		Vector3 targetVelocity = Vector3.zero;
		if (target.GetComponent<CharacterController>())
		{
			targetVelocity = target.GetComponent<CharacterController>().velocity * 0.5f;
		}

		Vector3 toTarget = (target.position + targetVelocity) - transform.position;

		// Strafe
		float dotToTarget = Vector3.Dot(transform.right, toTarget.normalized);
		float strafeDir = Mathf.Clamp(dotToTarget * 2, -1f, 1f);
		moveVector += transform.right * strafeDir * turnSpeed * Time.deltaTime;

		// Up/down for flyers
		float height = GetAltitude();
		if ((height + controller.velocity.y) <= 5.0f)
		{
			float noseUpScalar = Mathf.Clamp(Mathf.Abs(controller.velocity.y), 1f, 10f);
			moveVector += (Vector3.up * noseUpScalar * Time.deltaTime);
		}
		else if (height >= 10.0f)
		{
			moveVector -= Vector3.up * Time.deltaTime;
		}

		// Gravity
		//moveVector += Vector3.up * -gravity;

		// Slip
		Vector3 velo = transform.InverseTransformDirection(controller.velocity) * slip * Time.deltaTime;
		moveVector.x += velo.x;

		// Do the Movement!
		controller.Move(moveVector * moveSpeed);

		// Rotation
		lookVector = transform.position + controller.velocity;
		transform.LookAt(lookVector);
	}

	void SetMoving(bool value)
	{
		bMoving = value;

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
						limb.SetLimbActive(true, i);
					}
				}
			}
		}
	}

	float GetAltitude()
	{
		float result = 0.0f;
		RaycastHit hit;
		if (Physics.Raycast(transform.position + (Vector3.up * -5f), Vector3.up * -9999f, out hit, 9999f))
		{
			result = Vector3.Distance(transform.position, hit.point) + 5f;
		}

		return result;
	}

	private void OnTriggerEnter(Collider other)
	{
		if ((other.transform.parent != transform) && (other.gameObject != gameObject) && !other.CompareTag("Damage"))
		{
			Transform newSlamEffects = Instantiate(slamEffects, other.ClosestPoint(transform.position), Quaternion.identity);
			Destroy(newSlamEffects.gameObject, 1.5f);

			Collider[] nearColliders = Physics.OverlapSphere(transform.position, 5f);
			int numCols = nearColliders.Length;
			if (numCols > 0)
			{
				for (int i = 0; i < numCols; i++)
				{
					PlayerBody player = nearColliders[i].gameObject.GetComponent<PlayerBody>();
					if (player != null)
					{
						Vector3 slamDirection = (player.transform.position - transform.position);
						slamDirection.y = 0.0f;
						Vector3 slamVector = slamDirection.normalized + Vector3.up;
						player.TakeSlam(slamVector, 5.0f);
					}
				}
			}
		}
	}
}
