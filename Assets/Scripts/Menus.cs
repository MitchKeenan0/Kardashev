using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class Menus : MonoBehaviour
{
	public Slider sensitivitySlider;
	public Slider masterVolumeSlider;
	
	public GameObject loadingPanel;
	public AudioMixer masterMixer;

	private GameSystem game;
	private SmoothMouseLook mouseLook;
	private Camera cam;
	private Character player;
	private float lastFrameTime;
	private Vector3 vehicleScreenPosition;
	private Vector3 objectiveScreenPosition;
	private bool bHoldRecallPrompt = false;
	private bool bHintShowing = false;

	void Start()
    {
		game = FindObjectOfType<GameSystem>();
		mouseLook = FindObjectOfType<SmoothMouseLook>();
		cam = mouseLook.GetComponentInChildren<Camera>();
		player = FindObjectOfType<Character>();
		loadingPanel.SetActive(false);
	}

    void Update()
    {
		
	}

	// Menu options..

	public void EnterPause()
	{
		game.SetPaused(true);
		///add hud.hide
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

	public void ResetLevel()
	{
		game.ResetLevel();
	}

	public void ToMainMenu()
	{
		game.ExitToMenu();
	}

	public void QuitGame()
	{
		game.ExitGame();
	}


	// Options options..

	public void SetSensitivity(float value)
	{
		mouseLook.sensitivitySlider = sensitivitySlider;
		mouseLook.OptionsSensitivity(value);
	}

	public void SetMasterVolume(float value)
	{
		masterMixer.SetFloat("masterVol", masterVolumeSlider.value);
	}
}
