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
				if (hits[i].transform.CompareTag("Land"))
				{
					RaiseMesh(hits[i].point, effectStrength * Time.smoothDeltaTime, job.radius, job.RadiusFalloff);
				}
			}
		}
	}

	public void AddJob(Vector3 location, float effectIncrement, float radiusOfEffect, float duration, float falloff)
	{
		TerrainJob newJob = new TerrainJob(location, effectIncrement, radiusOfEffect, duration, falloff);
		newJob.timeAtCreation = Time.time;
		jobs.Add(newJob);
	}

	public void RaiseMesh(Vector3 location, float effectIncrement, float radius, float fallOff)
	{
		Collider[] cols = Physics.OverlapSphere(location, radius * 2f);
		if (cols.Length > 0){
			for (int i = 0; i < cols.Length; i++){
				if (cols[i].gameObject.GetComponent<GenerateMeshSimple>())
				{
					// Mesh movement
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
								if (distToHit <= radius)
								{
									// Movement of the ground
									Vector3 vertToHit = GetVertexWorldPosition(vertices[j], filter.transform) - location;
									vertToHit.y *= 0f;
									float proximityScalar = (radius - vertToHit.magnitude) * 0.001f * fallOff; /// 0.0006f looks good
									proximityScalar = Mathf.Clamp(proximityScalar, 0f, 1f);
									vertices[j] += Vector3.up * effectIncrement * proximityScalar;
								}
							}

							// Recalculate the mesh & collision
							mesh.vertices = vertices;
							filter.mesh = mesh;
							mesh.RecalculateBounds();

							MeshCollider meshCollider = cols[i].transform.GetComponent<MeshCollider>();
							if (meshCollider != null)
								meshCollider.sharedMesh = filter.mesh;
						}
					}
				}

				// "Bubbling" player, vehicle and others just over rising terrain
				if (effectIncrement > 0f)
				{
					if (cols[i].gameObject.GetComponent<Rigidbody>())
					{
						Rigidbody rigidB = cols[i].gameObject.GetComponent<Rigidbody>();
						rigidB.AddForce(Vector3.up * effectIncrement * Time.deltaTime * 10f);
					}
					else if (cols[i].gameObject.GetComponent<CharacterController>())
					{
						CharacterController controller = cols[i].gameObject.GetComponent<CharacterController>();
						controller.Move(Vector3.up * effectIncrement * Time.deltaTime * 10f);
					}
				}
			}
		}
	}

	public Vector3 GetVertexWorldPosition(Vector3 vertex, Transform owner)
	{
		return owner.localToWorldMatrix.MultiplyPoint3x4(vertex);
	}


}
