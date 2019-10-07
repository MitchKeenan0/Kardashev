using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandscaperBullet : Bullet
{
	public float falloff = 5f;
	public bool bImpactNormalize = true;
	public float impactNormalDamage = 1f;
	public float impactNormalRadius = 1f;

	public override void Start()
	{
		base.Start();
	}


	public override void Update()
	{
		base.Update();

	}


	public override void LandHit(RaycastHit hit, Vector3 hitPosition)
	{
		base.LandHit(hit, hitPosition);

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

			// Damage & Radius
			float thisHitDamage = damage;
			float thisHitRadius = radiusOfEffect;

			if (bImpactNormalize)
			{
				float normalizedDamage = Mathf.Abs(Mathf.Clamp(Vector3.Dot(transform.forward, hit.normal), -damage, damage));
				thisHitDamage = impactNormalDamage * damage * normalizedDamage;

				float normalizedRadius = Mathf.Abs(Mathf.Clamp(Vector3.Dot(transform.forward, hit.normal), -radiusOfEffect, radiusOfEffect));
				thisHitRadius *= impactNormalRadius * normalizedRadius;
				thisHitRadius = Mathf.Clamp(thisHitRadius, 1f, radiusOfEffect);

				///Debug.Log("damage: " + thisHitDamage + "   radius: " + thisHitRadius);
			}

			// Terrain height
			TerrainManager terrainManager = FindObjectOfType<TerrainManager>();
			if (terrainManager != null)
			{
				terrainManager.AddJob(hitPosition, thisHitDamage, thisHitRadius, damageDuration, falloff);
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
