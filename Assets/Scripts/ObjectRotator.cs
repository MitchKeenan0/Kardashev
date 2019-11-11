using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
	public Quaternion rotator = Quaternion.identity;
	public bool bRandomStartRotation = true;

    void Start()
    {
		if (bRandomStartRotation)
			transform.rotation = Random.rotation;
    }
}
