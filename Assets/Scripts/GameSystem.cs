using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSystem : MonoBehaviour
{
	public float populationDelay = 2.0f;
	public float populationSize = 3.0f;
	public float levelDifficulty = 0.05f;

	public Transform personPrefab;
	public Transform enemyPrefab;
	public Transform congratulation;
	public Transform nextButton;

	private int ScreenX;
	private int ScreenY;

	//private Text ScoreText;
	private Text TouchCountText;
	private Text PileScoreText;

	private SweepTouchControl Sweeper;
	private ScoreAnimation ScoreAnim;

	private List<HexPanel> panels;

	private bool bGameOn = false;


	public void SetPlayerSingleTileMode()
	{
		Sweeper.SetSphereMode(false);
	}

	public void SetPlayerMultiTileMode()
	{
		Sweeper.SetSphereMode(true);

		Debug.Log("SphereMode");
	}


	public void NewGravity(Vector3 position)
	{
		foreach (HexPanel p in panels)
		{
			p.SetGravityPosition(position);
		}
	}

    
    void Start()
    {
		Application.targetFrameRate = 30;

		if (nextButton != null)
		{
			nextButton.gameObject.SetActive(false);
		}

		ScreenX = Screen.width;
		ScreenY = Screen.height;

		//ScoreText = FindObjectOfType<Text>();
		Sweeper = FindObjectOfType<SweepTouchControl>();
	}


	public void InitGame()
	{
		CameraController camControl = Camera.main.gameObject.GetComponent<CameraController>();
		if (camControl != null)
		{
			camControl.ResetCamera();
		}

		ResetScores();


		// Store tiles
		panels = new List<HexPanel>();
		HexPanel[] panelArray = FindObjectsOfType<HexPanel>();
		panels.AddRange(panelArray);

		// Initial physics state
		HexPanel[] hexes = FindObjectsOfType<HexPanel>();
		int numHexes = hexes.Length;
		for (int i = 0; i < numHexes; i++)
		{
			HexPanel hexi = hexes[i];
			if ((hexi != null) && !hexi.IsFrozen())
			{
				hexi.SetPhysical(true);
			}
		}

		StartCoroutine(InitPopulation(numHexes, levelDifficulty));

		// Start connection system
		PeopleConnection connection = FindObjectOfType<PeopleConnection>();
		if (connection != null)
		{
			connection.SyncStart();
		}
	}


	void PopulateLevel(int hexCount, float difficulty)
	{
		int i = 0;
		foreach (HexPanel pan in panels)
		{
			if (i < hexCount)
			{
				i++;

				Transform panTransform = pan.gameObject.transform;

				// Person
				if (Random.Range(0.0f, 1.0f) <= ((0.33f * difficulty) * populationSize))
				{
					float distToCentre = Vector3.Distance(Vector3.zero, panTransform.position);
					if (distToCentre >= 1.0f)
					{
						PopulateTile(panTransform);
					}
				}

				// Enemy
				else if (Random.Range(0.0f, 1.0f) <= (levelDifficulty * 0.5f))
				{
					float distToCentre = Vector3.Distance(Vector3.zero, panTransform.position);
					if (distToCentre > 1.0f)
					{
						SpawnEnemy(panTransform);
					}
				}
			}
			else
			{
				break;
			}
		}
	}


	void PopulateTile(Transform hex)
	{
		Transform newPerson = Instantiate(personPrefab, hex.position, Quaternion.identity);
		newPerson.transform.SetParent(hex);

		HexPanel newHex = hex.GetComponent<HexPanel>();
		newHex.SetPopulated(true);

		newHex.gameObject.GetComponent<Rigidbody>().mass *= 3.0f;
		newHex.fallForce *= 9.0f;
	}


	void SpawnEnemy(Transform hex)
	{
		Transform newEnemy = Instantiate(enemyPrefab, hex.position, Quaternion.identity);
		newEnemy.transform.SetParent(hex);

		//HexPanel newHex = hex.GetComponent<HexPanel>();
		//newHex.SetPopulated(true);
	}


	IEnumerator InitPopulation(int hexCount, float difficulty)
	{
		yield return new WaitForSeconds(populationDelay);

		PopulateLevel(hexCount, difficulty);
	}


	public void GameBeginTurn()
	{
		
	}

	public void GameEndTurn()
	{
		if (panels != null)
		{
			int numHexes = panels.Count;
			if (numHexes > 0)
			{
				for (int i = 0; i < numHexes; i++)
				{
					HexPanel hex = panels[i];
					if (hex != null)
					{
						hex.SetMovedThisTurn(false);
					}
				}
			}
		}

		if (!bGameOn)
		{
			HexCharacter[] characters = FindObjectsOfType<HexCharacter>();
			int numChars = characters.Length;
			if (numChars > 0)
			{
				for (int i = 0; i < numChars; i++)
				{
					HexCharacter chara = characters[i];
					chara.SetCharacterEnabled(true);
				}
			}
			bGameOn = true;
		}
	}


	public void WinGame()
	{
		if (congratulation != null)
		{
			congratulation.gameObject.SetActive(true);
		}

		if (nextButton != null)
		{
			nextButton.gameObject.SetActive(true);
		}
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
		/// Inform score's animation
		if (ScoreAnim != null)
		{
			ScoreAnim.NewScore(score);
		}

		/// Update touch touch
		if (TouchCountText != null)
		{
			TouchCountText.text = ("Touch: " + touchNumber);
		}
	}


	void InitScoring()
	{
		var textArray = FindObjectsOfType<Text>();
		int numTexts = textArray.Length;
		if (numTexts > 0)
		{

			/// Touch count
			for (int i = 0; i < numTexts; i++)
			{
				Text ThisText = textArray[i];
				if (ThisText.tag == "TouchCount")
				{
					TouchCountText = ThisText;
					TouchCountText.text = "Touch: 0";
				}
			}

			/// Pile score
			for (int i = 0; i < numTexts; i++)
			{
				Text ThisText = textArray[i];
				if (ThisText.tag == "Score")
				{
					PileScoreText = ThisText;
					PileScoreText.text = "0";
				}
			}

			/// Animation system
			if (PileScoreText != null)
			{
				ScoreAnim = PileScoreText.gameObject.GetComponent<ScoreAnimation>();
			}

		}
	}


	void ResetScores()
	{
		if (TouchCountText != null)
		{
			TouchCountText.text = ("Touch: 0");
		}

		if (PileScoreText != null)
		{
			PileScoreText.text = "0";
		}

		if (ScoreAnim != null)
		{
			ScoreAnim.NewScore(0.0f);
		}

		if (Sweeper != null)
		{
			Sweeper.ResetSweeps();
		}
	}



}
