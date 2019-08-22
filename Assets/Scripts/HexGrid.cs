using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
	public Transform centreTilePrefab;
	public Transform tilePrefab;
	public Sprite icon1;
	public Sprite icon2;
	public Sprite icon3;
	public Sprite icon4;
	public Sprite icon5;

	public Transform personPrefab;
	public Transform characterPrefab;
	public Transform spawnTransform;
	public float spawnRate = 0.1f;

	public bool bGrid = true;
	public bool b3D = true;
	public bool bRandomSize = true;
	public int radialDepth = 10;

	public int x = 5;
	public int y = 5;

	public float fieldSize = 3.0f;
	public float radius = 0.5f;
	public float populace = 0.8f;
	public float danger = 0.05f;

	public float positionX = 0.0f;
	public float positionY = 0.0f;

	public bool useAsInnerCircleRadius = true;

	private float spawnTimer = 0.0f;
	//private float offsetX, offsetY;
	private int spawns = 0;
	private int ringIndex = 1;
	private bool bInitialized = false;

	private HexPanel centrePanel;




	void Start()
	{
		if (!bGrid)
		{
			// Centre tile
			Transform newCentreTile = Instantiate(centreTilePrefab, Vector3.zero, Quaternion.identity);
			centrePanel = newCentreTile.GetComponent<HexPanel>();
			if (centrePanel != null)
			{
				//centrePanel.Freeze();
			}
		}

		Vector3 eulers = new Vector3(0.0f, 0.0f, Random.Range(-180.0f, 180.0f));
		spawnTransform.Rotate(eulers, Space.World);

		// Random field size
		if (bRandomSize)
		{
			radialDepth = Mathf.FloorToInt(Random.Range(3.0f, 6.0f) * fieldSize);
		}
	}

	void Update()
	{
		if (ringIndex < (radialDepth + 1))
		{
			if (Time.time > 0.05f)
			{
				GenerateHexField(0);
			}
		}
		else if (!bInitialized)
		{
			bInitialized = true;

			GameSystem game = FindObjectOfType<GameSystem>();
			if (game != null)
			{
				game.InitGame();
			}
		}
	}


	public void SpawnNewTile(Vector3 spawnPosition, bool bLiveStart)
	{
		// Spawning
		Transform commonTile = Instantiate(tilePrefab, spawnPosition, Quaternion.identity);
		spawns += 1;

		HexPanel tileHex = commonTile.GetComponent<HexPanel>();

		// Imbue with icon pattern
		SpriteRenderer iconSprite = commonTile.GetComponentInChildren<SpriteRenderer>();
		if (iconSprite != null)
		{
			Sprite spriteToSpawn = null;
			if (iconSprite != null)
			{
				int randomInt = Mathf.FloorToInt(Random.Range(0.0f, 1.0f) * 5.0f);
				switch (randomInt)
				{
					case 0:
						spriteToSpawn = icon1;
						tileHex.tagID = 1;
						break;
					case 1:
						spriteToSpawn = icon2;
						tileHex.tagID = 2;
						break;
					case 2:
						spriteToSpawn = icon3;
						tileHex.tagID = 3;
						break;
					case 3:
						spriteToSpawn = icon4;
						tileHex.tagID = 4;
						break;
					case 4:
						spriteToSpawn = icon5;
						tileHex.tagID = 5;
						break;
					default:
						break;
				}
				
				iconSprite.sprite = spriteToSpawn;
			}
		}

		tileHex.SetPhysical(true);
	}


	void GenerateHexField(int requestedNum)
	{
		spawnTimer += Time.deltaTime;

		// Timed ring generation
		if ((spawnTimer >= spawnRate) || (requestedNum > 0))
		{
			// Each ring's components
			int ringSize = (ringIndex * 3) + requestedNum;
			if (ringIndex < 1)
			{
				ringSize = 6;
			}
			for (int i = 0; i < ringSize; i++)
			{
				Vector3 spawnPosition = Random.insideUnitCircle * ringIndex * 6;
				//spawnTransform.right * (index * 0.03f);

				SpawnNewTile(spawnPosition, false);
			}

			ringIndex += 1;
			spawnTimer = 0.0f;
		}
	}


}
