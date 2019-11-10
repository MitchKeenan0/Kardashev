using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityChart : MonoBehaviour
{
	public string[] abilityNames = { "footspeed", "jump", "boost", "throw", "recovery" };
	public List<Ability> abilityStructs;
	private HUDAnimator hud;
	private PlayerMovement movement;
	private PlayerBody body;

	void Start()
    {
		abilityStructs = new List<Ability>();
		movement = GetComponent<PlayerMovement>();
		body = GetComponent<PlayerBody>();
		hud = FindObjectOfType<HUDAnimator>();

		int numAbilities = abilityNames.Length;
		for (int i = 0; i < numAbilities; i++)
		{
			Ability ab = new Ability("", 1);
			ab.GiveName(abilityNames[i]);
			ab.GiveValue(1f, false);
			abilityStructs.Add(ab);
		}
	}

	public void IncreaseAbility(int abilityID, float value)
	{
		Ability ab = abilityStructs[abilityID];
		ab.GiveValue(value, true);

		switch (abilityID)
		{
			case 0: movement.moveSpeed += value;
				break;
			case 1: movement.jumpSpeed += value;
				break;
			case 2: movement.boostScale += value;
				break;

			default: break;
		}

		hud.AbilityLevel(ab.AbilityName, value);
	}

}
