using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCharacter : MonoBehaviour
{
	public float range = 1.0f;
	public int health = 1;
	public float moveRate = 0.5f;
	public bool bSpreadOnTakeover = true;

	public ParticleSystem moveParticles;
	public ParticleSystem damageParticles;
	public Transform targetSpritePrefab;
	public HexPanel currentHex = null;

	private LineRenderer line;
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
		line = GetComponent<LineRenderer>();
		targetSprite = Instantiate(targetSpritePrefab, Vector3.up * 100.0f, Quaternion.identity);
		CharacterThink();
    }


	// To be called once per move by the game
	public void UpdateCharacter()
	{
		// For game-time on/off mode
		if (bCharacterEnabled)
		{
			if (targetHex != null)
			{
				CharacterMove(targetHex.transform.position);
			}

			CharacterThink();
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
				if ((hex != null) && !hex.IsFrozen() && !hex.bEnemy)
				{

					targetHex = hex;

					targetTransform = hex.transform;

					if (targetSprite != null)
					{
						targetSprite.transform.position = targetTransform.position;
						targetSprite.transform.SetParent(targetTransform);
					}

					foundTarget = true;

					UpdateLineRender(true);
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
		if (targetHex != null)
		{
			if (targetHex.IsFrozen())
			{
				targetHex = null;
				CharacterThink();
			}
			else
			{
				Transform person = targetTransform.Find("Person(Clone)");
				if (person)
				{
					Destroy(person.gameObject);
				}

				if (bSpreadOnTakeover && targetHex.IsPopulated())
				{
					// Make another!
					SpawnCopy();
				}
				else
				{
					// Move over it like a chess
					transform.position = position;
					transform.SetParent(targetTransform);

					if (currentHex != null)
					{
						currentHex.bEnemy = false;
						currentHex = targetHex;
						currentHex.bEnemy = true;
						currentHex.SetPopulated(false);
					}
				}

				// Reset targeting sprite
				if (targetSprite != null)
				{
					targetSprite.transform.position = Vector3.up * 100.0f;
				}

				timeAtLastMove = Time.time;

				targetTransform = null;
				targetHex = null;
			}
		}
	}


	void SpawnCopy()
	{
		Transform newCopy = Instantiate(transform, targetTransform.position, Quaternion.identity);
	}


	void UpdateLineRender(bool On)
	{
		if (On)
		{
			if (!line.enabled)
			{
				line.enabled = true;
			}

			Vector3 myLine = targetTransform.position - transform.position;
			line.SetPosition(1, myLine);
		}

		if (!On)
		{
			if (line.enabled)
			{
				line.enabled = false;
			}

			line.SetPosition(1, transform.position);
		}
	}


	public void DestructCharacter()
	{
		if (targetSprite != null)
		{
			Destroy(targetSprite.gameObject);
		}
	}

}
