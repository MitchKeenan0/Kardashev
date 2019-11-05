using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
	public Transform[] commonStructures;
	public Transform[] rareStructures;
	public Transform[] characters;
	public float rarityScale = 100f;
	public float enemyGracePeriod = 30f;
	public float spawnRange = 5000f;
	public float minimumRange = 1000f;
	public float spawnInterval = 3f;
	public GameObject testCollider;

	private PlayerBody player;
	private List<Transform> spawnedObjects;
	private IEnumerator spawnCoroutine;
	private IEnumerator despawnCoroutine;
	private IEnumerator enemySpawnGraceCoroutine;

	public void SetPlayer(Transform value)
	{
		player = value.GetComponent<PlayerBody>();

		enemySpawnGraceCoroutine = BeginEnemySpawning();
		StartCoroutine(enemySpawnGraceCoroutine);
	}

    void Start()
    {
		player = FindObjectOfType<PlayerBody>();
		spawnedObjects = new List<Transform>();
	}

	IEnumerator BeginEnemySpawning()
	{
		yield return new WaitForSeconds(enemyGracePeriod);

		spawnCoroutine = TimedEnemySpawn(spawnInterval);
		StartCoroutine(spawnCoroutine);
	}

	IEnumerator TimedEnemySpawn(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);

		if (player != null)
		{
			SpawnEnemy(player.transform.position);
		}
	}

	void SpawnEnemy(Vector3 location)
	{
		// Character
		int numCharacters = characters.Length;
		if (numCharacters > 0)
		{
			int rando = Mathf.FloorToInt(Random.Range(0f, numCharacters));
			if (characters[rando] != null)
			{
				RaycastHit hit;
				Vector3 spawnTarget = location + (Random.onUnitSphere * spawnRange);
				Vector3 birdsEye = spawnTarget + (Vector3.up * 5000f);
				if (Physics.Raycast(birdsEye, Vector3.down * 15000f, out hit, 20000f)) /// this raycast misses a lot!
				{
					Transform newCharacter = Instantiate(characters[rando], hit.point, Quaternion.identity);
					if (newCharacter.GetComponent<BodyCharacter>())
					{
						transform.position += newCharacter.GetComponent<BodyCharacter>().spawnOffset;
					}
					spawnedObjects.Add(newCharacter);
				}
			}
		}
		
		// Refresh timer
		spawnCoroutine = TimedEnemySpawn(spawnInterval);
		StartCoroutine(spawnCoroutine);
	}

	public void SpawnObjectNearby(Vector3 location, float randomizePosition, bool fadeIn)
	{
		SpawnStructure(location, randomizePosition, fadeIn);
	}

	void SpawnStructure(Vector3 location, float randomizePosition, bool fadeIn)
	{
		Transform spawnPrefab = commonStructures[Mathf.FloorToInt(
			Random.Range(0f, commonStructures.Length))];
		if (Random.Range(0f, rarityScale) > (rarityScale - 10))
		{
			spawnPrefab = rareStructures[Mathf.FloorToInt(Random.Range(0f, rareStructures.Length))];
		}

		Vector3 spawnTarget = location + (Random.onUnitSphere * spawnRange);
		if (randomizePosition > 0f)
		{
			spawnTarget += Random.insideUnitSphere * randomizePosition;
		}

		RaycastHit hit;
		Vector3 birdsEye = spawnTarget + (Vector3.up * 10000f);
		if (Physics.Raycast(birdsEye, Vector3.down * 20000f, out hit, 20000f))
		{
			// Check for "level" surface
			if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) >= 0.7f)
			{
				if (spawnPrefab != null)
				{
					testCollider.transform.position = hit.point;
					testCollider.transform.localScale = spawnPrefab.localScale;

					Transform newStructure = Instantiate(spawnPrefab, hit.point, Quaternion.identity);
					if (fadeIn)
					{
						newStructure.gameObject.AddComponent<FadeObject>();
						newStructure.GetComponent<FadeObject>().StartFadeIn();
					}
					spawnedObjects.Add(newStructure);

					// Out-of-sight check for live spawning, to do
					//RaycastHit visionHit;
					//if (!Physics.Raycast(location, hit.point, out visionHit))
					//{
					//	Transform newStructure = Instantiate(spawnPrefab, hit.point, Quaternion.identity);
					//	if (fadeIn)
					//	{
					//		newStructure.gameObject.AddComponent<FadeObject>();
					//		newStructure.GetComponent<FadeObject>().StartFadeIn();
					//	}
					//	spawnedObjects.Add(newStructure);
					//}

					testCollider.transform.localScale = Vector3.one;
					testCollider.SetActive(false);
				}
			}
		}
	}

}
