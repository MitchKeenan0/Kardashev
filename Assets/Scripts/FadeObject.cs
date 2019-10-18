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
		ChangeRenderMode(render.material, BlendMode.Transparent);
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

		if (render.material.color.a >= (1f - interval))
		{
			ChangeRenderMode(render.material, BlendMode.Opaque);
		}
	}


	public void StartFading()
	{
		InitFade();
		fader = Fade();
		StartCoroutine(fader);
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

