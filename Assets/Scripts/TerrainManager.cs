using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainManager : MonoBehaviour
{
	public Transform bareTerrain;
	public Vector3 terrainPosition;
	public float startHeight = 0.5f;
	public float roughness = 0.003f;
	public float roughnessDensity = 0.5f;
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

		//Transform newTerrain = Instantiate(bareTerrain, terrainPosition, Quaternion.identity);
		//currentTerrain = newTerrain.GetComponent<Terrain>();
		//currentTerrainData = currentTerrain.terrainData;

		//xRes = currentTerrainData.heightmapWidth;
		//yRes = currentTerrainData.heightmapHeight;

		//heights = new float[currentTerrainData.alphamapWidth, currentTerrainData.alphamapHeight];
		//currentTerrainData.SetHeights(0, 0, heights);

		//SetTerrainHeight(startHeight);
		//RandomizePoints(roughness);
		//InitGround();
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
		effectStrength = job.EffectIncrement;

		int numHits = hits.Length;
		if (numHits > 0)
		{
			for (int i = 0; i < numHits; i++)
			{
				jobTerrain = hits[i].transform.GetComponent<Terrain>();
				if (jobTerrain != null)
				{
					currentTerrain = jobTerrain;
					currentTerrainData = currentTerrain.terrainData;

					xRes = currentTerrainData.heightmapWidth;
					yRes = currentTerrainData.heightmapHeight;

					RaiseTerrain(jobTerrain, hits[i].point, effectStrength, job.radius);
					job.radius = Mathf.Lerp(job.radius, 0f, Time.smoothDeltaTime * job.RadiusFalloff);
				}
				else if (hits[i].transform.GetComponent<MeshFilter>())
				{
					// Spherecast for all affected meshes
					Collider[] nearCols = Physics.OverlapSphere(hits[i].point, job.radius);
					foreach (Collider col in nearCols)
					{
						// Only affect terrain tiles
						if (col.transform.GetComponent<GenerateMeshSimple>())
						{
							MeshFilter mf = col.transform.GetComponent<MeshFilter>();
							Mesh mesh = new Mesh();
							mesh = mf.mesh;
							PaintRaise(mesh, hits[i].point, job.radius, effectStrength);
						}
					}

					//job.radius = Mathf.Lerp(job.radius, 0f, Time.smoothDeltaTime * job.RadiusFalloff);
				}
			}
		}
	}

	// For Mesh, when no Terrain
	public void PaintRaise(Mesh mesh, Vector3 center, float radius, float power)
	{
		Vector3 localPoint = transform.InverseTransformPoint(center);
		List<Vector3> verts = new List<Vector3>();
		mesh.GetVertices(verts);

		for (int i = 0; i < verts.Count; ++i)
		{
			var heading = verts[i] - center;
			var distance = heading.magnitude;
			var direction = heading / distance;
			if (heading.sqrMagnitude < radius * radius)
			{
				verts[i] = new Vector3(
					verts[i].x,
					verts[i].y + (power / distance),
					verts[i].z);
			}
		}

		mesh.SetVertices(verts);
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
							float effectAmount = Random.Range(-0.1f, 0.1f);
							float radiusAmount = Random.Range(5f, 50f);
							float radiusFalloff = Random.Range(3f, 15f);
							float duration = Random.Range(1f, 10f);

							AddJob(landHits[j].point, effectAmount, radiusAmount, duration, radiusFalloff);

							// Debugging
							//if (debugMarkerPrefab != null)
							//{
							//	Transform debugMarker = Instantiate(debugMarkerPrefab, landHits[j].point, Quaternion.identity);
							//}
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
				if (Random.Range(0f, 1f) <= roughnessDensity)
				{
					heights[x, y] += Random.Range(0.0f, strength);
				}
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


	public void AddJob(Vector3 location, float effectIncrement, float radiusOfEffect, float duration, float falloff)
	{
		TerrainJob newJob = new TerrainJob(location, effectIncrement, radiusOfEffect, duration, falloff);
		newJob.timeAtCreation = Time.time;
		jobs.Add(newJob);
	}


	public void RaiseMesh(Vector3 location, float effectIncrement, float radius)
	{
		Collider[] cols = Physics.OverlapSphere(location, radius * 2f);
		if (cols.Length > 0)
		{
			for (int i = 0; i < cols.Length; i++)
			{
				// Mesh movement
				if (cols[i].gameObject.CompareTag("Land"))
				{
					MeshFilter filter = cols[i].gameObject.GetComponent<MeshFilter>();
					if (filter != null)
					{
						// Moving the verts
						Mesh mesh = filter.mesh;
						Vector3[] vertices = mesh.vertices;
						int numVerts = vertices.Length;
						if (numVerts > 0)
						{
							for (int j = 0; j < numVerts; j++)
							{
								float distToHit = Vector3.Distance(location, GetVertexWorldPosition(vertices[j], filter.transform));
								if (distToHit <= (radius))/// * cols[i].transform.localScale.magnitude))
								{
									// Movement of the ground
									Vector3 vertToHit = GetVertexWorldPosition(vertices[j], filter.transform) - location;
									vertToHit.y *= 0f;
									float proximityScalar = (radius * cols[i].transform.localScale.magnitude) - vertToHit.magnitude;
									proximityScalar = Mathf.Clamp(proximityScalar, 0f, 1f);
									vertices[j] += Vector3.up * effectIncrement * proximityScalar;
								}
							}

							// Recalculate the mesh & collision
							mesh.vertices = vertices;
							filter.mesh = mesh;
							mesh.RecalculateBounds();

							MeshCollider meshCollider = cols[i].transform.GetComponent<MeshCollider>();
							if (meshCollider)
								meshCollider.sharedMesh = filter.mesh;
						}
					}


					// "Bubbling" player, vehicle and others just over rising terrain
					if (effectIncrement > 0f)
					{
						CharacterController controller = cols[i].gameObject.GetComponent<CharacterController>();
						if (controller != null)
						{
							PlayerBody player = controller.gameObject.GetComponent<PlayerBody>();
							bool canMove = true;
							if ((player != null) && player.IsRiding())
								canMove = false;

							if (canMove)
							{
								controller.Move(Vector3.up * effectIncrement);
							}
						}
					}
				}
			}
		}
	}

	public Vector3 GetVertexWorldPosition(Vector3 vertex, Transform owner)
	{
		return owner.localToWorldMatrix.MultiplyPoint3x4(vertex);
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

		// Get Average for smoothing
		//float averageHeight = 0f;
		//for (int xx = 0; xx < radiusInt; xx++)
		//{
		//	for (int yy = 0; yy < radiusInt; yy++)
		//	{
		//		averageHeight += heights[xx, yy];
		//	}
		//}

		//averageHeight /= (radiusInt * radiusInt);
		//Debug.Log("average height: " + averageHeight);

		for (int xx = 0; xx < radiusInt; xx++)
		{
			for (int yy = 0; yy < radiusInt; yy++)
			{
				float height = heights[xx, yy];
				float newHeight = heights[xx, yy] + (effectIncrement * 0.01f);
				height = Mathf.Lerp(height, newHeight, Time.smoothDeltaTime);
				heights[xx, yy] = height; /// += (effectIncrement * Time.smoothDeltaTime);
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
