using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingTool : Tool
{
	public Transform firePoint;
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
		float chargePower = Mathf.Clamp((Time.time - timeAtTriggerDown), 1f, 10f);
		Vector3 fireVelocity = (Camera.main.transform.forward * (throwPower * chargePower));

		Vector3 transformOffset = owner.position - transform.position;
		fireVelocity += transformOffset;

		Vector3 ownerVelocity = owner.GetComponent<CharacterController>().velocity * 0.15f;
		fireVelocity += ownerVelocity;

		Transform newThrowingTransform = Instantiate(throwingPrefab, firePoint.position, firePoint.rotation);

		Rigidbody throwingRb = newThrowingTransform.GetComponent<Rigidbody>();
		throwingRb.velocity = fireVelocity;
	}


}
