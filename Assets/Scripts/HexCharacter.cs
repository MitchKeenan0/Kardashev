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
		if ((Time.time - timeAtLastMove) >= moveRate)
		{
			// For game-time on/off mode
			if (bCharacterEnabled)
			{
				CharacterThink();

				if (targetHex != null)
				{
					CharacterMove(targetHex.transform.position);
					CharacterThink();
				}
			}

			timeAtLastMove = Time.time;
		}
	}


	void CharacterThink()
	{
		// Closest person
		GameObject target = null;
		float closestDistance = 99.9f;

		GameObject[] people = GameObject.FindGameObjectsWithTag("People");
		if (people.Length > 0)
		{
			for (int i = 0; i < people.Length; i++)
			{
				GameObject peep = people[i];
				HexPanel peepHex = peep.transform.parent.GetComponent<HexPanel>();
				if ((peepHex != null) && !peepHex.IsFrozen())
				{
					float dist = Vector3.Distance(transform.position, peep.transform.position);
					if (dist < closestDistance)
					{
						closestDistance = dist;
						target = peep;
					}
				}
			}
		}

		if (target != null)
		{
			Transform nearestPanel = RaycastToTarget(target.transform.position);
			if (nearestPanel != null)
			{
				HexPanel hexTarget = nearestPanel.GetComponent<HexPanel>();
				if (hexTarget != null)
				{
					targetHex = hexTarget;
					targetTransform = nearestPanel.transform;

					if (targetSprite != null)
					{
						targetSprite.transform.position = targetTransform.position;
						targetSprite.transform.SetParent(targetTransform);
					}
				}
			}
		}

		if (targetTransform != null)
		{
			UpdateLineRender(true);
		}
	}


	Transform RaycastToTarget(Vector3 target)
	{
		Transform result = null;
		RaycastHit[] hits;
		Vector3 start = transform.position;
		Vector3 direction = target - start;
		hits = Physics.RaycastAll(start, direction, range);
		if (hits.Length > 0)
		{
			result = hits[0].transform;
		}

		return result;
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
				if (person != null)
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
