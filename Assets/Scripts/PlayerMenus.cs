using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMenus : MonoBehaviour
{
	public Slider sensitivitySlider;

	private GameSystem game;
	private SmoothMouseLook cam;

    void Start()
    {
		game = FindObjectOfType<GameSystem>();
		cam = FindObjectOfType<SmoothMouseLook>();
    }

    public void EnterPause()
	{
		game.SetPaused(true);
	}

	public void ExitPause()
	{
		game.ReturnToGame();
	}

	public void EnterOptions()
	{
		game.EnterOptions();
	}

	public void ExitOptions()
	{
		game.ExitOptions();
	}

	public void ToMainMenu()
	{
		game.ExitToMenu();
	}

	public void QuitGame()
	{
		game.ExitGame();
	}

	// Options
	public void SetSensitivity(float value)
	{
		cam.sensitivitySlider = sensitivitySlider;
		cam.OptionsSensitivity(value);
	}
}
