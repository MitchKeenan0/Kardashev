using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artifact : MonoBehaviour
{
	public Collider solidCollider;
	public ParticleSystem airParticles;
	public Transform impactParticles;
	public Vector3 spawnOffset = Vector3.zero;
	public float spawnOffsetRandomize = 1f;

	private Rigidbody rb;
	private Character player;
	private Materializer materializer;
	private FadeObject fader;

	private List<Spear> stuckSpears;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		materializer = GetComponent<Materializer>();
		player = FindObjectOfType<Character>();
		fader = GetComponent<FadeObject>();
		stuckSpears = new List<Spear>();
	}

	public void Disperse()
	{
		if (!rb.isKinematic)
		{
			materializer.Disperse();
			Debug.Log("dispersing");

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

	public IEnumerator Despawn(float delayTime)
	{
		yield return new WaitForSeconds(delayTime);

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

				if (player == null)
					player = FindObjectOfType<Character>();
				Vector3 toPlayer = player.transform.position - transform.position;
				rb.velocity = toPlayer * velocity;
				rb.AddTorque(transform.rotation.eulerAngles);
			}
			else
			{
				rb.isKinematic = true;
				var em = airParticles.emission;
				em.enabled = false;
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (impactParticles != null && (!collision.transform.GetComponent<Spear>()))
		{
			foreach (var contact in collision.contacts)
			{
				Transform newImpact = Instantiate(impactParticles, contact.point, Quaternion.identity);
				Destroy(newImpact.gameObject, 5f);

				if (collision.transform.GetComponent<Character>())
				{
					//collision.transform.GetComponent<Character>().TakeSlam(rb.velocity, rb.velocity.magnitude, true);
				}
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.GetComponent<Character>())
		{
			player = other.gameObject.GetComponent<Character>();
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
}
