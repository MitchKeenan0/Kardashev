using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbMember : MonoBehaviour
{
	public float buoyancy = 1f;
	private Rigidbody rb;
	private bool bFallen = false;

    void Start()
    {
		rb = GetComponent<Rigidbody>(); 
    }

	void FixedUpdate()
	{
		if (!bFallen)
		{
			rb.AddForceAtPosition(Vector3.up * buoyancy, transform.position + transform.up);
		}
	}

	public void SetFallen(bool value)
	{
		bFallen = value;
	}
}
