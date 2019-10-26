using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spear : MonoBehaviour
{
	public float gravity = 1f;
	public float damage = 10f;
	public float impact = 0.2f;
	public Transform impactParticles;
	public Transform damageParticles;
	public bool bDespawn = false;
	public float despawnTime = 1f;
	public float raycastDistance = 1.1f;
	public Vector3 tipPosition;
	public Text damageText;
	public Collider bodyCollider;

	private Rigidbody rb;
	private ThrowingTool tool;
	private ToolRecovery recovery;
	private bool bStruck = false;
	private bool bDone = false;
	private bool bDamageText = false;
	private float despawnTimer = 0f;
	private float charge = 1f;
	private float timeAtHit = 0f;

	public void InitSpear(ThrowingTool owningTool, float chargePower)
	{
		tool = owningTool;
		charge = chargePower;
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

	public void SetPhysical(bool value)
	{
		rb.isKinematic = !value;
		if (value)
		{
			bodyCollider.enabled = true;
		}
		else
		{
			bodyCollider.enabled = false;
		}
	}


    void Start()
    {
		rb = GetComponent<Rigidbody>();
		recovery = GetComponentInChildren<ToolRecovery>();
		recovery.SetColliderActive(false);
		damageText.enabled = false;
		bodyCollider.enabled = false;
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

		if (bDamageText)
		{
			UpdateDamageText();
		}
    }


	void RaycastForHits()
	{
		RaycastHit[] hits = Physics.RaycastAll(transform.position + (transform.forward * tipPosition.z), transform.forward, raycastDistance * rb.velocity.magnitude);
		if (hits.Length > 0)
		{
			foreach (RaycastHit hit in hits)
			{
				if (!hit.collider.isTrigger && (hit.transform != transform))
				{
					StrikeObject(hit.transform.gameObject, hit.point);
				}
			}
		}
	}


	void StrikeObject(GameObject other, Vector3 impactPoint)
	{
		bStruck = true;
		Vector3 impactVelocity = rb.velocity * Mathf.Sqrt(rb.velocity.magnitude) * impact;
		rb.isKinematic = true;
		transform.position = impactPoint + (transform.forward * -tipPosition.z);

		if (other.GetComponent<BodyCharacter>())
		{
			BodyCharacter body = other.GetComponent<BodyCharacter>();
			transform.parent = other.transform;
			float dmg = damage * charge * Random.Range(0.8f, 1.2f);
			body.TakeDamage(dmg);

			Transform newDamage = Instantiate(damageParticles, transform.position, transform.rotation);
			newDamage.transform.parent = other.transform;
			Destroy(newDamage.gameObject, 15f);

			body.AddMoveCommand(impactVelocity);

			damageText.text = dmg.ToString("F2");
			if ((dmg / damage) > 2f)
			{
				damageText.color = Color.red;
			}
			damageText.enabled = true;
			damageText.transform.position = Camera.main.WorldToScreenPoint(impactPoint);
			timeAtHit = Time.time;
			bDamageText = true;
		}

		if (impactParticles != null)
		{
			Transform newImpact = Instantiate(impactParticles, transform.position, transform.rotation);
			Destroy(newImpact.gameObject, 3f);
		}

		// Set recoverable
		bodyCollider.enabled = true;
		recovery.SetColliderActive(true);
	}

	void UpdateDamageText()
	{
		damageText.rectTransform.position += (Vector3.up * Time.smoothDeltaTime * 10f);
		
		if (Time.time > (timeAtHit + 1.62f))
		{
			damageText.enabled = false;
		}
	}


}
