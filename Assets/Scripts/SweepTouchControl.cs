using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class SweepTouchControl : MonoBehaviour
{
	private Rigidbody2D rb;
	private SpriteRenderer sprite;
	private GameSystem game;
	private Touch touch;
	private Globe globe;

	private Vector3 currentTouchPosition;
	private Vector3 deltaTouch;
	private Vector3 deltaMouse;
	private Vector3 lastMousePosition;
	

	void Start()
    {
		rb = GetComponent<Rigidbody2D>();
		sprite = GetComponent<SpriteRenderer>();

		game = FindObjectOfType<GameSystem>();
		globe = FindObjectOfType<Globe>();
    }

    
    void Update()
    {
		UpdateSweepTouch();
		UpdateClick();
	}

	void UpdateClick()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit rayHit;
			if (Physics.Raycast(ray, out rayHit, 100.0f))
			{
				City cityHit = rayHit.transform.GetComponentInParent<City>();
				if (cityHit != null)
				{
					cityHit.ActivateCity();
				}
			}
		}
	}


	void UpdateSweepTouch()
	{
		// Mouse control
		if (Input.GetMouseButtonDown(0))
		{
			lastMousePosition = Input.mousePosition;
		}

		if (Input.GetMouseButton(0))
		{
			globe.ToggleSpinning(true);

			deltaMouse = Input.mousePosition - lastMousePosition;
			lastMousePosition = Input.mousePosition;

			globe.RotateGlobe(deltaMouse);
		}

		if (Input.GetMouseButtonUp(0))
		{
			lastMousePosition = Input.mousePosition;
			globe.RotateGlobe(deltaMouse);
			globe.ToggleSpinning(false);
		}


		// Touch Control
		if (Input.touches.Length > 0)
		{
			touch = Input.GetTouch(0);
			///currentTouchPosition = Camera.main.ScreenToWorldPoint(touch.position);

			if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
			{
				switch (touch.phase)
				{
					case TouchPhase.Began:
						globe.ToggleSpinning(true);
						break;

					case TouchPhase.Moved:
						deltaTouch = touch.deltaPosition;
						globe.RotateGlobe(deltaTouch);
						break;

					case TouchPhase.Ended:
						globe.RotateGlobe(deltaTouch);
						globe.ToggleSpinning(false);

						break;
				}
			}
		}
	}


	// Raycast to field
	public bool RaycastFromCameraTo(Vector3 rayTarget, GameObject intendedObject)
	{
		bool result = false;
		RaycastHit[] hits;
		Vector3 start = Camera.main.transform.position;
		Vector3 direction = (rayTarget - start);

		hits = Physics.RaycastAll(start, direction, 25.0f);
		int numHits = hits.Length;
		if (numHits > 0)
		{
			for (int i = 0; i < numHits; i++)
			{
				if (hits[i].transform.gameObject == intendedObject)
				{
					result = true;
				}
			}
		}

		return result;
	}


}
