using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AbilityStruct
{
	public string AbilityName;
	public float AbilityValue;

	public AbilityStruct(string abilityName, float value)
	{
		AbilityName = abilityName;
		AbilityValue = value;
	}

	public void GiveName(string value)
	{
		AbilityName = value;
	}

	public void GiveValue(float value)
	{
		AbilityValue = value;
	}
}

public class AbilityChart : MonoBehaviour
{
	public string[] abilityNames = { "footspeed", "jump", "boost", "throw", "recovery" };
	public List<AbilityStruct> abilityStructs;
	private HUDAnimator hud;

	void Start()
    {
		abilityStructs = new List<AbilityStruct>();
		int numAbilities = abilityNames.Length;
		for (int i = 0; i < numAbilities; i++)
		{
			AbilityStruct ab = new AbilityStruct("", 1);
			ab.GiveName(abilityNames[i]);
			ab.GiveValue(1f);
			abilityStructs.Add(ab);
		}
		hud = FindObjectOfType<HUDAnimator>();
	}

	public void IncreaseAbility(int abilityID, float value)
	{
		abilityStructs[abilityID].GiveValue(value);
		Debug.Log(abilityNames[abilityID] + " +" + value + "   at " + Time.time);
	}

	private void Update()
	{
		if (Input.GetButtonDown("Jump"))
		{
			IncreaseAbility(1, 1);
		}

		if (Input.GetButtonDown("Boost"))
		{
			IncreaseAbility(2, 1);
		}
	}

}
