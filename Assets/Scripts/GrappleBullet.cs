using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleBullet : Bullet
{
	public Transform reelActiveEffects;

	private GrapplingHook grapplingHook;
	private TrailRenderer trail;
	private Vector3 deltaV;
	private Vector3 lastPos;


	public override void Start()
	{
		base.Start();

		reelActiveEffects.gameObject.SetActive(false);
	}


	public override void Update()
	{
		base.Update();
		
	}


	public override void LandHit(RaycastHit hit, Vector3 hitPosition)
	{
		base.LandHit(hit, hitPosition);

		grapplingHook = GetOwningGun().GetComponent<GrapplingHook>();
		if ((grapplingHook != null) && (hit.transform != null))
		{
			grapplingHook.RegisterHit(hit.transform.gameObject, hitPosition);
		}
	}

	public void SetLightActive(bool value)
	{
		reelActiveEffects.gameObject.SetActive(value);
	}


}
