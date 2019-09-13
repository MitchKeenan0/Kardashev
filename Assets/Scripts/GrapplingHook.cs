using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : Tool
{
	public Transform hookHeadPrefab;
	public Transform firePoint;
	public float range = 100f;
	public float shotSpeed = 100f;

	private Transform hookTransform;
	private Transform owner;
	private bool bHookOut;

	public override void InitTool(Transform value)
	{
		base.InitTool(value);
		owner = value;
	}

	public override void SetToolActive(bool value)
	{
		base.SetToolActive(value);
		FireGrapplingHook();
	}

	void Start()
    {
		hookTransform = Instantiate(hookHeadPrefab, firePoint.position, Quaternion.identity);
    }

    void Update()
    {
        
    }

	void FireGrapplingHook()
	{
		bHookOut = true;
		hookTransform.parent = null;
	}
	
	 
}
