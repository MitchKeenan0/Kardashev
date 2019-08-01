using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSystem : MonoBehaviour
{
	public float populationDelay = 2.0f;
	public float populationSize = 3.0f;
	public float citizenSize = 1.5f;
	public float tileSize = 1.5f;
	public float levelDifficulty = 0.05f;

	public Transform personPrefab;
	public Transform enemyPrefab;
	public Transform congratulation;
	public Transform condolence;
	public Transform nextButton;
	public Transform fadeBlackScreen;

	private int ScreenX;
	private int ScreenY;
	
	private Text TouchCountText;
	private Text PileScoreText;

	private SweepTouchControl Sweeper;
	private ScoreAnimation ScoreAnim;
	private ToolBox toolbox;
	private PeopleConnection connection;

	private List<HexPanel> panels;
	private List<Transform> people;
	private int hexMovers = 0;
	private float timeAtTurnStart = 0.0f;
	//private bool bGameOn = false;
	//private int debugCount = 0;
	private int moveNumber = 0;


	// Public functions
	public void UpdateHexMovers(int value)
	{
		hexMovers += value;

		if (hexMovers <= 0)
		{
			if ((Time.time - timeAtTurnStart) >= 0.5f)
			{
				GameEndTurn();
			}
		}

		//Debug.Log("hexMovers " + hexMovers + " @ " + debugCount + " " + Time.time);
		//debugCount += 1;
	}

	public void LosePanel(HexPanel pan)
	{
		panels.Remove(pan);
	}

	public void SetPlayerSingleTileMode()
	{
		Sweeper.SetSphereMode(false);
	}

	public void SetPlayerMultiTileMode()
	{
		Sweeper.SetSphereMode(true);
	}

	public void NewGravity(Vector3 position)
	{
		foreach (HexPanel p in panels)
		{
			p.SetGravityPosition(position);
		}
	}

    // Core functions
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
		toolbox = FindObjectOfType<ToolBox>();
		connection = FindObjectOfType<PeopleConnection>();
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

		people = new List<Transform>();

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

		//StartCoroutine(InitPopulation(numHexes, levelDifficulty));

		// Start connection system
		//PeopleConnection connection = FindObjectOfType<PeopleConnection>();
		//if (connection != null)
		//{
		//	connection.SyncStart();
		//}
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
				if (panTransform.GetComponent<SpriteRenderer>().isVisible)
				{
					// Person
					if (Random.Range(0.0f, 1.0f) <= (0.11f * populationSize))
					{
						float distToCentre = Vector3.Distance(Vector3.zero, panTransform.position);
						if (distToCentre >= 1.5f)
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

					else
					{
						panTransform.localScale *= tileSize;
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
		HexPanel newHex = hex.GetComponent<HexPanel>();
		if (!newHex.bEnemy)
		{
			newHex.SetPopulated(true);

			Transform newPerson = Instantiate(personPrefab, hex.position, Quaternion.identity);
			newPerson.transform.SetParent(hex);



			newHex.gameObject.GetComponent<Rigidbody>().mass *= 3.0f;
			newHex.fallForce *= 6.0f;

			hex.localScale *= citizenSize;

			people.Add(newPerson);

			//newHex.SetPhysical(false);
		}
	}


	void SpawnEnemy(Transform hex)
	{
		HexPanel newHex = hex.GetComponent<HexPanel>();
		if (!newHex.IsPopulated())
		{
			Transform newEnemy = Instantiate(enemyPrefab, hex.position, Quaternion.identity);
			newEnemy.transform.SetParent(hex);


			newHex.bEnemy = true;
			HexCharacter newCharacter = newEnemy.GetComponent<HexCharacter>();
			if (newCharacter != null)
			{
				newCharacter.currentHex = newHex;
			}

			hex.localScale *= citizenSize;
		}
	}


	IEnumerator InitPopulation(int hexCount, float difficulty)
	{
		yield return new WaitForSeconds(populationDelay);

		PopulateLevel(hexCount, difficulty);

		GameObject[] people = GameObject.FindGameObjectsWithTag("People");
		if (people.Length <= 5)
		{
			PopulateLevel(hexCount, difficulty);
		}
	}


	public void GameBeginTurn()
	{
		timeAtTurnStart = Time.time;
	}

	public void GameEndTurn()
	{
		// Reload player charges
		toolbox.ReloadSingleCharges();

		// Reset panels
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

		// Update enemies turn
		if (moveNumber > 0)
		{
			connection.UpdateConnection();

			HexCharacter[] characters = FindObjectsOfType<HexCharacter>();
			int numChars = characters.Length;
			if (numChars > 0)
			{
				for (int i = 0; i < numChars; i++)
				{
					HexCharacter chara = characters[i];
					chara.SetCharacterEnabled(true);
					chara.UpdateCharacter();
				}
			}

			//connection.WinCondition();
		}

		moveNumber += 1;
	}


	public void WinGame(bool value)
	{
		if (value)
		{
			if (congratulation != null)
			{
				congratulation.gameObject.SetActive(true);
			}
		}
		else
		{
			if (condolence != null)
			{
				condolence.gameObject.SetActive(true);
			}
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
