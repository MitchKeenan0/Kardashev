using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : MonoBehaviour
{
	public float gravity = 1f;
	public float damage = 10f;
	public Transform impactParticles;
	public Transform damageParticles;
	public bool bDespawn = false;
	public float despawnTime = 1f;
	public float raycastDistance = 1.1f;
	public Vector3 tipPosition;

	private Rigidbody rb;
	private ThrowingTool tool;
	private ToolRecovery recovery;
	private bool bStruck = false;
	private bool bDone = false;
	private float despawnTimer = 0f;
	private Vector3 lastPosition;

	public void InitSpear(ThrowingTool owningTool)
	{
		tool = owningTool;
	}

	public void RecoverSpear()
	{
		tool.reserveAmmo += tool.throwCost;
		tool.GetHudInfo().SetToolReserve(tool.reserveAmmo.ToString());
		Destroy(gameObject);

		if (tool.reserveAmmo == 1)
		{
			tool.RecoverMockFast();
		}
	}


    void Start()
    {
		rb = GetComponent<Rigidbody>();
		recovery = GetComponentInChildren<ToolRecovery>();
		recovery.SetColliderActive(false);
		lastPosition = transform.position;
    }

    
    void Update()
    {
		if (!bStruck)
		{
			Vector3 deltaV = rb.velocity * 2f;
			if (deltaV.magnitude > 0f)
			{
				// Rotation to velocity
				Quaternion rotation = Quaternion.LookRotation(deltaV, Vector3.up);
				transform.rotation = rotation;

				// Gravity
				rb.AddForce(Vector3.up * -gravity * Time.smoothDeltaTime);

				RaycastForHits();
			}
		}
		else
		{
			if (bDespawn)
			{
				despawnTimer += Time.deltaTime;
				if (despawnTimer >= despawnTime)
				{
					bDone = true;
				}
			}
		}

		if (bDone)
		{
			transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.smoothDeltaTime);
			transform.localPosition = Vector3.Lerp(transform.localPosition, transform.localPosition + transform.forward, Time.smoothDeltaTime);
			if (transform.localScale.magnitude < 0.15f)
			{
				Destroy(gameObject, 0f);
			}
		}

		if (transform.position.y <= -500f)
		{
			Destroy(gameObject);
		}
    }


	void RaycastForHits()
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.position + tipPosition, transform.forward, out hit, raycastDistance * rb.velocity.magnitude))
		{
			if (!hit.collider.isTrigger)
			{
				StrikeObject(hit.transform.gameObject, hit.point);
			}
		}

		lastPosition = transform.position;
	}


	void StrikeObject(GameObject other, Vector3 impactPoint)
	{
		bStruck = true;
		Vector3 impactVelocity = rb.velocity * damage;
		rb.isKinematic = true;
		transform.position = impactPoint - tipPosition;

		if (other.GetComponent<BodyCharacter>())
		{
			transform.parent = other.transform; /// this has some issues if other has irregular transform scale
		}

		if (impactParticles != null)
		{
			Transform newImpact = Instantiate(impactParticles, transform.position, transform.rotation);
			Destroy(newImpact.gameObject, 3f);
		}

		// Force-push hit enemies
		if (other.GetComponent<BodyCharacter>())
		{
			Transform newDamage = Instantiate(damageParticles, transform.position, transform.rotation);
			Destroy(newDamage.gameObject, 3f);

			other.GetComponent<BodyCharacter>().AddMoveCommand(impactVelocity);
		}

		// Set recoverable
		recovery.SetColliderActive(true);
	}


	private void OnTriggerEnter(Collider other)
	{
		if (!bStruck && !other.isTrigger && (other != transform) && (!other.GetComponent<PlayerMovement>()))
		{
			StrikeObject(other.gameObject, transform.position - tipPosition);
		}
	}
}
