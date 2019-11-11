﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralToolkit;

[RequireComponent(typeof(MeshFilter))]
public class GenerateMeshSimple : MonoBehaviour {

    private MeshFilter meshFilter;

	private static TerrainLandmark[] landmarks;

	public bool bGarnished = false;

    public Vector3 TerrainSize { get; set; }
    public float CellSize { get; set; }
    public float NoiseScale { get; set; }

    public Gradient Gradient { get; set; }

    public Vector2 NoiseOffset { get; set; }

    public void Generate() {
        meshFilter = GetComponent<MeshFilter>();
		landmarks = FindObjectsOfType<TerrainLandmark>();

		MeshDraft draft = TerrainDraft(TerrainSize, CellSize, NoiseOffset, NoiseScale, Gradient, transform);
        draft.Move(Vector3.left * TerrainSize.x / 2 + Vector3.back * TerrainSize.z / 2);
        meshFilter.mesh = draft.ToMesh();

        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider)
            meshCollider.sharedMesh = meshFilter.mesh;
    }

	static Vector3 GetVertexWorldPosition(Vector3 vertex, Transform owner)
	{
		return owner.localToWorldMatrix.MultiplyPoint3x4(vertex);
	}

	static float GetUniqueHeight(Vector3 location, Transform owner)
	{
		float result = 0f;
		if (landmarks.Length > 0)
		{
			foreach(TerrainLandmark lm in landmarks)
			{
				if (Vector3.Distance(lm.transform.position, GetVertexWorldPosition(location, owner)) <= lm.range)
				{
					Vector3 toVertex = (lm.transform.position - GetVertexWorldPosition(location, owner));
					toVertex.y = 0f;
					float vertexDistToLandmark = toVertex.magnitude;
					if (vertexDistToLandmark <= lm.range)
					{
						float proximityScalar = Mathf.Sqrt((lm.range - vertexDistToLandmark) * 0.0001f * lm.falloff);
						proximityScalar = Mathf.Clamp(proximityScalar, 0f, 1f);
						result += (lm.elevation * proximityScalar);
					}
				}
			}
		}

		return result;
	}

	private static MeshDraft TerrainDraft(Vector3 terrainSize, float cellSize, Vector2 noiseOffset, float noiseScale, Gradient gradient, Transform owner) {
        int xSegments = Mathf.FloorToInt(terrainSize.x / cellSize);
        int zSegments = Mathf.FloorToInt(terrainSize.z / cellSize);

        float xStep = terrainSize.x / xSegments;
        float zStep = terrainSize.z / zSegments;
        int vertexCount = 6 * xSegments * zSegments;
        MeshDraft draft = new MeshDraft {
            name = "Terrain",
            vertices = new List<Vector3>(vertexCount),
            triangles = new List<int>(vertexCount),
            normals = new List<Vector3>(vertexCount),
            colors = new List<Color>(vertexCount)
        };

        for (int i = 0; i < vertexCount; i++) {
            draft.vertices.Add(Vector3.zero);
            draft.triangles.Add(0);
            draft.normals.Add(Vector3.zero);
            draft.colors.Add(Color.black);
        }

        for (int x = 0; x < xSegments; x++) {
            for (int z = 0; z < zSegments; z++) {
                int index0 = 6 * (x + z * xSegments);
                int index1 = index0 + 1;
                int index2 = index0 + 2;
                int index3 = index0 + 3;
                int index4 = index0 + 4;
                int index5 = index0 + 5;

                float height00 = GetHeight(x + 0, z + 0, xSegments, zSegments, noiseOffset, noiseScale);
                float height01 = GetHeight(x + 0, z + 1, xSegments, zSegments, noiseOffset, noiseScale);
                float height10 = GetHeight(x + 1, z + 0, xSegments, zSegments, noiseOffset, noiseScale);
                float height11 = GetHeight(x + 1, z + 1, xSegments, zSegments, noiseOffset, noiseScale);

                Vector3 vertex00 = new Vector3((x + 0) * xStep, height00 * terrainSize.y, (z + 0) * zStep);
				Vector3 vertex01 = new Vector3((x + 0) * xStep, height01 * terrainSize.y, (z + 1) * zStep);			
				Vector3 vertex10 = new Vector3((x + 1) * xStep, height10 * terrainSize.y, (z + 0) * zStep);				
				Vector3 vertex11 = new Vector3((x + 1) * xStep, height11 * terrainSize.y, (z + 1) * zStep);

				vertex00.y += GetUniqueHeight(vertex00, owner);
				vertex01.y += GetUniqueHeight(vertex01, owner);
				vertex10.y += GetUniqueHeight(vertex10, owner);
				vertex11.y += GetUniqueHeight(vertex11, owner);

				draft.vertices[index0] = vertex00;
                draft.vertices[index1] = vertex01;
                draft.vertices[index2] = vertex11;
                draft.vertices[index3] = vertex00;
                draft.vertices[index4] = vertex11;
                draft.vertices[index5] = vertex10;

                draft.colors[index0] = gradient.Evaluate(height00);
                draft.colors[index1] = gradient.Evaluate(height01);
                draft.colors[index2] = gradient.Evaluate(height11);
                draft.colors[index3] = gradient.Evaluate(height00);
                draft.colors[index4] = gradient.Evaluate(height11);
                draft.colors[index5] = gradient.Evaluate(height10);

                Vector3 normal000111 = Vector3.Cross(vertex01 - vertex00, vertex11 - vertex00).normalized;
                Vector3 normal001011 = Vector3.Cross(vertex11 - vertex00, vertex10 - vertex00).normalized;

                draft.normals[index0] = normal000111;
                draft.normals[index1] = normal000111;
                draft.normals[index2] = normal000111;
                draft.normals[index3] = normal001011;
                draft.normals[index4] = normal001011;
                draft.normals[index5] = normal001011;

                draft.triangles[index0] = index0;
                draft.triangles[index1] = index1;
                draft.triangles[index2] = index2;
                draft.triangles[index3] = index3;
                draft.triangles[index4] = index4;
                draft.triangles[index5] = index5;
            }
        }

        return draft;
    }

    private static float GetHeight(int x, int z, int xSegments, int zSegments, Vector2 noiseOffset, float noiseScale) {
		float noiseX = noiseScale * x / xSegments + noiseOffset.x;
        float noiseZ = noiseScale * z / zSegments + noiseOffset.y;
		float basicNoise = Mathf.PerlinNoise(noiseX, noiseZ);
		float noise = basicNoise * Mathf.Abs(basicNoise) * 6f;
		return noise;
	}

}