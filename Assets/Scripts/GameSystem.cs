using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSystem : MonoBehaviour
{
	public Transform fadeBlackScreen;
	public Transform cityPrefab;

	private int ScreenX;
	private int ScreenY;

	private CursorLockMode wantedMode;
	private SweepTouchControl Sweeper;
	private Globe globe;
	private Image BlackFader;
	private float targetFadeValue = 1.0f;
	private bool bFading = false;
	private bool bDoneFade = true;
	private bool bWaiting = false;
	private int waitingLevel = 0;


    void Start()
    {
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

		InitGame();

		Cursor.lockState = CursorLockMode.Confined;
		// Hide cursor when locking
		Cursor.visible = false;
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
	}

	public void GoToLevel(int levelID)
	{
		SetFade(true);

		if (bDoneFade)
		{
			SceneManager.LoadScene(levelID);
		}
		else
		{
			bWaiting = true;
			waitingLevel = levelID;
		}
	}

	void UpdateFade()
	{
		if (BlackFader != null)
		{
			Color newColor = Color.white;
			newColor.a = Mathf.Lerp(BlackFader.color.a, targetFadeValue, Time.deltaTime);
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
				if (BlackFader.color.a >= 0.95f) ///== targetFadeValue)
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

	
	public void InitGame()
	{
		// Home city
		SphereCollider globeCollider = globe.GetComponentInChildren<SphereCollider>();
		Vector3 pointOnSphere = globeCollider.ClosestPoint(Camera.main.transform.position + Camera.main.transform.forward * 5.0f);
		Transform newCity = Instantiate(cityPrefab, pointOnSphere, Quaternion.identity);
		newCity.parent = globeCollider.transform;


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
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
