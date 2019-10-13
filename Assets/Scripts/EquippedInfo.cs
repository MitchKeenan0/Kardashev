using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquippedInfo : MonoBehaviour
{
	public Text toolName;
	public Text toolReserve;

    void Start()
    {
        
    }
	
	public void SetToolName(string name)
	{
		toolName.text = name;
	}

	public void SetToolReserve(string value)
	{
		toolReserve.text = value;
	}

}
