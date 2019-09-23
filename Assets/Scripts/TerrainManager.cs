using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainManager : MonoBehaviour
{
	public Transform bareTerrain;
	public Vector3 terrainPosition;
	public float startHeight = 0.5f;
	public float roughness = 0.003f;
	public float groundPoints = 100f;
	public Transform debugMarkerPrefab;

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

	private Vector3 rayOrigin = Vector3.up * 9000f;
	private Vector3 rayBeam;
	private RaycastHit[] hits;
	private float effectStrength = 0f;
	private Terrain jobTerrain;
	private int jobIndex = 0;

	private List<TerrainJob> jobs;


	void Start()
    {
		jobs = new List<TerrainJob>();

		Terrain preExistingGround = FindObjectOfType<Terrain>();
		if (preExistingGround != null)
		{
			Destroy(preExistingGround.gameObject);
		}

		Transform newTerrain = Instantiate(bareTerrain, terrainPosition, Quaternion.identity);
		currentTerrain = newTerrain.GetComponent<Terrain>();
		currentTerrainData = currentTerrain.terrainData;

		xRes = currentTerrainData.heightmapWidth;
		yRes = currentTerrainData.heightmapHeight;

		heights = new float[currentTerrainData.alphamapWidth, currentTerrainData.alphamapHeight];
		currentTerrainData.SetHeights(0, 0, heights);

		SetTerrainHeight(startHeight);

		//InitGround();

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
			if (numJobs == 1)
			{
				TerrainJob thisJob = jobs[0];
				float jobLifetime = (Time.time - thisJob.timeAtCreation);
				if (jobLifetime >= thisJob.Duration)
				{
					jobs.Remove(thisJob);
				}
				else
				{
					thisJob.lifeTime = jobLifetime;
					ProcessJob(thisJob);
				}
			}

			// Round robin method handles jobs one-at-a-time
			else if (jobIndex <= (numJobs - 1))
			{
				TerrainJob thisJob = jobs[jobIndex];
				float jobLifetime = (Time.time - thisJob.timeAtCreation);
				if (jobLifetime >= thisJob.Duration)
				{
					jobs.Remove(thisJob);
				}
				else
				{
					thisJob.lifeTime = jobLifetime;
					ProcessJob(thisJob);
				}

				jobIndex++;
				if (jobIndex >= numJobs)
				{
					jobIndex = 0;
				}
			}
		}
	}


	void ProcessJob(TerrainJob job)
	{
		rayBeam = job.Location - rayOrigin;
		hits = Physics.RaycastAll(rayOrigin, rayBeam);
		effectStrength = job.EffectIncrement * Time.deltaTime;

		int numHits = hits.Length;
		if (numHits > 0)
		{
			for (int i = 0; i < numHits; i++)
			{
				jobTerrain = hits[i].transform.GetComponent<Terrain>();
				if (jobTerrain != null)
				{
					RaiseTerrain(jobTerrain, hits[i].point, effectStrength, job.RadiusOfEffect);
				}
			}
		}
	}


	void InitGround()
	{
		if (currentTerrain != null)
		{
			Vector3 birdsEye = Vector3.up * 9999f;

			for (int i = 0; i < groundPoints; i++)
			{
				float devX = Random.Range(0f, xRes);
				float devZ = Random.Range(0f, xRes);
				Vector3 beamDown = new Vector3(devX, -9999f, devZ);

				RaycastHit[] landHits = Physics.RaycastAll(birdsEye, beamDown);
				int numHits = landHits.Length;
				if (numHits > 0)
				{
					for (int j = 0; j < numHits; j++)
					{
						if (landHits[j].transform.GetComponent<Terrain>())
						{
							Terrain thisTerrain = landHits[j].transform.GetComponent<Terrain>();
							float effectAmount = Random.Range(-0.5f, 0.5f);
							float radiusAmount = Random.Range(1f, 10f);

							RaiseTerrain(thisTerrain, landHits[j].point, effectAmount, radiusAmount);
							//AddJob(landHits[j].point, effectAmount, radiusAmount, 10f);

							// Debugging
							if (debugMarkerPrefab != null)
							{
								Transform debugMarker = Instantiate(debugMarkerPrefab, landHits[j].point, Quaternion.identity);
							}
						}
					}
				}
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
		newJob.timeAtCreation = Time.time;
		jobs.Add(newJob);
	}


	public void RaiseTerrain(Terrain terrain, Vector3 location, float effectIncrement, float radiusOfEffect)
	{
		int radiusInt = Mathf.FloorToInt(radiusOfEffect);
		int offset = radiusInt / 2;
		Vector3 tempCoord = (location - terrain.GetPosition());
		Vector3 coord = new Vector3
			(
			(tempCoord.x / GetTerrainSize(terrain).x),
			(tempCoord.y / GetTerrainSize(terrain).y),
			(tempCoord.z / GetTerrainSize(terrain).z)
			);

		targetTerrainData = terrain.terrainData;
		terrainHeightMapHeight = targetTerrainData.heightmapHeight;
		terrainHeightMapWidth = targetTerrainData.heightmapWidth;

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
				heights[xx, yy] += effectIncrement;
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
