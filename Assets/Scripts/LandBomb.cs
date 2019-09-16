using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandBomb : MonoBehaviour
{
	public float radius = 10f;
	public float effectPower = 50f;
	public float delay = 1f;
	public Transform delayParticles;
	public Transform detonateParticles;

	private Rigidbody rb;
	private TerrainManager terrainManager;
	private bool bFuseLit = false;


    void Start()
    {
		rb = GetComponent<Rigidbody>();
		terrainManager = FindObjectOfType<TerrainManager>();
    }


	IEnumerator SetDetonation()
	{
		Transform delayEffect = Instantiate(delayParticles, transform.position, Quaternion.identity);
		delayEffect.parent = transform;
		Destroy(delayEffect.gameObject, delay);

		yield return new  WaitForSeconds(delay);

		// annd kaboom
		Transform detonateEffect = Instantiate(detonateParticles, transform.position, Quaternion.identity);
		Destroy(detonateEffect.gameObject, 3f);

		terrainManager.AddJob(transform.position, effectPower, radius, 0.5f);
	}


	private void OnTriggerEnter(Collider other)
	{
		if (!bFuseLit)
		{
			StartCoroutine(SetDetonation());
			bFuseLit = true;
		}
	}


	private void OnCollisionEnter(Collision collision)
	{
		if (!bFuseLit)
		{
			StartCoroutine(SetDetonation());
			bFuseLit = true;
		}
	}
}
