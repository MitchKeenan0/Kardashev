using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeObject : MonoBehaviour
{
	public float interval = 0.01f;
	MeshRenderer render;
	IEnumerator fader;

    void Start()
    {
		InitFade();
    }

	void InitFade()
	{
		render = transform.gameObject.GetComponent<MeshRenderer>();
		Color c = render.material.color;
		c.a = 0f;
		render.material.color = c;
	}


    IEnumerator Fade()
	{
		for (float f = interval; f < 1f; f += interval)
		{
			Color c = render.material.color;
			c.a = f;
			render.material.color = c;

			yield return new WaitForSeconds(interval);
		}
	}


	public void StartFading()
	{
		InitFade();
		fader = Fade();
		StartCoroutine(fader);
	}
}
