using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSystem : MonoBehaviour
{
	public Transform fadeBlackScreen;

	private int ScreenX;
	private int ScreenY;

	private SweepTouchControl Sweeper;
	private Image blackFader;
	private float targetFadeValue = 1.0f;
	private bool bFading = false;


    // Core functions
    void Start()
    {
		Application.targetFrameRate = 30;
		ScreenX = Screen.width;
		ScreenY = Screen.height;

		Sweeper = FindObjectOfType<SweepTouchControl>();
		blackFader = fadeBlackScreen.GetComponent<Image>();
		if (blackFader != null)
		{
			fadeBlackScreen.gameObject.SetActive(true);
			SetFade(false);
		}
	}

	void Update()
	{
		if (bFading)
		{
			UpdateFade();
		}
	}

	void UpdateFade()
	{
		if (blackFader != null)
		{
			Color newColor = Color.white;
			newColor.a = Mathf.Lerp(blackFader.color.a, targetFadeValue, Time.deltaTime);
			blackFader.color = newColor;

			if (blackFader.color.a == targetFadeValue)
			{
				bFading = false;
			}
		}
	}

	void SetFade(bool value)
	{
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
		CameraController camControl = Camera.main.gameObject.GetComponent<CameraController>();
		if (camControl != null)
		{
			camControl.ResetCamera();
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
