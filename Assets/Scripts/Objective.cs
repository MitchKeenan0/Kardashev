using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objective : MonoBehaviour
{
	public Vector3 location;
	public bool bInfinitelyFar = false;
	public bool bShowHint = false;
	public bool bHintLooping = false;
	public float firstHintDebutTime = 1.5f;
	public float hintAppearanceTime = 3.4f;
	public float hintIntervalTime = 10f;

	private HUD hud;
	private IEnumerator hintBeginCoroutine;
	private IEnumerator hintLoopingCoroutine;

    void Start()
    {
		hud = FindObjectOfType<HUD>();
		location = Vector3.forward * 99999999f;

		hintBeginCoroutine = BeginHints();
		StartCoroutine(hintBeginCoroutine);
    }

	IEnumerator BeginHints()
	{
		yield return new WaitForSeconds(firstHintDebutTime);
		SetHintVisible(true);
	}

	void SetHintVisible(bool value)
	{
		hud = FindObjectOfType<HUD>();
		hud.SetHintActive(this, true);
	}
}
