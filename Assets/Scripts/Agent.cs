﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
	public float lookSpeed = 10f;
	public float visionConeAngle = 90f;
	public float reactionTime = 0.3f;
	public GameObject primaryToolPrefab;

	private Character myCharacter;
	private Character playerCharacter;
	private Transform targetTransform;
	private Transform headComponent;
	private Transform bodyComponent;
	private GameObject primaryTool;
	private Vision vision;
	private Vector3 targetPosition = Vector3.zero;
	private Vector3 movePosition = Vector3.zero;
	private Vector3 aimPosition = Vector3.zero;
	private Vector3 headAimVector = Vector3.zero;
	private Vector3 bodyAimVector = Vector3.zero;
	private float timeAtTriggerDown = 0f;
	private bool bMoving = false;
	private bool bTriggerDown = false;
	private bool bAlive = true;
	private IEnumerator moveLocatorCoroutine;
	private IEnumerator aimLocatorCoroutine;

	void Awake()
	{
		vision = GetComponent<Vision>();
	}

	void Start()
    {
		myCharacter = GetComponent<Character>();
		myCharacter.SetBotControl(true);
		aimPosition = transform.forward * 100f;

		moveLocatorCoroutine = MoveLocatorDelay(2f);
		StartCoroutine(moveLocatorCoroutine);
		aimLocatorCoroutine = AimLocatorDelay(1f);
		StartCoroutine(aimLocatorCoroutine);

		if (primaryToolPrefab != null)
		{
			primaryTool = Instantiate(primaryToolPrefab, myCharacter.toolArm.position, myCharacter.toolArm.rotation);
			myCharacter.EquipObject(primaryTool);
		}
	}
    
    void Update()
    {
		if (bAlive)
		{
			if (playerCharacter == null)
			{
				if (FindObjectOfType<PlayerInput>())
				{
					playerCharacter = FindObjectOfType<PlayerInput>().GetComponent<Character>();
					vision.SetVisionTarget(playerCharacter.transform);
				}
			}
			
			if (headComponent == null)
				headComponent = myCharacter.head;

			if (bodyComponent == null)
				bodyComponent = myCharacter.body;

			// Movement
			if (movePosition != Vector3.zero)
				MoveTo(movePosition);

			// Sight and Aim
			Vector3 aimingVector = aimPosition;
			if (vision != null)
			{
				bool bHasLineOfSight = vision.CheckLineOfSight(headComponent);
				if (bHasLineOfSight)
				{
					targetTransform = playerCharacter.transform;

					if (targetTransform != null)
					{
						UpdateToolTrigger();
						aimingVector = targetTransform.position;
					}
				}
			}

			AimTo(aimingVector);
		}
	}

	void MoveTo(Vector3 worldPosition)
	{
		UpdateBodyRotation(worldPosition);

		Vector3 toTarget = worldPosition - transform.position;
		toTarget.y = 0f;
		Vector3 toTargetNorm = toTarget.normalized;

		// Forward
		float forwardDot = Vector3.Dot(bodyComponent.forward, toTargetNorm);
		float forwardInput = Mathf.Clamp(forwardDot * 100f, -1f, 1f);
		myCharacter.SetForward(forwardInput);

		if (Random.Range(0f, 1f) > 0.9f)
			myCharacter.Jump();

		// Boost
		if (Random.Range(0f, 1f) >= 0.9f)
			myCharacter.Boost();

		// Destination check
		if (toTarget.magnitude <= 5f)
		{
			movePosition = Vector3.zero;
			myCharacter.SetForward(0f);
		}
	}

	void AimTo(Vector3 worldPosition)
	{
		if (headComponent != null)
		{
			headAimVector = Vector3.Lerp(headAimVector, worldPosition, Time.smoothDeltaTime * lookSpeed);
			headComponent.LookAt(headAimVector);

			// Clamp head-angle to body
			float coneAngleHalf = visionConeAngle * 0.5f;
			float bodyAngleMin = bodyComponent.eulerAngles.y - coneAngleHalf;
			float bodyAngleMax = bodyComponent.eulerAngles.y + coneAngleHalf;
			float headEulerY = headComponent.eulerAngles.y;
			headEulerY = ClampAngle(headEulerY, bodyAngleMin, bodyAngleMax);
			headComponent.eulerAngles = new Vector3(headComponent.eulerAngles.x, headEulerY, headComponent.eulerAngles.z);
		}
	}

	void UpdateBodyRotation(Vector3 worldPosition)
	{
		if (bodyComponent != null)
		{
			bodyAimVector = Vector3.Lerp(bodyAimVector, worldPosition, Time.smoothDeltaTime * lookSpeed);
			bodyAimVector.y = bodyComponent.position.y;
			bodyComponent.LookAt(bodyAimVector);
		}
	}

	void UpdateToolTrigger()
	{
		if (targetTransform != null)
		{
			if (!bTriggerDown)
			{
				// Trigger down
				myCharacter.PrimaryTrigger(true);
				timeAtTriggerDown = Time.time;
				bTriggerDown = true;
			}
			else if (Time.time - timeAtTriggerDown > (Random.Range(0.5f, 2f)))
			{
				float toolToTargetAngle = Vector3.Angle(primaryTool.transform.forward, targetTransform.position - primaryTool.transform.position);
				bool bConfidentToFire = toolToTargetAngle <= 3f;
				if (bConfidentToFire)
				{
					// Trigger up
					myCharacter.PrimaryTrigger(false);
					bTriggerDown = false;
				}
			}
		}
		else
		{
			// Tool dropped
			myCharacter.PrimaryTrigger(false);
			bTriggerDown = false;
		}
	}

	void DecideMovePosition()
	{
		if (targetTransform != null)
		{
			movePosition = targetTransform.position;
		}
		else
		{
			movePosition = transform.position 
				+ transform.forward 
				+ (Random.insideUnitSphere * 10f);
		}
	}

	void DecideAimPosition()
	{
		if (targetTransform != null)
		{
			aimPosition = targetTransform.position;
		}
		else
		{
			aimPosition = transform.position + (headComponent.forward + Random.insideUnitSphere * 10f);
			aimPosition.y = headComponent.position.y;
		}
	}

	public void SetAlive(bool value)
	{
		bAlive = value;
	}

	IEnumerator MoveLocatorDelay(float waitTime)
	{
		while (true)
		{
			yield return new WaitForSeconds(waitTime);
			DecideMovePosition();
		}
	}

	IEnumerator AimLocatorDelay(float waitTime)
	{
		while (true)
		{
			yield return new WaitForSeconds(waitTime);
			DecideAimPosition();
		}
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		angle = Mathf.Repeat(angle, 360);
		min = Mathf.Repeat(min, 360);
		max = Mathf.Repeat(max, 360);
		bool inverse = false;
		var tmin = min;
		var tangle = angle;
		if (min > 180)
		{
			inverse = !inverse;
			tmin -= 180;
		}
		if (angle > 180)
		{
			inverse = !inverse;
			tangle -= 180;
		}
		var result = !inverse ? tangle > tmin : tangle < tmin;
		if (!result)
			angle = min;

		inverse = false;
		tangle = angle;
		var tmax = max;
		if (angle > 180)
		{
			inverse = !inverse;
			tangle -= 180;
		}
		if (max > 180)
		{
			inverse = !inverse;
			tmax -= 180;
		}

		result = !inverse ? tangle < tmax : tangle > tmax;
		if (!result)
			angle = max;
		return angle;
	}
}
