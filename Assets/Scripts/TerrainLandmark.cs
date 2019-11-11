using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainLandmark : MonoBehaviour
{
	public float minRange = 15000f;
	public float maxRange = 50000f;
	public float minElevation = 20000f;
	public float maxElevation = 200000f;
	public float minFalloff = 0.1f;
	public float maxFalloff = 0.9f;
	public float noiseScale = 1f;

	public float range = 15000;
	public float elevation = 20000f;
	public float falloff = 0.1f;

	void Awake()
	{
		range = Random.Range(minRange, maxRange);
		elevation = Random.Range(minElevation, maxElevation);
		falloff = Random.Range(minFalloff, maxFalloff);
	}
}
