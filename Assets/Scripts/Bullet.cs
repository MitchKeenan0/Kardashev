using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	public float bulletSpeed = 100f;
	public float gravity = 9f;
	public float lifeTimeMax = 3f;
	public float damage = 1000.0f;
	public float damageDuration = 0.5f;
	public float radiusOfEffect = 10.0f;
	public float lifeTime;
	public Transform impactParticles;
	public Transform onboardParticles;

	private Vector3 flightVector;
	private Vector3 lastPosition;
	private Vector3 deltaVector;
	private float onBoardBulletSpeed;
	
	private Transform owningGun;
	private Transform owningShooter;

	public Vector3 GetDeltaVector()
	{
		return (transform.position - lastPosition) * 1.5f;
	}


	// This is used to initialize the bullet by guns
	public virtual void AddSpeedModifier(float value, Transform gun, Transform shooter)
	{
		onBoardBulletSpeed = bulletSpeed * value;
		flightVector = (Vector3.forward * onBoardBulletSpeed);
		owningGun = gun;
		owningShooter = shooter;
		lastPosition = transform.position;
	}

	public float GetLifetime()
	{
		return lifeTime;
	}


	public virtual void Start()
	{
		onBoardBulletSpeed = bulletSpeed;
		flightVector = Vector3.forward * onBoardBulletSpeed;
		RaycastBulletPath();
	}


	public virtual void Update()
	{
		if (onBoardBulletSpeed != 0f)
		{
			RaycastBulletPath();
			UpdateFlight();
		}
	}


	void UpdateFlight()
	{
		// Flight duration
		lifeTime += Time.deltaTime;
		if ((lifeTimeMax > 0f) && (lifeTime >= lifeTimeMax))
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
		deltaVector = (transform.position - lastPosition) * 1.5f;
		if (Physics.Raycast(transform.position, deltaVector, out hit, deltaVector.magnitude))
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
