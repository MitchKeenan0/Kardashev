using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeopleConnection : MonoBehaviour
{
	public Transform connectionSegment;
	public float connectionRange = 0.65f;

	private GameSystem game;
	private List<GameObject> connectedPeople;


	void Start()
    {
		connectedPeople = new List<GameObject>();
		game = FindObjectOfType<GameSystem>();
    }

    
	public void AddObject(GameObject value)
	{
		if (!connectedPeople.Contains(value))
		{
			connectedPeople.Add(value);
		}
	}


	public void TestFromPoint(Vector3 origin)
	{
		Collider[] rawNeighbors = Physics.OverlapSphere(origin, connectionRange);

		int numHits = rawNeighbors.Length;
		if (numHits > 0)
		{
			for (int i = 0; i < numHits; i++)
			{

				HexPanel hex = rawNeighbors[i].gameObject.GetComponent<HexPanel>();
				if (hex != null)
				{
					if (hex.IsPopulated() && !hex.bConnected)
					{
						ConnectHex(hex);
					}
				}
			}
		}
	}


	void ConnectHex(HexPanel hex)
	{
		hex.bConnected = true;
		hex.GetComponent<SpriteRenderer>().material = hex.connectedMaterial;
		TestFromPoint(hex.gameObject.transform.position);
	}
}
