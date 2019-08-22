using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : MonoBehaviour
{
    
    void Start()
    {
		InitCity();
    }

    
    void InitCity()
	{
		Vector3 toCentre = transform.position - Vector3.zero;
		transform.LookAt(-toCentre, Vector3.up);
	}
}
