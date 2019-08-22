using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class SweepTouchControl : MonoBehaviour
{
	public Vector3 StartPosition;
	public bool enter = true;
	public bool stay = false;
	public bool exit = true;
	public float raycastsPerSecond = 10.0f;
	public float explosiveTouchForce = 10.0f;
	public bool bSphereCast = false;
	public float sphereRadius = 1.0f;

	private Rigidbody2D rb;
	private SpriteRenderer sprite;
	private GameSystem game;
	private ToolBox toolbox;
	private Touch touch;
	private Globe globe;

	private Vector3 currentTouchPosition;
	private Vector3 deltaTouch;
	private float raycastRate = 0.0f;


	/// 0: single-tile eraser .. 1: multi-tile eraser
	public void SetSphereMode(bool value)
	{
		bSphereCast = value;
	}
	

	void Start()
    {
		rb = GetComponent<Rigidbody2D>();
		sprite = GetComponent<SpriteRenderer>();

		game = FindObjectOfType<GameSystem>();
		toolbox = FindObjectOfType<ToolBox>();
		globe = FindObjectOfType<Globe>();

		raycastRate = (1.0f / raycastsPerSecond);
    }

    
    void Update()
    {
		if (Input.touchCount > 0)
		{
			UpdateSweepTouch();
		}
	}


	void UpdateSweepTouch()
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


	// Raycast to field
	bool RaycastFromCameraTo(Vector3 rayTarget, GameObject intendedObject)
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
