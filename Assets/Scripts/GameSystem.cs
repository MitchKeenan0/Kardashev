using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSystem : MonoBehaviour
{
	private int ScreenX;
	private int ScreenY;

	//private Text ScoreText;
	private Text TouchCountText;
	private Text PileScoreText;

	private SweepTouchControl Sweeper;
	private ScoreAnimation ScoreAnim;

	private List<HexPanel> panels;

    
    void Start()
    {
		Application.targetFrameRate = 30;

		ScreenX = Screen.width;
		ScreenY = Screen.height;

		//ScoreText = FindObjectOfType<Text>();
		Sweeper = FindObjectOfType<SweepTouchControl>();

		StartCoroutine(InitBoard());

		//InitScoring();
	}

	IEnumerator InitBoard()
	{
		yield return new WaitForSeconds(0.3f);

		InitGame();
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

		HexGrid grid = FindObjectOfType<HexGrid>();
		if (grid != null)
		{
			grid.StartHexGrid();
		}

		HexPanel[] hexes = FindObjectsOfType<HexPanel>();
		int numHexes = hexes.Length;
		Debug.Log("numHexes: " + numHexes);
		for (int i = 0; i < numHexes; i++)
		{
			HexPanel hexi = hexes[i];
			if ((hexi != null) && !hexi.IsFrozen())
			{
				hexi.SetPhysical(true);
				Debug.Log("YUPP");
			}
		}

		// Set centre tile
		//RaycastHit hit;
		//Vector3 start = Camera.main.transform.position;
		//Vector3 centreScreen = Camera.main.transform.forward * 15.0f;
		//if (Physics.Raycast(start, centreScreen, out hit))
		//{
		//	HexPanel hex = hit.collider.gameObject.GetComponent<HexPanel>();
		//	if (hex != null)
		//	{
		//		hex.Freeze();
		//	}
		//}

		PeopleConnection connection = FindObjectOfType<PeopleConnection>();
		if (connection != null)
		{
			connection.SyncStart();
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
