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
				GenerateHexField();
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


	void GenerateHexField()
	{
		spawnTimer += Time.deltaTime;

		// Timed ring generation
		if (spawnTimer >= spawnRate)
		{
			// Each ring's components
			int ringSize = ringIndex * 3;
			if (ringIndex <= 1)
			{
				ringSize = 6;
			}
			for (int i = 0; i < ringSize; i++)
			{
				// Rotating like pods in a flower
				float index = (ringIndex) * 61.8f;
				Vector3 eulers = new Vector3(0.0f, 0.0f, index + ringIndex);
				if (b3D)
				{
					eulers.y = eulers.z;
				}

				spawnTransform.Rotate(eulers, Space.World);
				Vector3 spawnPosition = spawnTransform.right * (index * 0.03f);

				// Spawning
				Transform commonTile = Instantiate(tilePrefab, spawnPosition, Quaternion.identity);
				spawns += 1;

				// Individual tile characteristics
				//Vector3 growth = (Vector3.one * Random.Range(0.01f, 0.1f));
				//commonTile.transform.localScale += growth;
				///commonTile.GetComponent<Rigidbody>().mass += growth.magnitude;

				// Imbue with icon pattern
				SpriteRenderer iconSprite = commonTile.GetComponent<SpriteRenderer>();
				//Sprite spriteToSpawn = null;
				Color tileColor = Color.white;
				if (iconSprite != null)
				{
					int randomInt = Mathf.FloorToInt(Random.Range(0.0f, 1.0f) * 4.0f);
					switch (randomInt)
					{
						case 0:
							tileColor = Color.green;
							break;
						case 1:
							tileColor = Color.blue;
							break;
						case 2:
							tileColor = Color.red;
							break;
						case 3:
							tileColor = Color.magenta;
							break;
						default:
							break;
					}

					iconSprite.color = tileColor;
					//iconSprite.sprite = spriteToSpawn;
					//iconSprite.transform.localScale = Vector3.one * 0.3f;
				}

				//float rando = Random.Range(0.5f, 0.7f);
				//Color background = new Color(
				//	0f,
				//	rando,
				//	0f
				//);

				//commonTile.GetComponent<SpriteRenderer>().color = background;
			}

			ringIndex += 1;

			spawnTimer = 0.0f;
		}
	}


}
