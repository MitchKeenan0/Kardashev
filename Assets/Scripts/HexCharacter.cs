using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCharacter : MonoBehaviour
{
	public float range = 1.0f;
	public int health = 1;
	public float moveRate = 0.5f;

	public ParticleSystem moveParticles;
	public ParticleSystem damageParticles;
	public Transform targetSpritePrefab;

	private HexPanel currentHex = null;
	private HexPanel targetHex = null;
	private Transform targetTransform;
	private Transform targetSprite;

	private float timeAtLastMove = 0.0f;
	private bool bCharacterEnabled = false;

	public void SetCharacterEnabled(bool value)
	{
		bCharacterEnabled = value;
	}

	
    void Start()
    {
		targetSprite = Instantiate(targetSpritePrefab, Vector3.up * 100.0f, Quaternion.identity);

		CharacterThink();
    }


	// To be called once per move by the game
	public void UpdateCharacter()
	{
		if (bCharacterEnabled)
		{
			CharacterThink();

			if ((Time.time - timeAtLastMove) >= moveRate)
			{
				CharacterMove(targetTransform.position);
			}
		}
	}


	void CharacterThink()
	{
		// Sphere-cast to evaluate targets
		Collider[] rawNeighbors = Physics.OverlapSphere(transform.position, range);
		int numHits = rawNeighbors.Length;

		if (numHits > 0)
		{
			bool foundTarget = false;

			// Search for populated target hex
			for (int i = 0; i < numHits; i++)
			{
				HexPanel hex = rawNeighbors[i].transform.gameObject.GetComponent<HexPanel>();
				if ((hex != null) && (hex.gameObject != gameObject) && !hex.IsFrozen())
				{
					targetHex = hex;
					targetTransform = hex.transform;
					
					if (currentHex == null)
					{
						currentHex = hex;
					}

					if (targetSprite != null)
					{
						targetSprite.transform.position = targetTransform.position;
						targetSprite.transform.SetParent(targetTransform);
					}

					foundTarget = true;

					break;
				}
			}

			// Or settle for a move hex
			if (!foundTarget)
			{
				while (targetTransform == null)
				{
					int randomInt = Mathf.RoundToInt(Random.Range(0.0f, numHits - 1));
					HexPanel moveHex = rawNeighbors[randomInt].transform.gameObject.GetComponent<HexPanel>();
					if (moveHex != null)
					{
						if ((moveHex != currentHex) && (!moveHex.IsFrozen()))
						{
							targetHex = moveHex;
							targetTransform = moveHex.transform;

							if (currentHex == null)
							{
								currentHex = moveHex;
							}

							if (targetSprite != null)
							{
								targetSprite.transform.position = targetTransform.position;
								targetSprite.transform.SetParent(targetTransform);
							}
						}
					}
				}
			}
		}
	}


	void CharacterMove(Vector3 position)
	{
		if (currentHex != null)
		{
			if ((currentHex.GetMovedThisTurn()) || !targetHex.IsPopulated())
			{
				if (targetHex != null)
				{
					transform.position = position;
					transform.SetParent(targetTransform);

					if (targetSprite != null)
					{
						targetSprite.transform.position = Vector3.up * 100.0f;
					}

					currentHex = targetHex;

					targetHex.SetPopulated(false);

					Transform person = targetTransform.Find("Person(Clone)");
					if (person)
					{
						Destroy(person.gameObject);
					}
				}
			}
		}

		timeAtLastMove = Time.time;

		targetTransform = null;
		targetHex = null;

		CharacterThink();
	}


	public void DestructCharacter()
	{
		if (targetSprite != null)
		{
			Destroy(targetSprite.gameObject);
		}
	}

}
