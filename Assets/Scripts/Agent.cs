using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
	public float lookSpeed = 20f;
	public float reactionTime = 0.3f;

	private Character myCharacter;
	private Transform targetTransform;
	private Transform headComponent;
	private Transform bodyComponent;
	private Vector3 targetPosition = Vector3.zero;
	private Vector3 movePosition = Vector3.zero;
	private Vector3 aimPosition = Vector3.zero;
	private Vector3 aimVector = Vector3.zero;

	private bool bMoving = false;

	private IEnumerator moveLocatorCoroutine;
	private IEnumerator aimLocatorCoroutine;

    void Start()
    {
		myCharacter = GetComponent<Character>();
		//targetTransform = FindObjectOfType<PlayerInput>().transform;
		headComponent = myCharacter.head;
		bodyComponent = myCharacter.body;
		myCharacter.SetBotControl(true);

		moveLocatorCoroutine = MoveLocatorDelay(2f);
		StartCoroutine(moveLocatorCoroutine);
		aimLocatorCoroutine = AimLocatorDelay(1f);
		StartCoroutine(aimLocatorCoroutine);
    }
    
    void Update()
    {
		if (movePosition != Vector3.zero)
			MoveTo(movePosition);

		if (aimPosition != Vector3.zero)
			AimTo(aimPosition);
	}

	void MoveTo(Vector3 worldPosition)
	{
		Vector3 toTarget = worldPosition - transform.position;
		
		// Destination check
		if (toTarget.magnitude <= 1f)
		{
			movePosition = Vector3.zero;
			myCharacter.SetForward(0f);
			return;
		}
		
		// Else we move it move it
		Vector3 toTargetNorm = toTarget.normalized;
		float forwardDot = Vector3.Dot(bodyComponent.forward, toTargetNorm);
		float forwardInput = Mathf.Clamp(forwardDot * 100f, -1f, 1f);
		myCharacter.SetForward(forwardInput);
	}

	void AimTo(Vector3 worldPosition)
	{
		aimVector = Vector3.Lerp(aimVector, worldPosition, Time.smoothDeltaTime * lookSpeed);
		// Add some eye height
		aimVector.y += 0.1f;
		
		// Body rotation
		if (bodyComponent != null)
		{
			Vector3 flatRotation = aimVector;
			flatRotation.y = transform.position.y;
			bodyComponent.LookAt(flatRotation);
		}
		
		// Head rotation
		if (headComponent != null)
		{
			headComponent.LookAt(aimVector);
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
			movePosition = transform.position + Random.insideUnitSphere * 10f;
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
			aimPosition = headComponent.forward + Random.insideUnitSphere * 10f;
			aimPosition.y = headComponent.position.y;
		}
	}

	IEnumerator MoveLocatorDelay(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		DecideMovePosition();
	}

	IEnumerator AimLocatorDelay(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		DecideAimPosition();
	}
}
