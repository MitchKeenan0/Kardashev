using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
	public Transform firePoint;
	public Transform bulletPrefab;
	public float bulletSpeedModifier = 1f;
	public float maxAmmo = 100f;
	public float aimSpeed = 10000f;

	private Vector3 targetVector;
	private Vector3 lerpAimVector;
	private Transform owningShooter;

	public void InitGun(Transform shooter)
	{
		owningShooter = shooter;
	}

	public void FireBullet()
	{
		Transform round = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
		Bullet newBullet = round.GetComponent<Bullet>();
		if (newBullet != null)
		{
			newBullet.AddSpeedModifier(bulletSpeedModifier, transform, owningShooter);
		}
	}


	void Start()
	{
		targetVector = lerpAimVector = transform.forward;
	}

	void Update()
	{
		UpdateAiming();
	}


	void UpdateAiming()
	{
		lerpAimVector = transform.position + (Camera.main.transform.forward * 100f);

		float dotToTarget = aimSpeed / Mathf.Abs(Vector3.Dot(transform.forward, lerpAimVector.normalized));

		targetVector = Vector3.Lerp(targetVector, lerpAimVector, Time.deltaTime * aimSpeed * dotToTarget);
		transform.LookAt(targetVector);
	}


	
}
