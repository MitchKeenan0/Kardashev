using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCharacter : MonoBehaviour
{
	public float range = 1.0f;
	public int health = 1;

	public ParticleSystem moveParticles;
	public ParticleSystem damageParticles;
	public Transform targetSpritePrefab;

	private HexPanel currentHex;
	private HexPanel targetHex;
	private Transform targetTransform;
	private Transform targetSprite;

	
    void Start()
    {
		targetSprite = Instantiate(targetSpritePrefab, Vector3.up * 100.0f, Quaternion.identity);
    }


	// To be called once per move by the game
	public void UpdateCharacter()
	{
		if (targetTransform != null)
		{
			Vector3 targetPos = targetTransform.position;
			CharacterMove(targetPos);
		}

		CharacterThink();
	}


	void CharacterThink()
	{
		// Sphere-cast to evaluate targets
		Collider[] rawNeighbors = Physics.OverlapSphere(transform.position, range);
		int numHits = rawNeighbors.Length;
		if (numHits > 0)
		{
			for (int i = 0; i < numHits; i++)
			{

				HexPanel hex = rawNeighbors[i].transform.gameObject.GetComponent<HexPanel>();
				if ((hex != null)
					&& (hex.gameObject != gameObject)
						&& hex.IsPopulated())
				{

					if (!hex.IsFrozen())
					{
						targetTransform = hex.transform;
						targetHex = hex;

						targetSprite.transform.position = targetTransform.position;
						targetSprite.transform.SetParent(targetTransform);

						break;
					}
				}
			}
		}
	}


	void CharacterMove(Vector3 position)
	{
		if (targetHex != null)
		{
			
			transform.position = position;
			transform.SetParent(targetTransform);

			targetSprite.transform.position = Vector3.up * 100.0f;
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
