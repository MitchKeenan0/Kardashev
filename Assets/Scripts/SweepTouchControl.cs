﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SweepTouchControl : MonoBehaviour
{
	public Vector3 StartPosition;
	public bool enter = true;
	public bool stay = false;
	public bool exit = true;
	public float raycastsPerSecond = 10.0f;
	public bool bExplosiveTouch = false;
	public float explosiveTouchForce = 10.0f;
	public bool bSphereCast = false;
	public float sphereRadius = 1.0f;

	private Rigidbody2D rb;
	private SpriteRenderer sprite;
	private GameSystem game;
	private List<GameObject> touchedGameObjects;
	private Touch touch;

	private Vector3 currentTouchPosition;
	private int NumberOfTouches = 0;
	private float PileScore = 0;
	private float stayCount = 0.0f;
	private float raycastTimer = 0.0f;
	private float raycastRate = 0.0f;
	private bool bTouching = false;
	private bool bTouched = false;


	/// 0: single-tile eraser .. 1: multi-tile eraser
	public void SetSphereMode(bool value)
	{
		bSphereCast = value;
	}
	

	void Start()
    {
		rb = GetComponent<Rigidbody2D>();
		sprite = GetComponent<SpriteRenderer>();
		touchedGameObjects = new List<GameObject>();
		game = FindObjectOfType<GameSystem>();

		transform.position = Camera.main.ScreenToWorldPoint(StartPosition);

		raycastRate = (1.0f / raycastsPerSecond);
    }

    
    void Update()
    {
		if (Input.touchCount > 0)
		{
			UpdateSweepTouch();
		}
	}


	public void ResetSweeps()
	{
		NumberOfTouches = 0;

	}


	void UpdateSweepTouch()
	{
		touch = Input.GetTouch(0);
		currentTouchPosition = Camera.main.ScreenToWorldPoint(touch.position);
		currentTouchPosition.z = 0.0f;

		switch (touch.phase)
		{
			case TouchPhase.Began:
				bTouching = true;
				rb.MovePosition(currentTouchPosition);
				sprite.enabled = true;
				break;

			case TouchPhase.Moved:
				rb.MovePosition(currentTouchPosition);
				break;

			case TouchPhase.Ended:
				bTouched = false;
				bTouching = false;
				EndOfSweep();
				break;
		}

		//
		// Raycast to select tiles
		if ((!bTouched && bTouching) || (bSphereCast == false))
		{
			if (raycastTimer >= raycastRate)
			{
				RaycastFromCameraTo(currentTouchPosition);
				
				raycastTimer = 0.0f;
				
				if (bSphereCast)
				{
					bTouched = true;
				}
			}
			else
			{
				raycastTimer += Time.deltaTime;
			}
		}
	}


	void EndOfSweep()
	{
		if (!bTouching)
		{
			sprite.enabled = false;

			game.GameEndTurn();

			if (touchedGameObjects.Count > 0)
			{
				int numTouched = touchedGameObjects.Count;
				if (numTouched > 0)
				{

					// Delete all selected tiles
					for (int i = 0; i < numTouched; ++i)
					{
						if (touchedGameObjects[i] != null)
						{
							HexPanel hex = touchedGameObjects[i].GetComponent<HexPanel>();
							if (hex != null)
							{
								hex.LoseTouch();
							}
						}
					}
				}
			}
		}

		touchedGameObjects.Clear();
	}


	void SpherecastFromCameraTo(Vector3 target)
	{
		Collider[] rawNeighbors = Physics.OverlapSphere(target, sphereRadius);
		int numHits = rawNeighbors.Length;
		if (numHits > 0)
		{
			for (int i = 0; i < numHits; i++)
			{
				// Validate each tile..
				HexPanel hex = rawNeighbors[i].transform.gameObject.GetComponent<HexPanel>();
				if ((hex != null)
					&& (hex.gameObject != gameObject)
						&& !hex.IsPhysical())
				{
					if (!touchedGameObjects.Contains(hex.gameObject))
					{
						if (!hex.IsFrozen())
						{
							touchedGameObjects.Add(hex.transform.gameObject);
							hex.ReceiveTouch();
						}
					}
				}
			}
		}
	}


	// Raycast to field
	void RaycastFromCameraTo(Vector3 target)
	{
		RaycastHit[] hits;
		HexPanel firstHex = null;
		Vector3 start = Camera.main.transform.position;
		Vector3 direction = (target - start) * 1.5f;

		hits = Physics.RaycastAll(start, direction, 25.0f);
		int numHits = hits.Length;
		if (numHits > 0)
		{
			for (int i = 0; i < numHits; i++)
			{
				HexPanel hex = hits[i].collider.gameObject.GetComponent<HexPanel>();
				if (hex != null)
				{
					if (!touchedGameObjects.Contains(hex.gameObject))
					{
						if (!hex.IsFrozen() && !hex.IsPopulated())
						{
							if (i == 0)
							{
								firstHex = hex;
							}

							if (bExplosiveTouch)
							{
								Collider[] nearbyColliders = Physics.OverlapSphere(hex.transform.position, 2.0f);
								int numColliders = nearbyColliders.Length;
								if (numColliders > 0)
								{
									for (int j = 0; j < numColliders; j++)
									{
										Rigidbody rbrb = nearbyColliders[i].GetComponent<Rigidbody>();
										if (rbrb != null)
										{
											Vector3 explosionForce = (rbrb.position - hex.transform.position).normalized;
											rbrb.AddForce(explosionForce * explosiveTouchForce);
										}
									}
								}
								

								hex.LoseTouch();
							}
							else if (!touchedGameObjects.Contains(hex.gameObject))
							{
								touchedGameObjects.Add(hits[i].collider.gameObject);

								hex.ReceiveTouch();
							}
						}
					}
				}
			}

			// Sphere for large touch
			if (bSphereCast)
			{
				Vector3 firstHexPosition = firstHex.transform.position;
				SpherecastFromCameraTo(firstHexPosition);
			}
		}
	}


	// Collisions
	void OnTriggerEnter2D(Collider2D other)
	{
		if (enter)
		{
			HexPanel hex = other.gameObject.GetComponent<HexPanel>();
			if (hex != null)
			{
				touchedGameObjects.Add(other.gameObject);

				hex.ReceiveTouch();
			}
		}
	}

	void OnTriggerStay2D(Collider2D other)
	{
		if (stay)
		{
			if (stayCount > 0.25f)
			{
				Debug.Log("staying");
				stayCount = stayCount - 0.25f;
			}
			else
			{
				stayCount = stayCount + Time.deltaTime;
			}
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (exit)
		{

			///SweepDirts.Remove(other.gameObject);
		}
	}


	void ReadPile()
	{
		/// Finished the touch
		NumberOfTouches += 1;

		var dirtsArray = FindObjectsOfType<Dirt>();
		int numDirts = dirtsArray.Length;
		float superSum = 0.0f;
		float superAverage = 0.0f;

		/// For camera..
		float lowestAverage = 999999.9f;
		Vector3 coreLocation = Vector2.zero;

		if (numDirts > 0)
		{
			/// Get average of distance for all dirts...
			for (int i = 0; i < numDirts; i++)
			{
				if (dirtsArray[i] != null)
				{
					/// Each dirt...
					Dirt ThisDirt = dirtsArray[i];
					if (CheckBounds(ThisDirt.gameObject))
					{
						float personalSum = 0.0f;
						for (int j = 0; j < numDirts; j++)
						{
							if (dirtsArray[j] != null)
							{
								Dirt ThatDirt = dirtsArray[j];
								float distToDirt = Vector2.Distance(ThisDirt.transform.position, ThatDirt.transform.position);
								personalSum += distToDirt;
							}
						}

						/// Contribute to the average
						float personalAverage = personalSum / numDirts;
						superSum += personalAverage;

						/// Possibly nominate centerpoint for camera
						if (personalAverage < lowestAverage)
						{
							lowestAverage = personalAverage;
							coreLocation = ThisDirt.transform.position;
						}
					}

					/// Determines how many neighbors this dirt has
					ThisDirt.ProbeSurroundings();
				}
			}

			/// Inform camera for tigher angle
			//Debug.Log("lowest average: " + lowestAverage);
			CameraController CamControl = Camera.main.GetComponent<CameraController>();
			if (CamControl != null)
			{
				CamControl.NewParameters(lowestAverage, coreLocation);
			}


			/// Inverse of the combined average is our pile score
			superAverage = superSum / numDirts;
			PileScore = Mathf.Pow((1.0f / superAverage) * 10.0f, 3.0f);

			if (game != null)
			{
				game.UpdateScore(PileScore, NumberOfTouches);
			}
		}
	}


	bool CheckBounds(GameObject obj)
	{
		Vector3 ScreenPosition = Camera.main.WorldToScreenPoint(obj.transform.position);
		float MyX = ScreenPosition.x;
		float MyY = ScreenPosition.y;
		float ScreenX = Screen.width;
		float ScreenY = Screen.height;
		bool bOnscreen = true;

		if ((MyX <= 0.0f) || (MyY <= 0.0f) || (MyX >= ScreenX) || (MyY >= ScreenY))
		{
			bOnscreen = false;
			Rigidbody2D dirtRb = obj.GetComponent<Rigidbody2D>();
			if (dirtRb != null)
			{
				Vector2 reflection = dirtRb.velocity * -1.0f;
				dirtRb.velocity = reflection;
			}
		}

		return bOnscreen;
	}


}
