using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeObject : MonoBehaviour
{
	public float interval = 0.01f;
	MeshRenderer render;
	IEnumerator fadeInCoroutine;
	IEnumerator fadeOutCoroutine;
	IEnumerator shineCoroutine;


	void Start()
    {
		render = GetComponent<MeshRenderer>();

		InitFade();
    }


	void InitFade()
	{
		render = transform.gameObject.GetComponent<MeshRenderer>();
		ChangeRenderMode(render.material, BlendMode.Fade);
		Color c = render.material.color;
		c.a = 0f;
		render.material.color = c;
	}

	void InitFadeOut()
	{
		render = transform.gameObject.GetComponent<MeshRenderer>();
		render.material.DisableKeyword("_EMISSION");
		Color c = render.material.color;
		c.a = 1f;
		render.material.color = c;
		ChangeRenderMode(render.material, BlendMode.Transparent);
	}


	IEnumerator FadeIn(float aValue, float aTime)
	{
		float alpha = render.material.color.a;
		for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
		{
			Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha, aValue, t));
			render.material.color = newColor;

			if (render.material.color.a >= (1f - interval))
			{
				Color c = render.material.color;
				c.a = 1f;
				ChangeRenderMode(render.material, BlendMode.Opaque);
				StopCoroutine(fadeInCoroutine);
			}

			yield return null;
		}
	}

	IEnumerator FadeOut()
	{
		for (float f = 0f; f < 1f; f += interval)
		{
			Color c = render.material.color;
			c.a = 1-f;
			render.material.color = c;

			yield return new WaitForSeconds(interval);
		}

		if (render.material.color.a <= interval)
		{
			Color c = render.material.color;
			c.a = 0f;
			render.material.color = c;
			StopCoroutine(fadeOutCoroutine);

			if (GetComponent<StructureHarvester>())
			{
				GetComponent<StructureHarvester>().Despawn();
			}
		}
	}

	IEnumerator FadeTo(float aValue, float aTime)
	{
		float alpha = render.material.color.a;
		for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
		{
			Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha, aValue, t));
			render.material.color = newColor;

			if (render.material.color.a <= interval)
			{
				Color c = render.material.color;
				c.a = 0f;
				render.material.color = c;
				StopCoroutine(fadeOutCoroutine);

				if (GetComponent<StructureHarvester>())
				{
					GetComponent<StructureHarvester>().Despawn();
				}
			}
			
			yield return null;
		}
	}

	IEnumerator ShineTo(float aValue, float aTime)
	{
		for (float t = 0f; t < 1f; t += Time.deltaTime / aTime)
		{
			Color newColor = Color.white * Mathf.Lerp(0f, aValue, t);
			render.material.SetColor("_EmissionColor", newColor);

			yield return null;
		}
	}

	IEnumerator DullTo(float aValue, float aTime)
	{
		for (float t = 0f; t < 1f; t += Time.deltaTime / aTime)
		{
			Color newColor = Color.white * Mathf.Lerp(1f, aValue, t);
			render.material.SetColor("_EmissionColor", newColor);

			yield return null;
		}
	}


	public void StartFadeIn()
	{
		InitFade();
		fadeInCoroutine = FadeIn(1f, 3f);
		StartCoroutine(fadeInCoroutine);
	}

	public void StartFadeOut(float fadeTime)
	{
		InitFadeOut();
		fadeOutCoroutine = FadeTo(0f, fadeTime);
		StartCoroutine(fadeOutCoroutine);
	}

	public void StartShine(float value, float time)
	{
		render = transform.gameObject.GetComponent<MeshRenderer>();
		if (render != null)
		{
			ChangeRenderMode(render.material, BlendMode.Opaque);
			render.material.EnableKeyword("_EMISSION");
			shineCoroutine = ShineTo(value, time);
			StartCoroutine(shineCoroutine);
		}
	}

	public void EndShine(float fadeTime)
	{
		render = transform.gameObject.GetComponent<MeshRenderer>();
		if (render != null)
		{
			ChangeRenderMode(render.material, BlendMode.Opaque);
			render.material.EnableKeyword("_EMISSION");
			shineCoroutine = DullTo(0f, fadeTime);
			StartCoroutine(shineCoroutine);
		}
	}

	public enum BlendMode
	{
		Opaque,
		Cutout,
		Fade,
		Transparent
	}

	public static void ChangeRenderMode(Material standardShaderMaterial, BlendMode blendMode)
	{
		switch (blendMode)
		{
			case BlendMode.Opaque:
				standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				standardShaderMaterial.SetInt("_ZWrite", 1);
				standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
				standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
				standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				standardShaderMaterial.renderQueue = -1;
				break;
			case BlendMode.Cutout:
				standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				standardShaderMaterial.SetInt("_ZWrite", 1);
				standardShaderMaterial.EnableKeyword("_ALPHATEST_ON");
				standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
				standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				standardShaderMaterial.renderQueue = 2450;
				break;
			case BlendMode.Fade:
				standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				standardShaderMaterial.SetInt("_ZWrite", 0);
				standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
				standardShaderMaterial.EnableKeyword("_ALPHABLEND_ON");
				standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				standardShaderMaterial.renderQueue = 3000;
				break;
			case BlendMode.Transparent:
				standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				standardShaderMaterial.SetInt("_ZWrite", 0);
				standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
				standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
				standardShaderMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
				standardShaderMaterial.renderQueue = 3000;
				break;
		}
	}


}

