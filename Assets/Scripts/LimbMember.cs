using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbMember : MonoBehaviour
{
	public float buoyancy = 1f;
	private Rigidbody rb;

    void Start()
    {
		rb = GetComponent<Rigidbody>(); 
    }

	void FixedUpdate()
	{
		rb.AddForce(Vector3.up * buoyancy);
	}
}
