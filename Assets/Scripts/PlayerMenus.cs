using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class PlayerMenus : MonoBehaviour
{
	public Slider sensitivitySlider;
	public GameObject vehiclePointer;
	public Text vehicleDistanceText;
	public Text objectiveDistanceText;
	public Text framerateText;
	public GameObject recallPrompt;
	public GameObject crosshair;
	public GameObject objectivePointer;
	public Transform Hud;
	public GameObject loadingPanel;
	public AudioMixer masterMixer;
	public Slider masterVolumeSlider;
	public GameObject spearChargePanel;
	public GameObject spearChargeBar;

	private GameSystem game;
	private SmoothMouseLook mouseLook;
	private Camera cam;
	private Vehicle vehicle;
	private PlayerBody player;
	private Objective objectif;
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
		player = GetComponentInParent<PlayerBody>();
		vehiclePointer.SetActive(false);
		recallPrompt.SetActive(true);
		lastFrameTime = Time.time;
		Hud.SetParent(null, false);
		loadingPanel.SetActive(false);
		spearChargePanel.SetActive(false);
	}

	void Update()
	{
		UpdateFrameCounter();
		if (bHintShowing && (objectif != null))
			UpdateHint(player.transform.position + objectif.location);
	}

	public void SetMasterVolume(float value)
	{
		masterMixer.SetFloat("masterVol", masterVolumeSlider.value);
	}

	void UpdateFrameCounter()
	{
		float deltaTime = (Time.time - lastFrameTime);
		float fps = 1f / deltaTime;
		if (Time.timeScale > 0f)
			framerateText.text = Mathf.Ceil(fps).ToString();
		lastFrameTime = Time.time;
	}

	public void SetSpearChargeActive(bool value)
	{
		spearChargePanel.SetActive(value);
	}

	public void SetSpearChargeValue(float value)
	{
		spearChargeBar.GetComponent<RectTransform>().sizeDelta = new Vector3(10f, value * 20f, 1f);
	}

	public void UpdateVehiclePointer(Vector3 worldPosition)
	{
		vehicleScreenPosition = WorldToScreen(worldPosition);
		vehiclePointer.transform.position = Vector3.Lerp(vehiclePointer.transform.position, vehicleScreenPosition, Time.smoothDeltaTime * 60f);

		// Update distance info text
		int meters = Mathf.FloorToInt(Vector3.Distance(player.transform.position, worldPosition) * 0.3f);
		vehicleDistanceText.text = meters + "m";
	}

	Vector3 WorldToScreen(Vector3 worldPosition)
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
			vehicleScreenPosition = WorldToScreen(vh.transform.position);
			vehiclePointer.transform.position = vehicleScreenPosition;
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

	public void SetHintActive(Objective obj, bool value)
	{
		objectif = obj;
		if (objectivePointer.activeInHierarchy != value)
		{
			objectivePointer.SetActive(value);
			bHintShowing = value;
			if (value)
			{
				objectiveScreenPosition = WorldToScreen(obj.location);
				objectivePointer.transform.position = objectiveScreenPosition;
			}
		}
	}

	void UpdateHint(Vector3 worldPosition)
	{
		objectiveScreenPosition = WorldToScreen(worldPosition);
		//if (objectif.bInfinitelyFar)
		//	objectiveScreenPosition += player.transform.position;
		objectivePointer.transform.position = Vector3.Lerp(objectivePointer.transform.position, objectiveScreenPosition, Time.smoothDeltaTime * 60f);

		// Update distance info text
		if (objectif != null)
		{
			if (!objectif.bInfinitelyFar)
			{
				int meters = Mathf.FloorToInt(Vector3.Distance(player.transform.position, worldPosition) * 0.3f);
				objectiveDistanceText.text = meters + "m";
			}
			else
			{
				objectiveDistanceText.text = "Unknown";
			}
		}
	}

	// Menu options..

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

	public void SetSensitivity(float value)
	{
		mouseLook.sensitivitySlider = sensitivitySlider;
		mouseLook.OptionsSensitivity(value);
	}
}
