using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeopleConnection : MonoBehaviour
{
	public Transform connectionEffect;
	public float connectionRange = 0.65f;
	public float connectionDelay = 0.1f;
	public bool bNotifyDelay = true;
	public bool bRemoveOnConnect = true;

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

	//public void UpdateConnection()
	//{
	//	// Connection
	//	FlushObjects();
	//	TestFromPoint(transform.position, connectionRange,  false);
	//}

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


	public void TestFromPoint(Vector3 origin, float range, int tagID, bool bExplode)
	{
		Collider[] rawNeighbors = Physics.OverlapSphere(origin, range);
		int numHits = rawNeighbors.Length;
		if (numHits > 0)
		{
			int i = 0;
			int consecutiveTagCounts = 0;
			List<HexPanel> tagBuddies = new List<HexPanel>();

			while (i < numHits)
			{
				HexPanel hex = rawNeighbors[i].gameObject.GetComponent<HexPanel>();

				if (hex != null)
				{
					if ((hex.tagID == tagID) && !hex.bConnected)
					{
						consecutiveTagCounts += 1;
						tagBuddies.Add(hex);
						//TestFromPoint(hex.transform.position, connectionRange, hex.tagID, false);
					}
				}

				i++;
			}

			// Connect successful clusters
			if (tagBuddies.Count >= 3)
			{
				foreach (HexPanel h in tagBuddies)
				{
					h.AddTouchCount(1);
				}
			}
		}
	}


	public void ConnectHex(HexPanel hex)
	{
		// Sprite material swap
		if (!hex.bConnected)
		{
			hex.bConnected = true;
			hex.ConnectHex();
			//hex.Freeze();
		}

		// Spread
		//Vector3 testPosition = hex.gameObject.transform.position;
		//if (bNotifyDelay)
		//{
		//	StartCoroutine(DelayedConnect(testPosition, connectionRange, hex.tagID, hex.bFirstTime));
		//}
		//else
		//{
		//	TestFromPoint(testPosition, connectionRange, hex.tagID, false);
		//}

		// First-timer.. Particles and Charge
		if (hex.bFirstTime && hex.bConnected)
		{
			Transform newEffect = Instantiate(connectionEffect, hex.transform.position, Quaternion.identity);
			AudioSource effectAudio = newEffect.GetComponent<AudioSource>();
			if (effectAudio != null)
			{
				effectAudio.pitch *= (Random.Range(1.5f, 1.55f));
				effectAudio.Play();
			}

			Destroy(newEffect.gameObject, 0.6f);

			toolbox.NewSingleChargeModifier(1);
		}

		hex.bFirstTime = false;

		if (bRemoveOnConnect)
		{
			Destroy(hex.gameObject, connectionDelay);
			HexGrid grid = FindObjectOfType<HexGrid>();
			if (grid != null)
			{
				Vector3 newTilePostition = Random.insideUnitCircle * 10.0f;
				if (newTilePostition.magnitude < 5.0f)
				{
					newTilePostition *= 2.0f;
				}

				grid.SpawnNewTile(newTilePostition, true);
			}
		}
		else
		{
			AddObject(hex.gameObject);
		}
		

		//WinCondition();
	}


	public IEnumerator DelayedConnect(Vector3 position, float range, int tagID, bool bExplode)
	{
		yield return new WaitForSeconds(connectionDelay);

		TestFromPoint(position, range, tagID, bExplode);
	}


	
}
