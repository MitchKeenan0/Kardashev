using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDAnimator : MonoBehaviour
{
	public GameObject spearAquirePanel;
	public GameObject lootAquirePanel;
	public GameObject[] abilityAquirePool;

	private Animator animator;
	private IEnumerator timeoutCoroutine;

	void Start()
    {
		animator = spearAquirePanel.GetComponent<Animator>();
		spearAquirePanel.SetActive(false);
    }

    public void PlayAnimation(string value)
	{
		if (value == "GetSpear")
		{
			spearAquirePanel.SetActive(true);
			spearAquirePanel.GetComponent<AudioSource>().Play();
			timeoutCoroutine = TimeoutAnimObject(1f, spearAquirePanel);
			StartCoroutine(timeoutCoroutine);
		}

		animator.Play(value);
	}

	public void SetSpearScore(int value)
	{
		spearAquirePanel.SetActive(true);
		Text spearScoreText = spearAquirePanel.GetComponentInChildren<Text>();
		spearScoreText.text = "+" + value;
	}

	private IEnumerator TimeoutAnimObject(float value, GameObject target)
	{
		yield return new WaitForSeconds(value);
		target.SetActive(false);
	}
}
