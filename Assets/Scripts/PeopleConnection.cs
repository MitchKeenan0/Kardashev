using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeopleConnection : MonoBehaviour
{
	public Transform connectionSegment;
	public float connectionRange = 0.65f;

	private GameSystem game;
	private List<GameObject> connectedPeople;

	private float testTimer = 0.0f;


	void Start()
    {
		connectedPeople = new List<GameObject>();
		game = FindObjectOfType<GameSystem>();
    }


	void Update()
	{
		testTimer += Time.deltaTime;
		if (testTimer >= 1.0f)
		{

			FlushObjects();
			TestFromPoint(transform.position);

			testTimer = 0.0f;
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
			}
		}

		connectedPeople.Clear();
	}


	public void TestFromPoint(Vector3 origin)
	{
		Collider[] rawNeighbors = Physics.OverlapSphere(origin, connectionRange);
		int numHits = rawNeighbors.Length;
		if (numHits > 0)
		{
			int i = 0;
			while (i < numHits)
			{
				HexPanel hex = rawNeighbors[i].gameObject.GetComponent<HexPanel>();

				if ((hex != null) && hex.IsPopulated() && !hex.bConnected)
				{
					ConnectHex(hex);
					AddObject(hex.gameObject);
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
		TestFromPoint(testPosition);
	}
}
