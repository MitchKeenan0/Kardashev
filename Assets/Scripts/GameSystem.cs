﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSystem : MonoBehaviour
{
	public Transform player;
	public Transform fadeBlackScreen;
	public float fadeSpeed = 3f;
	public Transform cityPrefab;
	public GameObject deathScreen;
	public GameObject pauseScreen;
	public GameObject optionsScreen;

	private int ScreenX;
	private int ScreenY;

	GameObject center;
	GameObject[] allGameobjects;

	private CursorLockMode wantedMode;
	private SweepTouchControl Sweeper;
	private Globe globe;
	private Image BlackFader;
	private float targetFadeValue = 1.0f;
	private bool bFading = false;
	private bool bDoneFade = true;
	private bool bWaiting = false;
	private bool bPaused = false;
	private int waitingLevel = 0;
	private bool bFindOptions = false;

	public void SetGraphicsQuality(int setting)
	{
		QualitySettings.SetQualityLevel(setting);
	}


    void Start()
    {
		if (deathScreen != null)
		{
			deathScreen.gameObject.SetActive(false);
		}

		if (pauseScreen != null)
		{
			pauseScreen.gameObject.SetActive(false);
		}
		else
		{
			pauseScreen = GameObject.FindGameObjectWithTag("Pause");
			if (pauseScreen != null)
			{
				pauseScreen.gameObject.SetActive(false);
			}
		}

		if (optionsScreen != null)
		{
			optionsScreen.gameObject.SetActive(false);
		}
		else
		{
			optionsScreen = GameObject.FindGameObjectWithTag("Options");
			if (optionsScreen != null)
			{
				optionsScreen.gameObject.SetActive(false);
			}
		}

		BlackFader = fadeBlackScreen.GetComponent<Image>();
		if (BlackFader != null)
		{
			fadeBlackScreen.gameObject.SetActive(true);
			SetFade(false);
		}

		ScreenX = Screen.width;
		ScreenY = Screen.height;
		Sweeper = FindObjectOfType<SweepTouchControl>();
		globe = FindObjectOfType<Globe>();

		InitGlobe();
	}

	void Update()
	{
		if (bFading && (Time.timeSinceLevelLoad > 0.2f))
		{
			UpdateFade();
		}

		if (bWaiting)
		{
			if (bDoneFade)
			{
				bWaiting = false;
				SceneManager.LoadScene(waitingLevel);
			}
		}

		// Pause
		if (Input.GetButtonDown("Cancel"))
		{
			// Return to pause from Options
			if ((optionsScreen != null) && optionsScreen.activeInHierarchy)
			{
				ExitOptions();
			}
			else
			{
				SetPaused(true);
			}
		}
	}


	public void ReturnToGame()
	{
		SetPaused(false);
	}


	public void SetPaused(bool value)
	{
		bPaused = value;
		Cursor.visible = value;

		if (bPaused)
		{
			Time.timeScale = 0f;
		}
		else
		{
			Time.timeScale = 1f;
		}

		if (pauseScreen != null)
		{
			pauseScreen.gameObject.SetActive(value);
		}
		else
		{
			pauseScreen = GameObject.FindGameObjectWithTag("Pause");
			if (pauseScreen != null)
			{
				pauseScreen.gameObject.SetActive(value);
			}
		}
	}

	public void EnterOptions()
	{
		if (optionsScreen != null)
		{
			optionsScreen.gameObject.SetActive(true);
			if (pauseScreen != null)
			{
				pauseScreen.SetActive(false);
			}
		}
		else
		{
			bFindOptions = true;
		}
	}

	public void ExitOptions()
	{
		if (optionsScreen != null)
		{
			optionsScreen.gameObject.SetActive(false);
			if (pauseScreen != null)
			{
				pauseScreen.SetActive(true);
			}
		}
	}
	


	public void GoToLevel(int levelID)
	{
		SetFade(true);

		if (bDoneFade)
		{
			Cursor.lockState = CursorLockMode.Confined;
			// Hide cursor when locking
			Cursor.visible = false;

			SceneManager.LoadScene(levelID);
		}
		else
		{
			bWaiting = true;
			waitingLevel = levelID;
		}
	}


	public void PlayerDied()
	{
		deathScreen.gameObject.SetActive(true);

		Cursor.lockState = CursorLockMode.None;
		// Hide cursor when locking
		Cursor.visible = true;
	}


	void UpdateFade()
	{
		if (BlackFader != null)
		{
			Color newColor = Color.white;
			newColor.a = Mathf.Lerp(BlackFader.color.a, targetFadeValue, fadeSpeed * Time.smoothDeltaTime);
			BlackFader.color = newColor;

			if (targetFadeValue == 0f)
			{
				if (BlackFader.color.a == targetFadeValue)
				{
					bFading = false;
					bDoneFade = true;
				}
			}
			else if (targetFadeValue == 1f)
			{
				if (BlackFader.color.a >= 0.99f) ///== targetFadeValue)
				{
					bFading = false;
					bDoneFade = true;
				}
			}
		}
	}


	void SetFade(bool value)
	{
		bDoneFade = false;
		bFading = true;

		if (value)
		{
			targetFadeValue = 1f;
		}
		else
		{
			targetFadeValue = 0f;
		}
	}

	
	public void InitGlobe()
	{
		ScreenX = Screen.width;
		ScreenY = Screen.height;
		Sweeper = FindObjectOfType<SweepTouchControl>();
		globe = FindObjectOfType<Globe>();

		if (globe != null)
		{
			// Home city
			SphereCollider globeCollider = globe.GetComponentInChildren<SphereCollider>();
			Vector3 pointOnSphere = globeCollider.ClosestPoint(Camera.main.transform.position + Camera.main.transform.forward * 5.0f);
			Transform newCity = Instantiate(cityPrefab, pointOnSphere, Quaternion.identity);
			newCity.parent = globeCollider.transform;
		}
	}


	public void GameBeginTurn()
	{
		//
	}

	public void GameEndTurn()
	{
		//
	}


	public void WinGame(bool value)
	{
		//
	}


	public void ResetLevel()
	{
		Scene scene = SceneManager.GetActiveScene();
		SceneManager.LoadScene(scene.name);
	}

	public void ExitToMenu()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(0);
	}



	public void ExitGame()
	{
		Application.Quit();
	}


	public void UpdateScore(float score, float touchNumber)
	{
		//
	}



}
