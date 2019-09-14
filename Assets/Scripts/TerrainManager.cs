using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainManager : MonoBehaviour
{
	public Transform bareTerrain;
	public Vector3 terrainPosition;
	public float startHeight = 0.5f;
	public float roughness = 0.003f;

	private Terrain currentTerrain;
	private TerrainData currentTerrainData;

	private float terrainSizeWidth;
	private float terrainSizeHeight;

	private int xRes;
	private int yRes;

	private TerrainData targetTerrainData;
	private int terrainHeightMapWidth;
	private int terrainHeightMapHeight;
	private float[,] heights;

	private List<TerrainJob> jobs;


	void Start()
    {
		jobs = new List<TerrainJob>();

		Terrain preExistingGround = FindObjectOfType<Terrain>();
		if (preExistingGround != null)
		{
			Destroy(preExistingGround.gameObject);
			Debug.Log("Cleared old terrain");
		}

		Transform newTerrain = Instantiate(bareTerrain, terrainPosition, Quaternion.identity);
		currentTerrain = newTerrain.GetComponent<Terrain>();
		currentTerrainData = currentTerrain.terrainData;

		xRes = currentTerrainData.heightmapWidth;
		yRes = currentTerrainData.heightmapHeight;

		heights = new float[currentTerrainData.alphamapWidth, currentTerrainData.alphamapHeight];
		currentTerrainData.SetHeights(0, 0, heights);

		SetTerrainHeight(startHeight);
		RandomizePoints(roughness);
	}


	void Update()
	{
		UpdateJobs();
	}


	void UpdateJobs()
	{
		int numJobs = jobs.Count;
		if (numJobs > 0)
		{
			for (int i = 0; i < numJobs; i++)
			{
				TerrainJob thisJob = jobs[i];

				float jobLifetime = thisJob.lifeTime;
				jobLifetime += Time.deltaTime;
				if (jobLifetime > thisJob.Duration)
				{
					jobs.Remove(thisJob);
					break;
				}
				else
				{
					thisJob.lifeTime = jobLifetime;
					ProcessJob(thisJob);
				}
			}
		}
	}


	void ProcessJob(TerrainJob job)
	{
		Vector3 rayOrigin = Vector3.up * 9000f;
		Vector3 rayBeam = job.Location - rayOrigin;
		RaycastHit[] hits = Physics.RaycastAll(rayOrigin, rayBeam);
		float effectStrength = job.EffectIncrement * Time.deltaTime;

		int numHits = hits.Length;
		if (numHits > 0)
		{
			for (int i = 0; i < numHits; i++)
			{
				Terrain terrain = hits[i].transform.GetComponent<Terrain>();
				if (terrain != null)
				{
					RaiseTerrain(terrain, hits[i].point, effectStrength, job.RadiusOfEffect);
				}

				break;
			}
		}
	}


	void RandomizePoints(float strength)
	{
		heights = currentTerrainData.GetHeights(0, 0, xRes, yRes);

		for (int y = 0; y < yRes; y++)
		{
			for (int x = 0; x < xRes; x++)
			{
				heights[x, y] += Random.Range(0.0f, strength);
			}
		}

		currentTerrainData.SetHeights(0, 0, heights);
	}


	void SetTerrainHeight(float value)
	{
		heights = currentTerrainData.GetHeights(0, 0, xRes, yRes);

		for (int y = 0; y < yRes; y++)
		{
			for (int x = 0; x < xRes; x++)
			{
				heights[x, y] = value;
			}
		}

		currentTerrainData.SetHeights(0, 0, heights);
	}


	public void AddJob(Vector3 location, float effectIncrement, float radiusOfEffect, float duration)
	{
		TerrainJob newJob = new TerrainJob(location, effectIncrement, radiusOfEffect, duration);
		jobs.Add(newJob);
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
