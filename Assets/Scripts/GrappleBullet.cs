using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleBullet : Bullet
{
	private GrapplingHook grapplingHook;
	private TrailRenderer trail;
	private Vector3 deltaV;
	private Vector3 lastPos;


	public override void Start()
	{
		base.Start();

	}


	public override void Update()
	{
		base.Update();
		
	}


	public override void LandHit(GameObject hitObj, Vector3 hitPosition)
	{
		base.LandHit(hitObj, hitPosition);

		grapplingHook = GetOwningGun().GetComponent<GrapplingHook>();
		if ((grapplingHook != null) && (hitObj != null))
		{
			grapplingHook.RegisterHit(hitObj, hitPosition);
		}
	}


}
