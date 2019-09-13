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
	public float lifeTime;
	public Transform impactParticles;
	public Transform onboardParticles;

	private Vector3 flightVector;
	private Vector3 lastPosition;
	
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

	public float GetLifetime()
	{
		return lifeTime;
	}


	public virtual void Start()
	{
		flightVector = Vector3.forward * bulletSpeed;
		RaycastBulletPath();
	}


	public virtual void Update()
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
			Destroy(gameObject, 0.1f);
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
						&& (hit.transform.gameObject != owningShooter.gameObject))
					{
						LandHit(hit.transform.gameObject, hit.point);
					}
				}
			}
		}
	}


	public virtual void LandHit(GameObject hitObj, Vector3 hitPosition)
	{
		
	}


}
