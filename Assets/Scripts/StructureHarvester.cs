using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureHarvester : MonoBehaviour
{
	public Transform particles;

	float shineEmission = 1f;
	float shineInTime = 0.6f;
	float shineOutTime = 0.1f;
	float fadeDelayTime = 1f;
	float fadeOutTime = 1f;

	private PlayerBody player;
	private FadeObject fader;
	private IEnumerator fadeDelay;
	private IEnumerator shineDelay;

	private void Start()
	{
		player = FindObjectOfType<PlayerBody>();
		fader = GetComponent<FadeObject>();
	}

	public void Disperse()
	{
		// Spawns particles in the shape of our mesh
		Transform newParticles = Instantiate(particles, transform.position, transform.rotation);
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

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform == player.transform)
		{
			player.SetStructure(this, true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.transform == player.transform)
		{
			player.SetStructure(this, false);
		}
	}

}
