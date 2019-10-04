using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
	public float height = 15f;
	public float offset = 15f;
	public float distance = 15f;

	private Camera cam;
	private Transform player;
	private Vector3 movePosition;

    void Start()
    {
		player = FindObjectOfType<PlayerMovement>().transform;
		cam = GetComponent<Camera>();

		movePosition.y = height;
		movePosition.x = offset;
		movePosition.z = -distance;
		transform.parent = player;
		transform.localPosition = movePosition;

		//transform.rotation = Quaternion.Euler((player.position - movePosition));
    }
}
