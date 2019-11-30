using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemBar : MonoBehaviour
{
	public Transform[] itemPrefabs;
	public Transform[] itemSlots;
	public Transform itemSelector;

	private List<GameObject> items;
	private AudioSource audioPlayer;

    void Start()
    {
		audioPlayer = GetComponent<AudioSource>();
		InitObjects();
    }

	void InitObjects()
	{
		items = new List<GameObject>();
		int numObjs = itemPrefabs.Length;
		if (numObjs > 0)
		{
			for (int i = 0; i < numObjs; i++)
			{
				Transform newItem = Instantiate(itemPrefabs[i], Vector3.zero, Quaternion.identity);
				items.Add(newItem.gameObject);
				newItem.gameObject.SetActive(false);
			}
		}

		itemSelector.gameObject.SetActive(false);
	}

	public GameObject GetItem(int id)
	{
		GameObject result = null;
		if (id < itemPrefabs.Length)
		{
			if ((items != null) && items[id] != null)
			{
				result = items[id];
				//itemSelector.gameObject.SetActive(true);
				//itemSelector.position = itemSlots[id].transform.position;

				// Equip sound effect
				Tool thisTool = result.GetComponent<Tool>();
				if (thisTool != null)
				{
					AudioClip equipSound = thisTool.equipSound;
					if (equipSound != null)
					{
						audioPlayer.clip = equipSound;
						audioPlayer.Play();
					}
				}
			}
		}
		//else
		//{
		//	itemSelector.gameObject.SetActive(false);
		//}

		return result;
	}
}
