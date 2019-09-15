using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleBullet : Bullet
{
	private GrapplingHook grapplingHook;
	private TrailRenderer trail;



	public override void Start()
	{
		base.Start();

		trail = GetComponentInChildren<TrailRenderer>();
		trail.enabled = false;
		trail.Clear();
	}


	public override void Update()
	{
		base.Update();

	}


	public override void AddSpeedModifier(float value, Transform gun, Transform shooter)
	{
		base.AddSpeedModifier(value, gun, shooter);
		grapplingHook = gun.GetComponent<GrapplingHook>();

		trail.Clear();
		trail.enabled = (value > 0f);
	}


	public override void LandHit(GameObject hitObj, Vector3 hitPosition)
	{
		base.LandHit(hitObj, hitPosition);

		grapplingHook.RegisterHit(hitObj, hitPosition);
	}
}
