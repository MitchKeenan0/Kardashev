using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleBullet : Bullet
{
	private GrapplingHook grapplingHook;



	public override void Start()
	{
		base.Start();
	}


	public override void Update()
	{
		base.Update();

	}


	public override void AddSpeedModifier(float value, Transform gun, Transform shooter)
	{
		base.AddSpeedModifier(value, gun, shooter);
		grapplingHook = gun.GetComponent<GrapplingHook>();
	}


	public override void LandHit(GameObject hitObj, Vector3 hitPosition)
	{
		base.LandHit(hitObj, hitPosition);

		grapplingHook.RegisterHit(hitObj, hitPosition);
	}
}
