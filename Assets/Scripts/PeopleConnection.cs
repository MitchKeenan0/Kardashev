using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeopleConnection : MonoBehaviour
{
	public Transform connectionEffect;
	public float connectionRange = 0.65f;
	public float connectionDelay = 0.1f;
	public bool bNotifyDelay = true;

	private GameSystem game;
	private List<GameObject> connectedPeople;
	private GameObject[] people;
	private ToolBox toolbox;

	//private float testTimer = 0.0f;
	

	void Start()
    {
		connectedPeople = new List<GameObject>();
		game = FindObjectOfType<GameSystem>();
		toolbox = FindObjectOfType<ToolBox>();
	}

	public void SyncStart()
	{
		people = GameObject.FindGameObjectsWithTag("People");
		game = FindObjectOfType<GameSystem>();
	}

	public void DisconnectHex(HexPanel hex)
	{
		hex.bConnected = false;
		//hex.GetComponent<SpriteRenderer>().material = hex.normalMaterial;
		if (!hex.bFirstTime)
		{
			hex.GetComponent<SpriteRenderer>().color *= 0.9f;
		}
	}

	public void UpdateConnection()
	{
		// Connection
		FlushObjects();
		TestFromPoint(transform.position, connectionRange, false);
	}

	public void WinCondition()
	{
		// Win condition
		people = GameObject.FindGameObjectsWithTag("People");
		bool ready = (people != null);
		if (ready)
		{
			///Debug.Log(connectedPeople.Count + " people connected of " + people.Length);
			if (connectedPeople != null)
			{
				if ((people.Length > 0) && (connectedPeople.Count == people.Length))
				{
					game = FindObjectOfType<GameSystem>();
					if (game != null)
					{
						game.WinGame(true);
					}
				}
			}

			if ((people.Length == 0) && (Time.timeSinceLevelLoad > 2.0f)) ///(people.Length == 0)
			{
				game = FindObjectOfType<GameSystem>();
				if (game != null)
				{
					game.WinGame(false);
				}
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
			}
		}
	}


	public void TestFromPoint(Vector3 origin, float range, bool bExplode)
	{
		Collider[] rawNeighbors = Physics.OverlapSphere(origin, range);
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
						if (!hex.IsFrozen() && !hex.IsPopulated() && !hex.bEnemy)
						{
							//hex.LoseTouch();
						}
					}
				}

				i++;
			}
		}
	}


	void ConnectHex(HexPanel hex)
	{
		// Sprite material swap
		if (!hex.bConnected)
		{
			hex.bConnected = true;

			hex.Freeze();
		}

		// First-timer.. Particles and Charge
		if (hex.bFirstTime && hex.bConnected)
		{
			Transform newEffect = Instantiate(connectionEffect, hex.transform.position, Quaternion.identity);

			Destroy(newEffect.gameObject, 0.6f);

			toolbox.NewSingleChargeModifier(1);
		}

		// Spread
		Vector3 testPosition = hex.gameObject.transform.position;
		if (bNotifyDelay)
		{
			StartCoroutine(DelayNotify(testPosition, connectionRange, hex.bFirstTime));
		}
		else
		{
			TestFromPoint(testPosition, connectionRange, false);
		}

		hex.bFirstTime = false;

		AddObject(hex.gameObject);

		WinCondition();
	}


	IEnumerator DelayNotify(Vector3 position, float range, bool bExplode)
	{
		yield return new WaitForSeconds(connectionDelay);

		TestFromPoint(position, range, bExplode);
	}


	
}
