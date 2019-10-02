using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFader : MonoBehaviour
{
	public float naturalIntensity = 1f;
	public float targetIntensity = 0f;
	public float fadeSpeed = 1f;
	private Light igt;


    void Start()
    {
		igt = GetComponent<Light>();
		if (igt == null)
		{
			igt = GetComponentInChildren<Light>();
		}

		if (igt != null)
		{
			igt.intensity = naturalIntensity;
		}
    }

    
    void Update()
    {
		if (igt != null)
		{
			if (igt.intensity != targetIntensity)
			{
				igt.intensity = Mathf.Lerp(igt.intensity, targetIntensity, Time.smoothDeltaTime * fadeSpeed);
			}
		}
    }
}
