using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMenus : MonoBehaviour
{
	public Slider sensitivitySlider;
	public GameObject vehiclePointer;
	public Text vehicleDistanceText;
	public Text framerateText;
	public GameObject recallPrompt;
	public GameObject crosshair;

	private GameSystem game;
	private SmoothMouseLook mouseLook;
	private Camera cam;
	private Vehicle vehicle;
	private PlayerBody player;
	private float lastFrameTime;
	private Vector3 vehiclePointerPosition;
	private bool bHoldRecallPrompt = false;

	void Start()
    {
		game = FindObjectOfType<GameSystem>();
		mouseLook = FindObjectOfType<SmoothMouseLook>();
		cam = mouseLook.GetComponentInChildren<Camera>();
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
		vehiclePointer.transform.position = Vector3.Lerp(vehiclePointer.transform.position, vehiclePointerPosition, Time.smoothDeltaTime * 15f);

		// Update distance info text
		int meters = Mathf.FloorToInt(Vector3.Distance(player.transform.position, worldPosition) * 0.3f);
		vehicleDistanceText.text = meters + "m";
	}

	Vector3 GetVehiclePointerPosition(Vector3 worldPosition)
	{
		Vector3 toWorldPos = (worldPosition - cam.transform.position);
		Vector3 screenPos = cam.WorldToScreenPoint(worldPosition);

		// Special case if vehicle is more than 1000u away
		if (toWorldPos.magnitude >= 1000f)
		{
			float beyondRange = (toWorldPos.magnitude - 1000f) + 10f;
			Vector3 retroPosition = toWorldPos.normalized * -beyondRange;
			Vector3 clampedWorldPos = worldPosition + retroPosition;
			screenPos = cam.WorldToScreenPoint(clampedWorldPos);
		}

		// Behind camera case
		Vector3 toPointer = toWorldPos.normalized;
		float dotToPointer = Vector3.Dot(cam.transform.forward, toPointer);
		if (dotToPointer < 0f)
		{
			if (!recallPrompt.activeInHierarchy){
				recallPrompt.SetActive(true);
			}

			if (screenPos.x < Screen.width / 2){
				screenPos.x = Screen.width - 150f;
			}
			else{
				screenPos.x = 150f;
			}

			if (toPointer.y > 0){
				screenPos.y = Screen.height - 150f;
			}
			else if (toPointer.y < 0){
				screenPos.y = 150f;
			}
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
		crosshair.SetActive(false);
	}

	public void ExitPause()
	{
		game.ReturnToGame();
		crosshair.SetActive(true);
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
		mouseLook.sensitivitySlider = sensitivitySlider;
		mouseLook.OptionsSensitivity(value);
	}
}
