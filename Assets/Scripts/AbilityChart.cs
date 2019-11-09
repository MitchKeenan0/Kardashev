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
		int numAbilities = abilityStructs.Count;
		for (int i = 0; i < numAbilities; i++)
		{
			abilityStructs[i].GiveName(abilityNames[i]);
			abilityStructs[i].GiveValue(1f);
		}
		hud = FindObjectOfType<HUDAnimator>();
	}

	public void IncreaseAbility(int abilityID, float value)
	{
		abilityStructs[abilityID].GiveValue(value);
		// hud
	}

	private void Update()
	{
		if (Input.GetButtonDown("Jump"))
		{

		}
	}

}
