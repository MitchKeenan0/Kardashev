using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandscaperBullet : Bullet
{

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

		float thisHitDamage = Mathf.Pow(1f + lifeTime, 3) * 0.1f * damage;

		if (impactParticles != null)
		{
			// Spawning impact particles
			Transform hitParticles = Instantiate(impactParticles, transform.position, transform.rotation);
			Destroy(hitParticles.gameObject, 1.1f);

			// Detach lifetime particles
			if (onboardParticles != null)
			{
				onboardParticles.parent = null;
				Destroy(onboardParticles.gameObject, 1.0f);
			}

			// Terrain height
			Terrain hitTerrain = hitObj.GetComponent<Terrain>();
			if (hitTerrain != null)
			{
				TerrainManager terrMan = FindObjectOfType<TerrainManager>();
				if (terrMan != null)
				{
					terrMan.AddJob(hitPosition, thisHitDamage, radiusOfEffect, damageDuration);
					//terrMan.LowerTerrain(hitTerrain, hitPosition, thisHitDamage, radiusOfEffect);
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
