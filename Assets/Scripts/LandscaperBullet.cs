﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandscaperBullet : Bullet
{
	public float falloff = 1f;
	public bool bImpartVelocity = true;
	public AudioClip fireSound;
	public AudioClip impactSound;

	private AudioSource audioPlayer;
	private bool bHit = false;

	public override void Start()
	{
		base.Start();
		audioPlayer = GetComponent<AudioSource>();
		if (fireSound != null)
		{
			audioPlayer.PlayOneShot(fireSound);
		}
	}

	public override void Update()
	{
		base.Update();
		if (bHit)
		{
			audioPlayer.volume = Mathf.Lerp(audioPlayer.volume, 0f, Time.deltaTime * 2f);
		}
	}

	public override void LandHit(RaycastHit hit, Vector3 hitPosition)
	{
		base.LandHit(hit, hitPosition);

		if (!bHit && (impactParticles != null))
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
			float hitnormalScalar = Mathf.Clamp(Mathf.Pow(1f / Mathf.Abs(Vector3.Dot(Vector3.up, hit.normal)), 2f), 1f, 10f);
			float thisHitDamage = damage * Mathf.Clamp(hitnormalScalar, 1f, 2f);
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

			audioPlayer.PlayOneShot(impactSound);

			bHit = true;

			//Destroy(gameObject, 2f);
		}
	}

	public Vector3 GetVertexWorldPosition(Vector3 vertex, Transform owner)
	{
		return owner.localToWorldMatrix.MultiplyPoint3x4(vertex);
	}
}
