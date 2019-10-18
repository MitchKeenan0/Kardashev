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

	public Vector3 GetFlightVector()
	{
		return flightVector;
	}

	public void SetLifetime(float value)
	{
		lifeTime = value;
	}

	public Vector3 GetDeltaVector()
	{
		return (transform.position - lastPosition) * 1.5f;
	}

	public Transform GetOwningGun()
	{
		return owningGun;
	}

	public Transform GetOwningShooter()
	{
		return owningShooter;
	}


	// Also used to init bullet's VIP objects
	public virtual void AddSpeedModifier(float value, Transform gun, Transform shooter)
	{
		onBoardBulletSpeed = bulletSpeed * value;
		flightVector = (Vector3.forward * onBoardBulletSpeed);
		owningGun = gun;
		owningShooter = shooter;
	}

	public float GetLifetime()
	{
		return lifeTime;
	}


	public virtual void Start()
	{
		onBoardBulletSpeed = bulletSpeed;
		lastPosition = transform.position;
		flightVector = Vector3.forward * onBoardBulletSpeed;
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
		lifeTime += Time.smoothDeltaTime;
		if ((lifeTimeMax != 0f) && (lifeTime >= lifeTimeMax))
		{
			Destroy(gameObject, 0.1f);
		}

		// Vectoring
		lastPosition = transform.position;
		flightVector += (Vector3.up * -gravity * Time.smoothDeltaTime);

		transform.Translate(flightVector * Time.smoothDeltaTime);
	}


	public void RaycastBulletPath()
	{
		Vector3 origin = transform.position;
		deltaVector = (transform.position - lastPosition) * 5f;

		// Case for point-blank shots
		if (lifeTime < 0.15f)
		{
			origin += transform.forward * -(bulletSpeed * Time.smoothDeltaTime);
			deltaVector = transform.forward * (bulletSpeed * Time.smoothDeltaTime);
		}

		RaycastHit[] hits = Physics.RaycastAll(origin, deltaVector, deltaVector.magnitude);
		int numHits = hits.Length;
		if (numHits > 0)
		{
			for (int i = 0; i < numHits; i++)
			{
				RaycastHit hit = hits[i];
				if (!hit.collider.isTrigger)
				{
					Transform hitTransform = hit.transform;
					if ((hitTransform != owningGun) && (hitTransform != owningShooter))
					{
						LandHit(hit, hit.point);

						if (lifeTimeMax != 0f)
						{
							Destroy(gameObject, 0.1f);
						}

						//Debug.Log("Bullet hit " + hitTransform.name);
					}
				}
			}
		}
	}


	public virtual void LandHit(RaycastHit hit, Vector3 hitPosition)
	{
		
	}


}
