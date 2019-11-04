using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandscaperBullet : Bullet
{
	public float falloff = 5f;
	public bool bImpartVelocity = true;

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
			float hitnormalScalar = Mathf.Clamp(Mathf.Pow(1f / Mathf.Abs(Vector3.Dot(Vector3.up, hit.normal)), 2f), 1f, 2f);
			float thisHitDamage = damage * hitnormalScalar;
			float thisHitRadius = radiusOfEffect * hitnormalScalar;

			// Ground Manipulations
			if (hit.transform.CompareTag("Land"))
			{
				TerrainManager tMan = FindObjectOfType<TerrainManager>();
				if (tMan != null)
				{
					//tMan.RaiseMesh(hitPosition, thisHitDamage, thisHitRadius);
					tMan.AddJob(hitPosition, thisHitDamage, thisHitRadius, damageDuration, falloff);
				}
			}

			Destroy(gameObject);
		}
	}

	public Vector3 GetVertexWorldPosition(Vector3 vertex, Transform owner)
	{
		return owner.localToWorldMatrix.MultiplyPoint3x4(vertex);
	}
}
