using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingTool : Tool
{
	public Transform throwingPrefab;
	public float throwPower;

	private float timeAtTriggerDown;
	private bool bCharging;

	public override void InitTool(Transform value)
	{
		base.InitTool(value);

	}


	public override void SetToolActive(bool value)
	{
		base.SetToolActive(value);

		if (value)
		{
			timeAtTriggerDown = Time.time;
			bCharging = true;
		}
		else if (bCharging)
		{
			FireThrowingTool();
		}
	}


	public override void SetToolAlternateActive(bool value)
	{
		base.SetToolAlternateActive(value);
		
	}

	void Start()
    {
        
    }

    
	void FireThrowingTool()
	{
		Transform newThrowingTransform = Instantiate(throwingPrefab, transform.position, Quaternion.identity);
		Rigidbody throwingRb = newThrowingTransform.GetComponent<Rigidbody>();

		float chargePower = Mathf.Clamp((Time.time - timeAtTriggerDown), 1f, 10f);
		throwingRb.velocity = Camera.main.transform.forward * (throwPower * chargePower);
	}


}
