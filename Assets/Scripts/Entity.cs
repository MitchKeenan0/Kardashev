using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
	public float moveSpeed = 1f;

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
}
