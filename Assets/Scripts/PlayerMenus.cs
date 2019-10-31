using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMenus : MonoBehaviour
{
	public Slider sensitivitySlider;
	public GameObject vehiclePointer;
	public Text framerateText;
	public GameObject recallPrompt;

	private GameSystem game;
	private SmoothMouseLook cam;
	private Vehicle vehicle;
	private PlayerBody player;
	private float lastFrameTime;
	private Vector3 vehiclePointerPosition;
	private bool bHoldRecallPrompt = false;

	void Start()
    {
		game = FindObjectOfType<GameSystem>();
		cam = FindObjectOfType<SmoothMouseLook>();
		player = GetComponentInParent<PlayerBody>();
		vehiclePointer.SetActive(false);
		recallPrompt.SetActive(true);
		lastFrameTime = Time.time;
	}

	void Update()
	{
		UpdateFrameCounter();
	}

	void UpdateFrameCounter()
	{
		float deltaTime = (Time.time - lastFrameTime);
		float fps = 1f / deltaTime;
		if (Time.timeScale > 0f)
			framerateText.text = Mathf.Ceil(fps).ToString();
		lastFrameTime = Time.time;
	}

	public void UpdateVehiclePointer(Vector3 worldPosition)
	{
		vehiclePointerPosition = GetVehiclePointerPosition(worldPosition);
		vehiclePointer.transform.position = Vector3.Lerp(vehiclePointer.transform.position, vehiclePointerPosition, Time.smoothDeltaTime * 30f);
	}

	Vector3 GetVehiclePointerPosition(Vector3 worldPosition)
	{
		Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
		Vector3 toPointer = (worldPosition - Camera.main.transform.position).normalized;
		float dotToPointer = Vector3.Dot(Camera.main.transform.forward, toPointer);
		if (dotToPointer < 0f)
		{
			if (!recallPrompt.activeInHierarchy)
			{
				recallPrompt.SetActive(true);
				Debug.Log(bHoldRecallPrompt);
			}

			if (screenPos.x < Screen.width / 2)
			{
				screenPos.x = Screen.width - 150f;
			}
			else
			{
				screenPos.x = 150f;
			}

			if (toPointer.y > 0)
			{
				screenPos.y = Screen.height - 150f;
			}
			else if (toPointer.y < 0)
			{
				screenPos.y = 150f;
			}
		}
		else if (recallPrompt.activeInHierarchy)
		{
			recallPrompt.SetActive(false);
		}

		screenPos.x = Mathf.Clamp(screenPos.x, 150f, Screen.width - 150f);
		screenPos.y = Mathf.Clamp(screenPos.y, 300f, Screen.height - 150f);

		return screenPos;
	}

	public void SetVehiclePointerActive(Vehicle vh, bool value)
	{
		vehicle = vh;
		vehiclePointer.SetActive(value);
		if (value)
		{
			vehiclePointerPosition = GetVehiclePointerPosition(vh.transform.position);
			vehiclePointer.transform.position = vehiclePointerPosition;
		}
	}

	public void SetRecallPromptActive(bool value)
	{
		if (recallPrompt.activeInHierarchy != value)
		{
			recallPrompt.SetActive(value);
			bHoldRecallPrompt = value;
		}
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

	// Options
	public void SetSensitivity(float value)
	{
		cam.sensitivitySlider = sensitivitySlider;
		cam.OptionsSensitivity(value);
	}
}
