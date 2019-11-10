using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDAnimator : MonoBehaviour
{
	public GameObject spearAquirePanel;
	public GameObject lootAquirePanel;
	public List<GameObject> abilityAquirePool;

	private Animator animator;
	private IEnumerator spearTimeoutCoroutine;
	private IEnumerator abilityTimeoutCoroutine;

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
			spearTimeoutCoroutine = TimeoutSpearAnim(1f, spearAquirePanel);
			StartCoroutine(spearTimeoutCoroutine);
		}

		animator.Play(value);
	}

	public void SetSpearScore(int value)
	{
		spearAquirePanel.SetActive(true);
		Text spearScoreText = spearAquirePanel.GetComponentInChildren<Text>();
		spearScoreText.text = "+" + value;
	}

	public void AbilityLevel(string abilityName, float value)
	{
		foreach(GameObject go in abilityAquirePool)
		{
			if (!go.activeInHierarchy)
			{
				go.GetComponentInChildren<Text>().text = abilityName + " +" + value;
				go.SetActive(true);
				abilityTimeoutCoroutine = TimeoutAbilityAnim(1f, go);
				StartCoroutine(abilityTimeoutCoroutine);
				break;
			}
		}
	}

	private IEnumerator TimeoutAbilityAnim(float value, GameObject target)
	{
		yield return new WaitForSeconds(value);
		target.SetActive(false);
	}

	private IEnumerator TimeoutSpearAnim(float value, GameObject target)
	{
		yield return new WaitForSeconds(value);
		target.SetActive(false);
	}
}
