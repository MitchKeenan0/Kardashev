﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    public Transform tilePrefab;
	public Transform personPrefab;

	public int x = 5;
	public int y = 5;

	public float fieldSize = 3.0f;
	public float radius = 0.5f;
	public float populace = 0.8f;

	public float positionX = 0.0f;
	public float positionY = 0.0f;

	public bool useAsInnerCircleRadius = true;

	private float offsetX, offsetY;

	void Start()
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
						if (Random.Range(0.0f, 1.0f) >= populace)
						{
							Transform newPerson = Instantiate(personPrefab, pos, Quaternion.identity);
							newPerson.parent = newTile;
						}
					}
				}
			}
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
