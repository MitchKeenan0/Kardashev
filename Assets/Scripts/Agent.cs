using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
	public float lookSpeed = 10f;
	public float visionConeAngle = 90f;
	public float reactionTime = 0.3f;

	private Character myCharacter;
	private Character playerCharacter;
	private Transform targetTransform;
	private Transform headComponent;
	private Transform bodyComponent;
	private Vector3 targetPosition = Vector3.zero;
	private Vector3 movePosition = Vector3.zero;
	private Vector3 aimPosition = Vector3.zero;
	private Vector3 headAimVector = Vector3.zero;
	private Vector3 bodyAimVector = Vector3.zero;

	private bool bMoving = false;

	private IEnumerator moveLocatorCoroutine;
	private IEnumerator aimLocatorCoroutine;

    void Start()
    {
		myCharacter = GetComponent<Character>();
		playerCharacter = FindObjectOfType<PlayerInput>().GetComponent<Character>();
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

		if (targetTransform != null)
			AimTo(targetTransform.position);
		else if (aimPosition != Vector3.zero)
			AimTo(aimPosition);
	}

	void MoveTo(Vector3 worldPosition)
	{
		// Body rotation
		if (bodyComponent != null)
		{
			bodyAimVector = Vector3.Lerp(bodyAimVector, worldPosition, Time.smoothDeltaTime * lookSpeed);
			bodyAimVector.y = transform.position.y;
			bodyComponent.LookAt(bodyAimVector);
		}

		Vector3 toTarget = worldPosition - transform.position;
		toTarget.y = 0f;
		Vector3 toTargetNorm = toTarget.normalized;
		float forwardDot = Vector3.Dot(bodyComponent.forward, toTargetNorm);
		float forwardInput = Mathf.Clamp(forwardDot * 100f, -1f, 1f);
		myCharacter.SetForward(forwardInput);

		// Destination check
		if (toTarget.magnitude <= 5f)
		{
			movePosition = Vector3.zero;
			myCharacter.SetForward(0f);
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

	void AimTo(Vector3 worldPosition)
	{
		// Head rotation
		if (headComponent != null)
		{
			headAimVector = Vector3.Lerp(headAimVector, worldPosition, Time.smoothDeltaTime * lookSpeed);
			// add some eye height
			headAimVector.y += 0.1f;
			headComponent.LookAt(headAimVector);

			// Clamp head-angle to body
			float coneAngleHalf = visionConeAngle * 0.5f;
			float bodyAngleMin = bodyComponent.eulerAngles.y - coneAngleHalf;
			float bodyAngleMax = bodyComponent.eulerAngles.y + coneAngleHalf;
			float headEulerY = headComponent.eulerAngles.y;
			headEulerY = ClampAngle(headEulerY, bodyAngleMin, bodyAngleMax);
			headComponent.eulerAngles = new Vector3(headComponent.eulerAngles.x, headEulerY, headComponent.eulerAngles.z);
		}

		// Vision check
		Vector3 toPlayer = playerCharacter.transform.position - headComponent.position;
		float angle = Vector3.Angle(headComponent.forward, toPlayer);
		if (angle <= visionConeAngle)
		{
			if (targetTransform == null)
			{
				targetTransform = playerCharacter.transform;
			}
		}
		else if (targetTransform != null)
		{
			targetTransform = null;
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
}
