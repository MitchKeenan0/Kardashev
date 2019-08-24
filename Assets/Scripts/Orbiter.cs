﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbiter : MonoBehaviour
{
	public float rotationSpeed;

    void Start()
    {
        
    }
	
    void Update()
    {
		transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));
	}
}
