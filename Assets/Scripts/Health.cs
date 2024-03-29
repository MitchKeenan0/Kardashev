﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
	public int maxHealth = 10000;
	public int armour = 10;
	public ParticleSystem damageParticles;

	private int health = 0;
	private HealthBar healthBar;
	private Character myCharacter;

	void Start()
    {
		healthBar = FindObjectOfType<HealthBar>();
		myCharacter = GetComponent<Character>();
		health = maxHealth;
		if (!myCharacter.IsBot())
			healthBar.SetHealth(maxHealth, true);
	}

    public void TakeDamage(float value)
	{
		damageParticles.Play();
		int incomingDamage = Mathf.FloorToInt(value * (1f / armour));
		health = Mathf.FloorToInt(Mathf.Clamp(health - incomingDamage, 0, maxHealth));
		if (!myCharacter.IsBot())
			healthBar.SetHealth(health, false);

		if (health <= 0f)
		{
			myCharacter.Die();

			// Player's game over screen
			if (!myCharacter.IsBot())
			{
				GameSystem game = FindObjectOfType<GameSystem>();
				if (game != null)
				{
					game.PlayerDied();
				}
			}

			Explode();
		}
	}

	void Explode()
	{
		// Kerplode character pieces
		MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer mesh in meshes)
		{
			GameObject meshGO = mesh.gameObject;
			meshGO.transform.parent = null;
			meshGO.transform.position += Random.insideUnitSphere * 0.6f;
			meshGO.transform.rotation *= Random.rotation;

			Rigidbody grb = null;
			if (meshGO.GetComponent<Rigidbody>())
			{
				grb = meshGO.GetComponent<Rigidbody>();
				meshGO.GetComponent<Rigidbody>().AddExplosionForce(100f, transform.position, 5f);
			}
			else
			{
				// Add rigidbody and colliders
				grb = meshGO.AddComponent<Rigidbody>();
				if (meshGO.GetComponent<MeshFilter>())
				{
					MeshCollider meshCol = meshGO.AddComponent<MeshCollider>();
					meshCol.convex = true;
					meshCol.sharedMesh = meshGO.GetComponent<MeshFilter>().mesh;
					meshCol.material = GetComponent<Collider>().material;
				}
			}

			if (grb != null)
			{
				grb.isKinematic = false;
				grb.AddExplosionForce(100f, transform.position, 5f);
			}
		}
	}
}
