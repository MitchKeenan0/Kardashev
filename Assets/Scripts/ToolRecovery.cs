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
		if (other.GetComponent<PlayerMovement>())
		{
			invitationText.SetActive(true);
			if (other.GetComponent<PlayerBody>())
			{
				other.GetComponent<PlayerBody>().SetRecovery(true, transform.parent.gameObject);
			}
		}
	}


	private void OnTriggerExit(Collider other)
	{
		if (other.GetComponent<PlayerMovement>())
		{
			invitationText.SetActive(false);
			if (other.GetComponent<PlayerBody>())
			{
				other.GetComponent<PlayerBody>().SetRecovery(false, transform.parent.gameObject);
			}
		}
	}

}
