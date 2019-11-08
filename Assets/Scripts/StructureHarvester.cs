using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureHarvester : MonoBehaviour
{
	public Collider solidCollider;
	public Transform disperseParticles;
	public ParticleSystem airParticles;
	public Transform impactParticles;
	public Vector3 spawnOffset = Vector3.zero;
	public float spawnOffsetRandomize = 1f;

	float shineEmission = 1f;
	float shineInTime = 0.6f;
	float shineOutTime = 0.1f;
	float fadeDelayTime = 1f;
	float fadeOutTime = 1f;

	private Rigidbody rb;
	private PlayerBody player;
	private FadeObject fader;
	private IEnumerator fadeDelay;
	private IEnumerator shineDelay;

	private List<Spear> stuckSpears;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		player = FindObjectOfType<PlayerBody>();
		fader = GetComponent<FadeObject>();
		stuckSpears = new List<Spear>();
	}

	public void Disperse()
	{
		if (!rb.isKinematic)
		{
			// Spawns particles in the shape of our mesh
			Transform newParticles = Instantiate(disperseParticles, transform.position, transform.rotation);
			var sh = newParticles.GetComponent<ParticleSystem>().shape;
			sh.shapeType = ParticleSystemShapeType.Mesh;
			sh.mesh = GetComponent<MeshFilter>().mesh;
			sh.scale = transform.localScale;

			Destroy(newParticles.gameObject, 10f);

			if (!GetComponent<FadeObject>())
			{
				fader = gameObject.AddComponent<FadeObject>();
			}

			if (fader != null)
			{
				fader.StartShine(shineEmission, shineInTime);

				shineDelay = KillShine();
				StartCoroutine(shineDelay);

				fadeDelay = DelayedFade();
				StartCoroutine(fadeDelay);
			}

			int numSpears = stuckSpears.Count;
			for (int i = 0; i < numSpears; i++)
			{
				if (stuckSpears[i] != null)
				{
					stuckSpears[i].SetPhysical(true);
				}
			}
		}
	}

	IEnumerator KillShine()
	{
		yield return new WaitForSeconds(shineInTime);

		fader.EndShine(shineOutTime);
	}

	IEnumerator DelayedFade()
	{
		fadeDelayTime = shineInTime + shineOutTime;
		yield return new WaitForSeconds(fadeDelayTime);

		fader.StartFadeOut(fadeOutTime);
	}

	public void Despawn()
	{
		player.SetStructure(this, false);
		Destroy(gameObject);
	}

	public void SetPhysical(bool value, float velocity)
	{
		if (!rb)
			rb = GetComponent<Rigidbody>();

		if (rb != null)
		{
			if (value)
			{
				rb.isKinematic = false;
				var em = airParticles.emission;
				em.enabled = true;

				rb.velocity = -spawnOffset * velocity;
				rb.AddTorque(transform.rotation.eulerAngles);
			}
			else
			{
				rb.isKinematic = true;
				var em = airParticles.emission;
				em.enabled = false;
				Debug.Log("SetKinematic");
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.GetComponent<PlayerBody>())
		{
			player = other.gameObject.GetComponent<PlayerBody>();
		}
		if (player != null && (other.transform == player.transform))
		{
			player.SetStructure(this, true);
		}

		if (other.gameObject.GetComponent<Spear>())
		{
			stuckSpears.Add(other.gameObject.GetComponent<Spear>());
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (player != null)
		{
			if (other.transform == player.transform)
			{
				player.SetStructure(this, false);
			}
		}

		if (other.gameObject.GetComponent<Spear>())
		{
			stuckSpears.Remove(other.gameObject.GetComponent<Spear>());
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (impactParticles != null && (!collision.transform.GetComponent<Spear>()))
		{
			ContactPoint contact = collision.GetContact(0);
			Transform newImpact = Instantiate(impactParticles, contact.point, Quaternion.identity);
			Destroy(newImpact.gameObject, 5f);
		}
	}

}
