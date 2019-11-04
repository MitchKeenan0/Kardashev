using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : MonoBehaviour
{
	public string toolName;
	public AudioClip equipSound;
	public Transform owner;

	private bool bActive = false;
	private bool bAlternateActive = false;

	public virtual void InitTool(Transform value)
	{
		owner = value;
	}

	public virtual void SetToolActive(bool value)
	{
		bActive = value;
	}

	public virtual void SetToolAlternateActive(bool value)
	{
		bAlternateActive = value;
	}

	public virtual void ActivateTool()
	{

	}

    void Start()
    {
        
    }
}
