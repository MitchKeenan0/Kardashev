using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolRecovery : MonoBehaviour
{
	public GameObject invitationText;

	private void Start()
	{
		invitationText.SetActive(false);
	}

	public void SetColliderActive(bool value)
	{
		GetComponent<Collider>().enabled = value;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponentInChildren<Character>())
		{
			invitationText.SetActive(true);
			other.GetComponentInChildren<Character>().SetRecovery(true, transform.parent.gameObject);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.GetComponentInChildren<Character>())
		{
			invitationText.SetActive(false);
			other.GetComponentInChildren<Character>().SetRecovery(false, transform.parent.gameObject);
		}
	}
}
