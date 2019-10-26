using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
	public Transform[] structures;
	public Transform[] characters;
	public float spawnRange = 5000f;
	public float minimumRange = 1000f;
	public float spawnInterval = 3f;
	public GameObject testCollider;

	private PlayerBody player;
	private List<Transform> spawnedObjects;
	private IEnumerator spawnCoroutine;
	private IEnumerator despawnCoroutine;

	public void SetPlayer(Transform value)
	{
		player = value.GetComponent<PlayerBody>();
		//spawnCoroutine = TimedSpawn(spawnInterval);
		//StartCoroutine(spawnCoroutine);
	}

    void Start()
    {
		player = FindObjectOfType<PlayerBody>();
		spawnedObjects = new List<Transform>();
	}

    
	IEnumerator TimedSpawn(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);

		if (player != null)
		{
			SpawnObject(player.transform.position, 0f);
		}
	}


	public void SpawnObjectNearby(Vector3 location, float randomizePosition)
	{
		SpawnObject(location, randomizePosition);
	}


	void SpawnObject(Vector3 location, float randomizePosition)
	{
		Vector3 spawnTarget = location + (Random.onUnitSphere * spawnRange);
		if (randomizePosition > 0f)
		{
			spawnTarget += Random.insideUnitSphere * randomizePosition;
		}

		RaycastHit hit;
		Vector3 birdsEye = spawnTarget + (Vector3.up * 5000f);
		if (Physics.Raycast(birdsEye, Vector3.down * 15000f, out hit, 20000f)) /// this raycast misses a lot!
		{
			// Check for "level" surface
			if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) >= 0.7f)
			{
				// Structure or Character
				if (Random.Range(0f, 1f) >= 0.05f)
				{
					int numStructures = structures.Length;
					int rando = Mathf.FloorToInt(Random.Range(0f, numStructures));
					if (structures[rando] != null)
					{
						testCollider.transform.position = hit.point;
						testCollider.transform.localScale = structures[rando].localScale;

						RaycastHit visionHit;
						if (!Physics.Raycast(location, hit.point, out visionHit))
						{
							Transform newStructure = Instantiate(structures[rando], hit.point, Quaternion.identity);
							newStructure.transform.position += Vector3.down * Random.Range(1f, 10f);
							newStructure.gameObject.AddComponent<FadeObject>();
							newStructure.GetComponent<FadeObject>().StartFadeIn();
							spawnedObjects.Add(newStructure);
						}

						testCollider.transform.localScale = Vector3.one;
						testCollider.SetActive(false);
					}
				}
				else
				{
					// Character
					int numCharacters = characters.Length;
					int rando = Mathf.FloorToInt(Random.Range(0f, numCharacters));

					if (characters[rando] != null)
					{
						Transform newCharacter = Instantiate(characters[rando], hit.point, Quaternion.identity);
						spawnedObjects.Add(newCharacter);
					}
				}
			}
		}

		// Refresh timer
		//spawnCoroutine = TimedSpawn(spawnInterval);
		//StartCoroutine(spawnCoroutine);
	}

	void ManageObjects()
	{

	}

}
