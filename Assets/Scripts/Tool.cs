using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : MonoBehaviour
{
	public string toolName;
	public AudioClip equipSound;
	public AudioClip primarySound;
	public bool bPlayPrimaryActivation = true;
	public AudioClip secondarySound;
	public bool bPlaySecondaryActivation = true;
	public Transform owner;

	private bool bActive = false;
	private bool bAlternateActive = false;
	private AudioSource audioPlayer;

	void Start()
	{
		audioPlayer = GetComponent<AudioSource>();
	}

	public virtual void InitTool(Transform value)
	{
		owner = value;
	}

	public virtual void SetToolActive(bool value)
	{
		bActive = value;
		if (value)
		{
			if (bPlayPrimaryActivation)
			{
				if (!audioPlayer)
					audioPlayer = GetComponent<AudioSource>();
				audioPlayer.PlayOneShot(primarySound);
			}
		}
	}

	public virtual void SetToolAlternateActive(bool value)
	{
		bAlternateActive = value;
		if (value)
		{
			if (bPlaySecondaryActivation)
			{
				audioPlayer.PlayOneShot(secondarySound);
			}
		}
	}

	public virtual void ActivateTool()
	{

	}
}
