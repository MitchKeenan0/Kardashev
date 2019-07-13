using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    public Transform tilePrefab;
	public Transform personPrefab;
	public Transform characterPrefab;
	public Transform spawnTransform;
	public float spawnRate = 0.1f;
	public float boostScale = 3.0f;

	public bool bGrid = true;
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
	private float offsetX, offsetY;
	private int spawns = 0;
	private int ringIndex = 1;

	private HexPanel centrePanel;

	


	void Start()
	{
		if (!bGrid)
		{
			// Centre tile
			Transform newCentreTile = Instantiate(tilePrefab, Vector3.zero, Quaternion.identity);
			centrePanel = newCentreTile.GetComponent<HexPanel>();
			if (centrePanel != null)
			{
				centrePanel.Freeze();
			}
		}

		if (bRandomSize)
		{
			radialDepth = Mathf.FloorToInt(Random.Range(3.0f, 9.0f));
		}
	}

	void Update()
	{
		if (ringIndex < (radialDepth + 1))
		{
			GenerateHexField();
		}
	}

	public void StartHexGrid()
	{
		float border = Mathf.Clamp(-fieldSize, -3.0f, 0.0f);

		Vector3 initialPosition = Camera.main.ScreenToWorldPoint(Vector3.zero);
		initialPosition.x += border + positionX;
		initialPosition.y += border + positionY;
		initialPosition.z = 0.0f;
		transform.position = initialPosition;

		Vector3 localPos = transform.position;
		localPos.z = 0.0f;

		float unitLength = useAsInnerCircleRadius ? (radius / (Mathf.Sqrt(3) / 2)) : radius;

		offsetX = unitLength * Mathf.Sqrt(3);
		offsetY = unitLength * 1.5f;

		if (bGrid)
		{
			for (int i = 0; i < x; i++)
			{
				for (int j = 0; j < y; j++)
				{
					Vector2 hexpos = HexOffset(i, j);


					// Decide if screen bounds allow this tile
					bool boundsWidth = (hexpos.x > border) || (hexpos.x < Screen.width + border);
					bool boundsHeight = (hexpos.y > border) || (hexpos.y < Screen.height + border);

					Vector3 pos = new Vector3(hexpos.x, hexpos.y, 0) + localPos;

					// Kern into circular shape
					if (Vector3.Distance(Vector3.zero, pos) <= fieldSize)
					{
						if (boundsWidth && boundsHeight)
						{
							Transform newTile = Instantiate(tilePrefab, pos, Quaternion.identity);

							// Populate with person === TO DO === remove this and init people consistently
							if ((Random.Range(0.0f, 1.0f) >= populace)
								&& (Vector3.Distance(newTile.position, Vector3.zero) >= 1.0f))
							{
								Transform newPerson = Instantiate(personPrefab, pos, Quaternion.identity);
								newPerson.parent = newTile;

								HexPanel hex = newTile.GetComponent<HexPanel>();
								hex.SetPopulated(true);
							}
						}
					}
				}
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
			int ringSize = ringIndex * 10;
			if (ringIndex <= 1)
			{
				ringSize = 6;
			}
			for (int i = 0; i < ringSize; i++)
			{
				float index = (ringIndex) * 61.8f;
				Vector3 eulers = new Vector3(0.0f, 0.0f, index);

				spawnTransform.Rotate(eulers, Space.World);
				Vector3 spawnPosition = spawnTransform.right * (index * 0.03f);


				Transform commonTile = Instantiate(tilePrefab, spawnPosition, Quaternion.identity);
				spawns += 1;


				if ((Random.Range(0.0f, 1.0f) >= populace) // Populate with person === TO DO === remove this and init people consistently
					&& (ringIndex > 2))
				{
					
					if ((Random.Range(0.0f, 1.0f) <= danger)) // Chance for dangerous character enemy
					{
						Transform newCharacter = Instantiate(characterPrefab, spawnPosition, Quaternion.identity);
						newCharacter.transform.SetParent(commonTile);

						//HexPanel hex = commonTile.GetComponent<HexPanel>();
						//hex.SetPopulated(true);

						// Increased force toward centre
						///hex.fallForce *= boostScale;
					}
					else // People
					{
						Transform newPerson = Instantiate(personPrefab, spawnPosition, Quaternion.identity);
						newPerson.transform.SetParent(commonTile);

						HexPanel hex = commonTile.GetComponent<HexPanel>();
						hex.SetPopulated(true);

						// Increased force toward centre
						hex.fallForce *= boostScale;
					}
				}

				// Individual tile characteristics
				Vector3 growth = (Vector3.one * Random.Range(0.01f, 0.1f));
				commonTile.transform.localScale += growth;
				commonTile.GetComponent<Rigidbody>().mass += growth.magnitude;

				float rando = Random.Range(0.3f, 0.5f);
				Color background = new Color(
					rando,
					rando,
					0f
				);
				commonTile.GetComponent<SpriteRenderer>().color = background;
			}

			ringIndex += 1;

			spawnTimer = 0.0f;
		}
	}

	Vector2 HexOffset(int x, int y)
	{
		Vector2 position = Vector2.zero;

		if (y % 2 == 0)
		{
			position.x = x * offsetX;
			position.y = y * offsetY;
		}
		else
		{
			position.x = (x + 0.5f) * offsetX;
			position.y = y * offsetY;
		}

		return position;
	}
}
