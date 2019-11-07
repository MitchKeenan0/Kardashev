using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandBomb : MonoBehaviour
{
	public float radius = 10f;
	public float falloff = 5f;
	public float effectPower = 50f;
	public float effectDuration = 1f;
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
		TerrainManager tMan = FindObjectOfType<TerrainManager>();
		if (tMan != null)
		{
			tMan.AddJob(transform.position, effectPower, radius, effectDuration, falloff);
		}
		Destroy(gameObject, 0.1f);
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
