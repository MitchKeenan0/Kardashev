using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyCharacter : MonoBehaviour
{
	public float moveSpeed = 10f;
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

		Vector3 toTarget = target.position - transform.position;
		float dotToTarget = Vector3.Dot(transform.right, toTarget.normalized);
		///Debug.Log("dotToTarget: " + dotToTarget);

		float strafeDir = Mathf.Clamp(dotToTarget, -1f, 1f);
		moveVector += transform.right * strafeDir * Time.deltaTime;

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

		controller.Move(moveVector * moveSpeed);

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
		Transform newSlamEffects = Instantiate(slamEffects, other.ClosestPoint(transform.position), Quaternion.identity);
		Destroy(newSlamEffects.gameObject, 1.0f);

		Collider[] nearColliders = Physics.OverlapSphere(transform.position, 15.0f);
		int numCols = nearColliders.Length;
		if (numCols > 0)
		{
			for (int i = 0; i < numCols; i++)
			{
				PlayerMovement player = nearColliders[i].gameObject.GetComponent<PlayerMovement>();
				if (player != null)
				{
					Vector3 slamVector = (player.transform.position - transform.position).normalized;
					player.GetComponent<CharacterController>().Move(slamVector * 50.0f);
					Debug.Log("SLAM");
				}
			}
		}
	}
}
