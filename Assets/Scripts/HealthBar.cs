using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
	public Image healthBarImage;
	public Text healthBarText;
	public int min;
	public int max;

	private int currentHealth;
	private float healthPercentage;

	public void SetHealth(int value, bool bNewMaxHealth)
	{
		if (bNewMaxHealth)
			max = value;

		currentHealth = value;
		healthPercentage = (float)currentHealth / (float)(max - min);

		healthBarText.text = string.Format("{0} HP", Mathf.RoundToInt(healthPercentage * max));
		healthBarImage.fillAmount = healthPercentage;
	}

	public int CurrentHealth()
	{
		return currentHealth;
	}
}
