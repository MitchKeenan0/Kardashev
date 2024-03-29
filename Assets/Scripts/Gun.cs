﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Tool
{
	public Transform firePoint;
	public Transform bulletPrefab;
	public Transform alternateFirePrefab;
	public float bulletSpeedModifier = 1f;
	public float automaticRateOfFire = 20f;
	public float maxAmmo = 100f;
	public float aimSpeed = 10000f;

	private Vector3 targetVector;
	private Vector3 lerpAimVector;
	private Transform owningShooter;
	private AudioSource gunAudioSource;
	private bool bArmed = false;
	private bool bAlternateArmed = false;
	private float automaticFireTimer = 0f;
	private float autoFireTime;

	public override void SetToolActive(bool value)
	{
		base.SetToolActive(value);
		SetArmed(value);
	}

	public override void SetToolAlternateActive(bool value)
	{
		base.SetToolAlternateActive(value);
		bAlternateArmed = value;
	}

	public override void InitTool(Transform owner)
	{
		base.InitTool(owner);
		owningShooter = owner.gameObject.transform;
	}

	public void SetArmed(bool value)
	{
		bArmed = value;
	}

	void Start()
	{
		autoFireTime = (1f / automaticRateOfFire);
		targetVector = lerpAimVector = transform.forward;
		gunAudioSource = GetComponent<AudioSource>();
	}

	void Update()
	{
		UpdateAiming();
		automaticFireTimer += Time.deltaTime;
		if (bArmed || bAlternateArmed){
			if (automaticRateOfFire > 0f){
				if (automaticFireTimer >= autoFireTime){
					FireBullet();
					automaticFireTimer = 0.0f;
				}
			}
			else
			{
				FireBullet();
				SetArmed(false);
			}
		}
	}

	void UpdateAiming()
	{
		lerpAimVector = transform.position + (Camera.main.transform.forward * 100f);
		float dotToTarget = aimSpeed / Mathf.Abs(Vector3.Dot(transform.forward, lerpAimVector.normalized));
		targetVector = Vector3.Lerp(targetVector, lerpAimVector, Time.deltaTime * aimSpeed * dotToTarget);
		transform.LookAt(targetVector);
	}

	void FireBullet()
	{
		Transform bulletToFire = null;
		AudioClip sound = null;

		// Primary / Alt fire
		if (bAlternateArmed)
		{
			bulletToFire = Instantiate(alternateFirePrefab, firePoint.position, firePoint.rotation);
			sound = secondarySound;
		}
		else
		{
			bulletToFire = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
			sound = primarySound;
		}

		// Shoot it!
		if (bulletToFire != null)
		{
			Bullet newBullet = bulletToFire.GetComponent<Bullet>();
			if (newBullet != null)
			{
				newBullet.AddSpeedModifier(bulletSpeedModifier, transform, owningShooter);
			}

			AudioSource bulletSound = bulletToFire.gameObject.GetComponent<AudioSource>();
			if (bulletSound != null)
			{
				if (sound != null)
				{
					bulletSound.PlayOneShot(sound);
				}
			}
		}
	}
}
