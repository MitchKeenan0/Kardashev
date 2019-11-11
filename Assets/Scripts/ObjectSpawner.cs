using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
	public Transform[] artifacts;
	public Transform[] commonStructures;
	public Transform[] rareStructures;
	public Transform[] characters;
	public float rarityScale = 100f;
	public float enemyGracePeriod = 30f;
	public float spawnRange = 5000f;
	public float minimumRange = 1000f;
	public float enemySpawnInterval = 3f;
	public float artifactSpawnInterval = 3f;
	public GameObject testCollider;

	private PlayerBody player;
	private List<Transform> spawnedObjects;
	private PlayerStartPosition playerStart;
	private IEnumerator spawnEnemyCoroutine;
	private IEnumerator spawnArtifactCoroutine;
	private IEnumerator despawnCoroutine;
	private IEnumerator enemySpawnGraceCoroutine;

	public void SetPlayer(Transform value)
	{
		player = value.GetComponent<PlayerBody>();
		playerStart = FindObjectOfType<PlayerStartPosition>();

		enemySpawnGraceCoroutine = BeginEnemySpawning();
		StartCoroutine(enemySpawnGraceCoroutine);

		BeginArtifactSpawning();
	}

    void Start()
    {
		playerStart = FindObjectOfType<PlayerStartPosition>();
		player = FindObjectOfType<PlayerBody>();
		spawnedObjects = new List<Transform>();
	}

	IEnumerator BeginEnemySpawning()
	{
		yield return new WaitForSeconds(enemyGracePeriod);

		spawnEnemyCoroutine = TimedEnemySpawn(enemySpawnInterval);
		StartCoroutine(spawnEnemyCoroutine);
	}

	void BeginArtifactSpawning()
	{
		spawnArtifactCoroutine = TimedArtifactSpawn(artifactSpawnInterval);
		StartCoroutine(spawnArtifactCoroutine);
	}

	IEnumerator TimedArtifactSpawn(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);

		if (player != null)
		{
			SpawnArtifact(player.transform.position);
		}
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
				Vector3 birdsEye = spawnTarget + (Vector3.up * 50000f);
				if (Physics.Raycast(birdsEye, Vector3.down * 150000f, out hit, 200000f)) /// this raycast misses a lot!
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
		spawnEnemyCoroutine = TimedEnemySpawn(enemySpawnInterval);
		StartCoroutine(spawnEnemyCoroutine);
	}

	void SpawnArtifact(Vector3 location)
	{
		Transform spawnPrefab = artifacts[Mathf.FloorToInt(Random.Range(0f, artifacts.Length))];
		Vector3 spawnLocation = location + Random.insideUnitSphere * 10000f;
		spawnLocation.y = 0f;
		spawnLocation += spawnPrefab.GetComponent<StructureHarvester>().spawnOffset * Random.Range(0.1f, 1f);
		spawnLocation += player.GetVelocity() * 10f;
		Transform arti = Instantiate(spawnPrefab, spawnLocation, Random.rotation);
		arti.GetComponent<StructureHarvester>().SetPhysical(true, Random.Range(0.1f, 0.01f));
		arti.GetComponent<FadeObject>().StartFadeIn();

		// Refresh timer
		spawnArtifactCoroutine = TimedArtifactSpawn(artifactSpawnInterval);
		StartCoroutine(spawnArtifactCoroutine);
	}

	public void SpawnObjectNearby(Vector3 location, float randomizePosition, bool fadeIn)
	{
		SpawnStructure(location, randomizePosition, fadeIn);
	}

	void SpawnStructure(Vector3 location, float randomizePosition, bool fadeIn)
	{
		Transform spawnPrefab = commonStructures[Mathf.FloorToInt(
			Random.Range(0f, commonStructures.Length))];
		if (Random.Range(0f, rarityScale) > (rarityScale - 3f))
		{
			spawnPrefab = rareStructures[Mathf.FloorToInt(Random.Range(0f, rareStructures.Length))];
		}

		Vector3 spawnTarget = location + (Random.onUnitSphere * spawnRange);
		Vector3 toPlayer = playerStart.transform.position - spawnTarget;
		toPlayer.y = 0f;
		if (toPlayer.magnitude < 100f)
		{
			spawnTarget += toPlayer * -Random.Range(1.1f, 2f);
		}

		if (randomizePosition > 0f)
		{
			spawnTarget += Random.insideUnitSphere * randomizePosition;
		}

		RaycastHit hit;
		Vector3 birdsEye = spawnTarget + (Vector3.up * 50000f);
		if (Physics.Raycast(birdsEye, Vector3.down * 100000f, out hit, 100000f))
		{
			// Check for "level" surface
			if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) >= 0.5f)
			{
				if (spawnPrefab != null)
				{
					Transform newStructure = Instantiate(spawnPrefab, hit.point, Quaternion.identity);

					if (fadeIn)
					{
						newStructure.gameObject.AddComponent<FadeObject>();
						newStructure.GetComponent<FadeObject>().StartFadeIn();
					}

					spawnedObjects.Add(newStructure);
				}
			}
		}
	}

}
