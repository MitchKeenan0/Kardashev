using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnMouseOverColor : MonoBehaviour
{
	public SpriteRenderer citySprite;
	public Transform cityNameText;

	private bool bClicked = false;
	private Color originalColor;

	
	void Start()
    {
		if (cityNameText != null)
		{
			cityNameText.gameObject.SetActive(false);
		}

		originalColor = citySprite.color;
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
				Color clickColor = Color.white;
				clickColor.a = originalColor.a * 1.6f;
				citySprite.color = clickColor;
			}
		}
	}

	private void OnMouseDown()
	{
		bClicked = true;
		citySprite.color = Color.white;
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
				citySprite.color = originalColor;
			}
		}
	}
}
