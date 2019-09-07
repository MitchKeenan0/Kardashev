using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
	public Transform bareTerrain;

	private Terrain currentTerrain;
	private TerrainData currentTerrainData;

	private float terrainSizeWidth;
	private float terrainSizeHeight;

	private TerrainData targetTerrainData;
	private int terrainHeightMapWidth;
	private int terrainHeightMapHeight;
	private float[,] height;

	void Start()
    {
		Terrain preExistingGround = FindObjectOfType<Terrain>();
		if (preExistingGround != null)
		{
			Destroy(preExistingGround.gameObject);
		}

		Transform newTerrain = Instantiate(bareTerrain, Vector3.zero, Quaternion.identity);
		currentTerrain = newTerrain.GetComponent<Terrain>();
		currentTerrainData = currentTerrain.terrainData;

		height = new float[currentTerrainData.alphamapWidth, currentTerrainData.alphamapHeight];
		currentTerrainData.SetHeights(0, 0, height);

		//terrainSizeWidth = currentTerrainData.heightmapWidth;
		//terrainSizeHeight = currentTerrainData.heightmapHeight;
		//newTerrain.transform.position = new Vector3(terrainSizeWidth / 2, terrainSizeHeight / 2, terrainSizeWidth / 2);
    }


	public void RaiseTerrain(Terrain terrain, Vector3 location, float effectIncrement, float radiusOfEffect)
	{
		targetTerrainData = terrain.terrainData;
		terrainHeightMapHeight = terrain.terrainData.heightmapHeight;
		terrainHeightMapWidth = terrain.terrainData.heightmapWidth;

		int radiusInt = Mathf.FloorToInt(radiusOfEffect);
		int offset = radiusInt / 2;

		Vector3 tempCoord = (location - terrain.GetPosition());
		Vector3 coord;

		coord = new Vector3
			(
			(tempCoord.x / GetTerrainSize(terrain).x),
			(tempCoord.y / GetTerrainSize(terrain).y),
			(tempCoord.z / GetTerrainSize(terrain).z)
			);

		Vector3 locationInTerrain = new Vector3(coord.x * terrainHeightMapWidth, 0, coord.z * terrainHeightMapHeight);

		int terX = (int)locationInTerrain.x - offset;

		int terZ = (int)locationInTerrain.z - offset;

		float[,] heights = targetTerrainData.GetHeights(terX, terZ, radiusInt, radiusInt);

		for (int xx = 0; xx < radiusInt; xx++)
		{
			for (int yy = 0; yy < radiusInt; yy++)
			{
				heights[xx, yy] += (effectIncrement * Time.smoothDeltaTime);
			}
		}

		targetTerrainData.SetHeights(terX, terZ, heights);
	}


	public void LowerTerrain(Terrain terrain, Vector3 location, float effectIncrement, float radiusOfEffect)
	{
		targetTerrainData = terrain.terrainData;
		terrainHeightMapHeight = terrain.terrainData.heightmapHeight;
		terrainHeightMapWidth = terrain.terrainData.heightmapWidth;

		int radiusInt = Mathf.FloorToInt(radiusOfEffect);
		int offset = radiusInt / 2;

		Vector3 tempCoord = (location - terrain.GetPosition());
		Vector3 coord;

		coord = new Vector3
			(
			(tempCoord.x / GetTerrainSize(terrain).x),
			(tempCoord.y / GetTerrainSize(terrain).y),
			(tempCoord.z / GetTerrainSize(terrain).z)
			);

		Vector3 locationInTerrain = new Vector3(coord.x * terrainHeightMapWidth, 0, coord.z * terrainHeightMapHeight);

		int terX = (int)locationInTerrain.x - offset;

		int terZ = (int)locationInTerrain.z - offset;

		float[,] heights = targetTerrainData.GetHeights(terX, terZ, radiusInt, radiusInt);

		for (int xx = 0; xx < radiusInt; xx++)
		{
			for (int yy = 0; yy < radiusInt; yy++)
			{
				heights[xx, yy] -= (effectIncrement * Time.smoothDeltaTime);
			}
		}

		targetTerrainData.SetHeights(terX, terZ, heights);
	}


	public Vector3 GetTerrainSize(Terrain t)
	{
		if (t)
		{
			return t.terrainData.size;
		}

		return Vector3.zero;
	}


}
