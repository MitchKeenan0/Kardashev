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
		if (other.GetComponentInChildren<PlayerBody>())
		{
			invitationText.SetActive(true);
			other.GetComponentInChildren<PlayerBody>().SetRecovery(true, transform.parent.gameObject);
		}
	}


	private void OnTriggerExit(Collider other)
	{
		if (other.GetComponentInChildren<PlayerBody>())
		{
			invitationText.SetActive(false);
			other.GetComponentInChildren<PlayerBody>().SetRecovery(false, transform.parent.gameObject);
		}
	}

}
