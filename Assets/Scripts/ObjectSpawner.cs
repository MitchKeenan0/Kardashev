using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
	public Transform[] structures;
	public Transform[] characters;
	public float spawnRange = 5000f;
	public float spawnInterval = 3f;

	private PlayerBody player;
	private List<Transform> spawnedObjects;
	private IEnumerator spawnCoroutine;
	private IEnumerator despawnCoroutine;

    void Start()
    {
		player = FindObjectOfType<PlayerBody>();
		spawnedObjects = new List<Transform>();
		spawnCoroutine = TimedSpawn(spawnInterval);
		StartCoroutine(spawnCoroutine);
	}

    
	IEnumerator TimedSpawn(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);

		SpawnObject();
	}


	void SpawnObject()
	{
		Vector3 lead = player.GetComponent<CharacterController>().velocity * 5f;
		Vector3 spawnTarget = player.transform.position + lead + (Random.onUnitSphere * spawnRange);
		spawnTarget.y = -1000f;

		RaycastHit hit;
		Vector3 birdsEye = player.transform.position + Vector3.up * 500f;
		if (Physics.Raycast(birdsEye, spawnTarget - birdsEye, out hit))
		{
			if (Random.Range(0f, 1f) > 0.5f)
			{
				// Structure
				int numStructures = structures.Length;
				int rando = Mathf.FloorToInt(Random.Range(0f, numStructures));

				if (structures[rando] != null)
				{
					Transform newStructure = Instantiate(structures[rando], hit.point, Quaternion.identity);
					newStructure.gameObject.AddComponent<FadeObject>();
					newStructure.GetComponent<FadeObject>().StartFading();
					spawnedObjects.Add(newStructure);
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

		// Refresh timer
		spawnCoroutine = TimedSpawn(spawnInterval);
		StartCoroutine(spawnCoroutine);
	}

	void ManageObjects()
	{

	}

}
