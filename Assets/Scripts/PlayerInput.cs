using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
	private Character player;
	private GameSystem game;
	private Menus menus;
	private HUD hud;

    void Start()
    {
		player = GetComponent<Character>();
		game = FindObjectOfType<GameSystem>();
		menus = FindObjectOfType<Menus>();
		hud = FindObjectOfType<HUD>();
    }

    void Update()
    {
		UpdateInput();
    }

	void UpdateInput()
	{
		// Move direction
		player.SetForward(Input.GetAxisRaw("Vertical"));
		player.SetLateral(Input.GetAxisRaw("Horizontal"));

		// Jump
		if (Input.GetButtonDown("Jump"))
		{
			player.Jump();
		}

		// Boost
		if (Input.GetButtonDown("Boost"))
		{
			player.Boost();
		}

		// Trigger down
		if (Input.GetMouseButtonDown(0))
		{
			player.PrimaryTrigger(true);
		}

		// Trigger up
		else if (Input.GetMouseButtonUp(0))
		{
			player.PrimaryTrigger(false);
		}

		// Alt trigger down
		if (Input.GetMouseButtonDown(1))
		{
			player.AlternateTrigger(true);
		}

		// Alt trigger up
		if (Input.GetMouseButtonUp(1))
		{
			player.AlternateTrigger(false);
		}

		// Interact
		if (Input.GetButtonDown("Interact"))
		{
			player.Interact();
		}

		// Pick-up thowing tools
		if (Input.GetButtonDown("Pickup"))
		{
			player.PickupItem();
		}

		// Recall vehicle
		if (Input.GetButton("Recall"))
		{
			player.RecallTrigger(true);
		}
		if (Input.GetButtonUp("Recall"))
		{
			player.RecallTrigger(false);
		}

		// Item selection
		if (Input.GetButtonDown("1"))
		{
			player.EquipItem(1);
		}
		if (Input.GetButtonDown("2"))
		{
			player.EquipItem(2);
		}
		if (Input.GetButtonDown("3"))
		{
			player.EquipItem(3);
		}
		if (Input.GetButtonDown("4"))
		{
			player.EquipItem(4);
		}
		if (Input.GetButtonDown("5"))
		{
			player.EquipItem(5);
		}
		if (Input.GetButtonDown("6"))
		{
			player.EquipItem(6);
		}
	}
}
