using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSystem : MonoBehaviour
{
	public Transform startPoint;
	public Transform playerPrefab;
	public Transform player;
	public Transform[] playerObjects;
	public float fadeSpeed = 3f;
	public Transform cityPrefab;
	public GameObject deathScreen;
	public GameObject pauseScreen;
	public GameObject optionsScreen;
	public GameObject fadeBlackScreen;
	public GameObject loadingPanel;
	public GameObject[] yieldToLoadingPanel;
	public GameObject sunLight;

	private int ScreenX;
	private int ScreenY;

	GameObject center;
	GameObject[] allGameobjects;

	private CursorLockMode cursorMode;
	private SweepTouchControl Sweeper;
	private Globe globe;
	private Image BlackFader;
	private float targetFadeValue = 1.0f;
	private bool bFading = false;
	private bool bDoneFade = true;
	private bool bWaiting = false;
	private bool bPaused = false;
	private int waitingLevel = 0;
	private bool bSpawningPlayer = false;

	public void SetGraphicsQuality(int setting)
	{
		QualitySettings.SetQualityLevel(setting);
	}

    void Start()
    {
		Application.targetFrameRate = Screen.currentResolution.refreshRate;

		// Filling in for player while terrain loads
		TerrainControllerSimple terrain = FindObjectOfType<TerrainControllerSimple>();
		if (terrain != null)
		{
			terrain.SetPlayer(startPoint);
		}

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

		if (fadeBlackScreen != null)
		{
			BlackFader = fadeBlackScreen.GetComponent<Image>();
			if (BlackFader != null)
			{
				fadeBlackScreen.gameObject.SetActive(true);
				SetFade(false);
			}
		}

		ScreenX = Screen.width;
		ScreenY = Screen.height;
		Sweeper = FindObjectOfType<SweepTouchControl>();
		globe = FindObjectOfType<Globe>();

		InitGlobe();

		//if (playerPrefab != null)
		//{
		//	bSpawningPlayer = true;
		//	SetStartPosition();
		//}

		if (loadingPanel != null)
			loadingPanel.SetActive(false);
	}

	void Update()
	{
		//if (bSpawningPlayer && (player == null))
		//{
		//	SetStartPosition();
		//}

		if (bFading && (Time.timeSinceLevelLoad > 0.2f))
		{
			UpdateFade();
		}

		if (bWaiting)
		{
			if (bDoneFade)
			{
				bWaiting = false;

				loadingPanel.SetActive(true);
				foreach (GameObject go in yieldToLoadingPanel)
				{
					go.SetActive(false);
				}

				SceneManager.LoadScene(waitingLevel);
			}
		}

		// Pause
		if (Input.GetButtonDown("Cancel"))
		{
			PlayerMenus liveMenu = FindObjectOfType<PlayerMenus>();
			if (liveMenu != null)
			{
				if ((optionsScreen != null) && optionsScreen.activeInHierarchy)
				{
					liveMenu.ExitOptions();
					//ExitOptions();
				}
				else
				{
					liveMenu.EnterPause();
					//SetPaused(true);
				}
			}
			
		}

		// Environmental update
		if (sunLight != null && player != null)
		{
			sunLight.transform.position = player.transform.position + Vector3.up * 100f;
		}

		if (RenderSettings.skybox != null)
		{
			RenderSettings.skybox.SetFloat("_Rotation", Time.time * 0.16f);
		}
	}

	public void SetStartPosition()
	{
		if (startPoint != null)
		{
			RaycastHit[] hits;
			bool bFoundLandingSpot = false;
			Vector3 toNewPosition = Vector3.zero;
			Vector3 rayOrigin = startPoint.position + (Vector3.up * 50000f);
			Vector3 rayDirection = Vector3.down * 80000f;
			hits = Physics.RaycastAll(rayOrigin, rayDirection, 80000f);
			if (hits.Length > 0)
			{
				while (!bFoundLandingSpot)
				{
					Vector3 newRandomSample = Random.insideUnitSphere * 1000f;
					hits = Physics.RaycastAll(rayOrigin + newRandomSample, rayDirection, 80000f);
					for (int i = 0; i < hits.Length; i++)
					{
						if ((Vector3.Dot(Vector3.up, hits[i].normal) > 0.95f)
							&& hits[i].transform.gameObject.CompareTag("Land"))
						{
							bFoundLandingSpot = true;
						}
					}
				}

				int numHits = hits.Length;
				for (int i = 0; i < numHits; i++)
				{
					// Player position
					RaycastHit hit = hits[i];
					Vector3 newPlayerPosition = hit.point + (Vector3.up * 2f);
					Transform spawnedPlayer = Instantiate(playerPrefab, newPlayerPosition, Quaternion.identity);
					player = spawnedPlayer;
					bSpawningPlayer = false;

					// Hook up systems to player
					TerrainControllerSimple terrain = FindObjectOfType<TerrainControllerSimple>();
					terrain.SetPlayer(player);
					ObjectSpawner spawner = FindObjectOfType<ObjectSpawner>();
					spawner.SetPlayer(player);
					SmoothMouseLook cam = FindObjectOfType<SmoothMouseLook>();
					cam.body = player;
					MiniMap miniMap = FindObjectOfType<MiniMap>();
					miniMap.SetLookObject(player);
					Character playerCharacter = player.GetComponent<Character>();
					//pauseScreen = playerCharacter.pauseScreen;
					//optionsScreen = playerCharacter.optionsScreen;
					//deathScreen = playerCharacter.deathScreen;
					//fadeBlackScreen = playerCharacter.fadeBlackScreen;
					//loadingPanel = playerCharacter.loadingPanel;

					// Spawn player's objects ie. Vehicle
					int numObjs = playerObjects.Length;
					if (numObjs > 0)
					{
						Vector3 offset = player.forward * 50f * (i + 1);
						Vector3 spawnOffset = (Vector3.forward * 10f) + (Vector3.right * Random.Range(-6f, 6f));
						Vector3 rayStart = newPlayerPosition;
						RaycastHit rayHit;
						if (Physics.Raycast(rayStart, offset, out rayHit)){
							spawnOffset = Vector3.ClampMagnitude(rayHit.point - newPlayerPosition, Random.Range(10f, offset.magnitude));	
						}
						Vector3 rotationVector = Random.onUnitSphere;
						rotationVector.y = 0f;
						Quaternion spawnRotation = Quaternion.Euler(rotationVector);
						Transform newObj = Instantiate(playerObjects[i], newPlayerPosition + spawnOffset + Vector3.up, spawnRotation);
						newObj.gameObject.SetActive(true);
					}

					if (player != null)
					{
						break;
					}
				}
			}
		}
		else
		{
			bSpawningPlayer = false;
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
			Cursor.lockState = CursorLockMode.None;
		}
		else
		{
			Time.timeScale = 1f;
			Cursor.lockState = CursorLockMode.Locked;
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

			loadingPanel.SetActive(true);
			foreach (GameObject go in yieldToLoadingPanel)
			{
				go.SetActive(false);
			}

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
				if (BlackFader.color.a >= 0.999f) ///== targetFadeValue)
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
		Debug.ClearDeveloperConsole();

		ReturnToGame();
		loadingPanel.SetActive(true);
		Time.timeScale = 0;
		foreach (GameObject go in yieldToLoadingPanel)
		{
			go.SetActive(false);
		}

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
