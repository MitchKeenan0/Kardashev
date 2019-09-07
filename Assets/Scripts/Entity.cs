using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
	public float moveSpeed = 1f;
	public Transform impactParticles;

	private Transform target = null;
	private Vector3 targetPosition;
	private Rigidbody rb;
	
	public void SetTarget(Transform newTarget)
	{
		target = newTarget;
	}


    void Start()
    {
		rb = GetComponent<Rigidbody>();
    }

	
    void Update()
    {
        if (target != null)
		{
			MoveToPosition();
		}
    }

	void MoveToPosition()
	{
		targetPosition = target.position;
		Vector3 moveForce = (targetPosition - transform.position).normalized;
		rb.AddForce(moveForce * moveSpeed);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (impactParticles != null)
		{
			Transform newImpact = Instantiate(impactParticles, transform.position, Quaternion.identity);
			Destroy(newImpact.gameObject, 1.0f);
		}
	}
}
