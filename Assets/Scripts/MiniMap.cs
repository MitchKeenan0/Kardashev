using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
	public Transform lookAt;
	public float height = 15f;
	public float offset = 15f;
	public float distance = 15f;

	private Camera cam;
	
	private Vector3 movePosition;

    void Start()
    {
		cam = GetComponent<Camera>();
    }

	public void SetLookObject(Transform value)
	{
		lookAt = value;
		movePosition.y = height;
		movePosition.x = offset;
		movePosition.z = -distance;
		transform.parent = lookAt;
		transform.localPosition = movePosition;
	}
}
