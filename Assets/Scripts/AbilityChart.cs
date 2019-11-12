using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityChart : MonoBehaviour
{
	public string[] abilityNames = { "footspeed", "jump", "boost", "throw", "recovery" };
	public List<Ability> abilityStructs;
	private HUD hud;
	private Character player;

	void Start()
    {
		abilityStructs = new List<Ability>();
		player = GetComponent<Character>();
		hud = FindObjectOfType<HUD>();

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
			case 0:
				player.moveSpeed += value;
				break;
			case 1:
				player.jumpSpeed += value;
				break;
			case 2:
				player.boostScale += value;
				break;

			default: break;
		}

		if (hud != null)
		{
			hud.AbilityLevel(ab.AbilityName, value);
		}
	}

}
