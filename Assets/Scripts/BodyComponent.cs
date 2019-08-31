using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyComponent : MonoBehaviour
{
	public float attractionForce = 10f;
	public float attractionRange = 1f;

	private List<Transform> linkedObjects;
	private float updateTimer = 0f;

	// Start is called before the first frame update
	void Start()
    {
		linkedObjects = new List<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
		UpdateObjects();

		float deltaTime = Time.deltaTime;
		updateTimer += deltaTime;
		if (updateTimer >= 0.2f)
		{
			CheckSurroundings();
			updateTimer = 0.0f - deltaTime;
		}
    }

	void UpdateObjects()
	{
		int objsNum = linkedObjects.Count;
		if (objsNum > 0)
		{
			for (int i = 0; i < objsNum; i++)
			{
				Rigidbody rbObj = linkedObjects[i].GetComponent<Rigidbody>();
				if (rbObj != null)
				{
					if (Vector3.Distance(transform.position, rbObj.position) <= attractionRange)
					{
						Vector3 toJoint = (transform.position - rbObj.position).normalized;
						rbObj.AddForce(toJoint * attractionForce);
					}
					else
					{
						Entity freeEntity = linkedObjects[i].GetComponent<Entity>();
						freeEntity.SetTarget(FindObjectOfType<PlayerMovement>().transform);
					}
				}
			}
		}
	}

	void CheckSurroundings()
	{
		Entity[] nearbyObjects = FindObjectsOfType<Entity>();
		int objsNum = nearbyObjects.Length;
		for (int i = 0; i < objsNum; i++)
		{
			Entity other = nearbyObjects[i];
			if ((other != null) && other.transform.CompareTag("Body"))
			{
				if (Vector3.Distance(transform.position, other.transform.position) <= attractionRange)
				{
					if (!linkedObjects.Contains(other.transform))
					{
						linkedObjects.Add(other.transform);
					}
				}
			}
		}
	}
}
