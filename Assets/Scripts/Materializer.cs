using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Materializer : MonoBehaviour
{
	public Transform disperseParticles;

	private FadeObject fader;
	private IEnumerator fadeDelay;
	private IEnumerator shineDelay;
	private float shineEmission = 1f;
	private float shineInTime = 0.6f;
	private float shineOutTime = 0.1f;
	private float fadeDelayTime = 1f;
	private float fadeOutTime = 1f;

    public void Disperse()
	{
		Transform dematParticles = Instantiate(disperseParticles, transform.position, transform.rotation);
		dematParticles.SetParent(transform);

		var em = disperseParticles.GetComponent<ParticleSystem>().emission;
		em.rateOverTime = transform.localScale.magnitude * 15f;

		var sh = dematParticles.GetComponent<ParticleSystem>().shape;
		sh.shapeType = ParticleSystemShapeType.Mesh;
		if (GetComponent<MeshFilter>())
			sh.mesh = GetComponent<MeshFilter>().mesh;
		else if (GetComponentInChildren<MeshFilter>())
			sh.mesh = GetComponentInChildren<MeshFilter>().mesh;
		sh.scale = transform.localScale;

		if (GetComponent<Artifact>())
			GetComponent<Artifact>().Despawn(10f);
		else
			Destroy(dematParticles.gameObject, 10f);

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

		if (GetComponentInChildren<TrailRenderer>())
			GetComponentInChildren<TrailRenderer>().emitting = false;
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
}
