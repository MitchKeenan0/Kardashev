using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability
{
	public string AbilityName = "";
	public float AbilityValue = 1f;

	public Ability(string abilityName, float value)
	{
		AbilityName = abilityName;
		AbilityValue = value;
	}

	public void GiveName(string value)
	{
		AbilityName = value;
	}

	public void GiveValue(float value, bool bAdditive)
	{
		if (bAdditive)
			AbilityValue += value;
		else
			AbilityValue = value;
	}
}
