using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : MonoBehaviour
{
	public int LevelID = 1;
	private GameSystem game;

    void Start()
    {
		game = FindObjectOfType<GameSystem>();
		InitCity();
    }

    
    void InitCity()
	{
		Vector3 toCentre = transform.position - Vector3.zero;
		transform.LookAt(toCentre, Vector3.up);
	}

	public void ActivateCity()
	{
		game.GoToLevel(LevelID);
	}
}
