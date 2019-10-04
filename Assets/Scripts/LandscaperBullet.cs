using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandscaperBullet : Bullet
{
	public float falloff = 5f;

	public override void Start()
	{
		base.Start();
	}


	public override void Update()
	{
		base.Update();

	}


	public override void LandHit(GameObject hitObj, Vector3 hitPosition)
	{
		base.LandHit(hitObj, hitPosition);

		if (impactParticles != null)
		{
			// Spawning impact particles
			Transform hitParticles = Instantiate(impactParticles, hitPosition, transform.rotation);
			Destroy(hitParticles.gameObject, 1.1f);

			// Detach lifetime particles
			if (onboardParticles != null)
			{
				onboardParticles.parent = null;
				Destroy(onboardParticles.gameObject, 1.0f);
			}

			// Damage
			float thisHitDamage = damage;

			// Terrain height
			Terrain hitTerrain = hitObj.GetComponent<Terrain>();
			if (hitTerrain != null)
			{
				TerrainManager terrMan = FindObjectOfType<TerrainManager>();
				if (terrMan != null)
				{
					terrMan.AddJob(hitPosition, thisHitDamage, radiusOfEffect, damageDuration, falloff);
				}
			}

			// Damage-_
			if (radiusOfEffect > 0f)
			{
				Collider[] cols = Physics.OverlapSphere(transform.position, radiusOfEffect);
				if (cols.Length > 0)
				{
					for (int i = 0; i < cols.Length; i++)
					{
						Entity hitEntity = cols[i].transform.gameObject.GetComponent<Entity>();
						if (hitEntity != null)
						{
							Rigidbody entityRB = cols[i].transform.gameObject.GetComponent<Rigidbody>();
							if (entityRB != null)
							{
								entityRB.AddForce(1000.0f * thisHitDamage * transform.forward);
							}
						}
					}
				}
			}

			Destroy(gameObject);
		}
	}
}
