using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	public float bulletSpeed = 100f;
	public float gravity = 9f;
	public float lifeTimeMax = 3f;
	public float damage = 1000.0f;
	public float radiusOfEffect = 10.0f;
	public Transform impactParticles;
	public Transform onboardParticles;

	private Vector3 flightVector;
	private Vector3 lastPosition;
	private float lifeTime;
	private Transform owningGun;
	private Transform owningShooter;


	// This is used to initialize the bullet by guns
	public void AddSpeedModifier(float value, Transform gun, Transform shooter)
	{
		bulletSpeed *= value;
		flightVector = (Vector3.forward * bulletSpeed);
		owningGun = gun;
		owningShooter = shooter;
	}


	void Start()
	{
		flightVector = Vector3.forward * bulletSpeed;
		RaycastBulletPath();
	}


	void Update()
	{
		RaycastBulletPath();
		UpdateFlight();
	}


	void UpdateFlight()
	{
		// Flight duration
		lifeTime += Time.deltaTime;
		if (lifeTime >= lifeTimeMax)
		{
			Destroy(gameObject);
		}

		// Vectoring
		lastPosition = transform.position;
		flightVector += (Vector3.up * -gravity);
		transform.Translate(flightVector * Time.deltaTime);
	}


	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject != gameObject)
		{
			LandHit(other.gameObject, other.ClosestPoint(transform.position));
		}
	}


	void RaycastBulletPath()
	{
		RaycastHit hit;
		Vector3 deltaV = (transform.position - lastPosition) * 1.5f;
		if (Physics.Raycast(transform.position, deltaV, out hit, deltaV.magnitude))
		{
			if (hit.transform.gameObject != gameObject)
			{
				if (!hit.collider.isTrigger)
				{
					if ((hit.transform.gameObject != owningGun.gameObject)
						&& (hit.transform.gameObject != owningShooter.transform.gameObject))
					{
						LandHit(hit.transform.gameObject, hit.point);
					}
				}
			}
		}
	}


	void LandHit(GameObject hitObj, Vector3 hitPosition)
	{
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

			Terrain hitTerrain = hitObj.GetComponent<Terrain>();
			if (hitTerrain != null)
			{
				TerrainManager terrMan = FindObjectOfType<TerrainManager>();
				if (terrMan != null)
				{
					terrMan.RaiseTerrain(hitTerrain, hitPosition, damage, radiusOfEffect);
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
								entityRB.AddForce(1000.0f * transform.forward);
							}
						}
					}
				}
			}

			//Debug.Log("Hit " + hitObj.name);

			// Work is done
			Destroy(gameObject);
		}
	}


}
