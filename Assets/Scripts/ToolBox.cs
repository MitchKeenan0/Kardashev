using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolBox : MonoBehaviour
{
	// Single tile mode
	public Transform singleTileChargeTransform;
	private Text singleTileChargeText;
	private int m_singleTileCharges = 1;
	public int singleTileCharges = 1;

	// Start is called before the first frame update
    void Start()
    {
		singleTileChargeText = singleTileChargeTransform.GetComponentInChildren<Text>();
		m_singleTileCharges = singleTileCharges;
		singleTileChargeText.text = singleTileCharges.ToString();
    }

	public void ReloadSingleCharges()
	{
		singleTileCharges = m_singleTileCharges;
	}

	public void NewSingleChargeModifier(int value)
	{
		singleTileCharges += value;
		singleTileChargeText.text = singleTileCharges.ToString();

		if (value > 0)
		{
			m_singleTileCharges += value;
		}
	}
}
