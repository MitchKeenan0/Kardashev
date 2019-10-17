using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemBar : MonoBehaviour
{
	public Transform[] itemPrefabs;
	private List<GameObject> items;

	public GameObject GetItem(int id)
	{
		GameObject result = null;
		if ((items != null) && items[id] != null)
		{
			result = items[id];
		}
		return result;
	}


    void Start()
    {
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
				Transform newItem = Instantiate(itemPrefabs[i], Vector3.up * -5000, Quaternion.identity);
				items.Add(newItem.gameObject);
			}
		}
	}
}
