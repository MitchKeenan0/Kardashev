using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnMouseOverColor : MonoBehaviour
{
	public SpriteRenderer citySprite;
	public Transform cityNameText;

	private bool bClicked = false;


	
	void Start()
    {
		if (cityNameText != null)
		{
			cityNameText.gameObject.SetActive(false);
		}
	}

	private void OnMouseEnter()
	{
		if (!bClicked)
		{
			if (cityNameText != null)
			{
				cityNameText.gameObject.SetActive(true);
			}

			if (citySprite != null)
			{
				citySprite.color = Color.white;
			}
		}
	}

	private void OnMouseExit()
	{
		if (!bClicked)
		{
			if (cityNameText != null)
			{
				cityNameText.gameObject.SetActive(false);
			}

			if (citySprite != null)
			{
				citySprite.color = Color.black;
			}
		}
	}

	private void OnMouseDown()
	{
		bClicked = true;
		citySprite.color = Color.white;
	}
}
