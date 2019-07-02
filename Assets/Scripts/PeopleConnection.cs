using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeopleConnection : MonoBehaviour
{
	public Transform connectionEffect;
	public float connectionRange = 0.65f;

	private GameSystem game;
	private List<GameObject> connectedPeople;
	private GameObject[] people;

	private float testTimer = 0.0f;
	


	void Start()
    {
		connectedPeople = new List<GameObject>();
		game = FindObjectOfType<GameSystem>();
	}

	public void SyncStart()
	{
		people = GameObject.FindGameObjectsWithTag("People");
	}


	void Update()
	{
		testTimer += Time.deltaTime;
		if (testTimer >= 1.0f)
		{
			FlushObjects();

			TestFromPoint(transform.position, false);

			testTimer = 0.0f;

			// Win condition
			if ((people.Length > 0) && (connectedPeople.Count == people.Length))
			{
				Debug.Log("YOU WIN: " + connectedPeople.Count + " people connected of " + people.Length + ".");
			}
		}
	}


	public void AddObject(GameObject value)
	{
		if (!connectedPeople.Contains(value))
		{
			connectedPeople.Add(value);
		}
	}


	void FlushObjects()
	{
		int numPeople = connectedPeople.Count;
		for (int i = 0; i < numPeople; i++)
		{
			HexPanel hex = connectedPeople[i].GetComponent<HexPanel>();
			if (hex != null)
			{
				hex.bConnected = false;
				hex.GetComponent<SpriteRenderer>().material = hex.normalMaterial;
			}
		}

		connectedPeople.Clear();
	}


	public void TestFromPoint(Vector3 origin, bool bExplode)
	{
		Collider[] rawNeighbors = Physics.OverlapSphere(origin, connectionRange);
		int numHits = rawNeighbors.Length;
		if (numHits > 0)
		{
			int i = 0;
			while (i < numHits)
			{
				HexPanel hex = rawNeighbors[i].gameObject.GetComponent<HexPanel>();

				if (hex != null)
				{
					if (hex.IsPopulated() && !hex.bConnected)
					{
						ConnectHex(hex);
					}
					else if (bExplode && (hex.bFirstTime))
					{
						if (!hex.IsFrozen() && !hex.IsPopulated())
						{
							hex.LoseTouch();
						}
					}
				}

				i++;
			}
		}
	}


	void ConnectHex(HexPanel hex)
	{
		if (!hex.bConnected)
		{
			hex.bConnected = true;
			hex.GetComponent<SpriteRenderer>().material = hex.connectedMaterial;
		}

		Vector3 testPosition = hex.gameObject.transform.position;
		TestFromPoint(testPosition, false);

		if (hex.bFirstTime && hex.bConnected)
		{
			Transform newEffect = Instantiate(connectionEffect, hex.transform.position, Quaternion.identity);
			Destroy(newEffect.gameObject, 0.6f);
			hex.bFirstTime = false;
		}

		AddObject(hex.gameObject);
	}
}
