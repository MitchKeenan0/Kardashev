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
	public float damageTextDuration = 1.6f;
	public Collider bodyCollider;

	private Rigidbody rb;
	private ThrowingTool tool;
	private ToolRecovery recovery;
	private bool bStruck = false;
	private bool bDone = false;
	private bool bDamageText = false;
	private float despawnTimer = 0f;
	private float charge = 1f;
	private float timeAtDamageText = 0f;
	private float givenSpeed = 0f;

	public void InitSpear(ThrowingTool owningTool, float chargePower, float speed)
	{
		tool = owningTool;
		charge = chargePower;
		givenSpeed = speed;
		RaycastForHits();
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
			RaycastForHits();

			Vector3 deltaV = rb.velocity * 2f;
			if (deltaV.magnitude > 0f)
			{
				// Rotation to velocity
				Quaternion rotation = Quaternion.LookRotation(deltaV, Vector3.up);
				transform.rotation = rotation;

				// Gravity
				rb.AddForce(Vector3.up * -gravity * Time.smoothDeltaTime);
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

		if (bDamageText)
		{
			UpdateDamageText();
		}
    }

	void RaycastForHits()
	{
		if (!rb)
			rb = GetComponent<Rigidbody>();

		// First-frame case when speed is zero
		float speedScalar = rb.velocity.magnitude;
		if (speedScalar < givenSpeed)
		{
			speedScalar = givenSpeed;
		}

		Vector3 spearRay = transform.forward * raycastDistance * speedScalar;
		Vector3 origin = transform.position + (spearRay * -0.1f);
		RaycastHit[] hits = Physics.RaycastAll(origin, spearRay, spearRay.magnitude);
		if (hits.Length > 0)
		{
			foreach (RaycastHit hit in hits)
			{
				if (!hit.collider.isTrigger && (hit.transform != transform) && (hit.transform != tool.owner))
				{
					Vector3 toHit = (hit.point - transform.position).normalized;
					float dotToHit = Vector3.Dot(transform.forward, toHit);
					if (dotToHit > 0.1f)
					{
						StrikeObject(hit.transform.gameObject, hit.point);
					}
				}
			}
		}
	}

	void StrikeObject(GameObject other, Vector3 impactPoint)
	{
		bStruck = true;
		rb.isKinematic = true;
		transform.position = impactPoint + (transform.forward * -tipPosition.z);

		if (impactParticles != null)
		{
			Transform newImpact = Instantiate(impactParticles, transform.position, transform.rotation);
			Destroy(newImpact.gameObject, 3f);
		}

		if (other.GetComponent<StructureHarvester>())
		{
			other.GetComponent<StructureHarvester>().Disperse();
		}

		if (other.GetComponent<Character>())
		{
			// Character damage
			Character chara = other.GetComponent<Character>();
			transform.parent = chara.transform;
			float thisHitDamage = damage * charge * Random.Range(0.8f, 1.2f);
			chara.TakeDamage(thisHitDamage);

			// Numeric damage text
			damageText.text = thisHitDamage.ToString("F2");
			if ((thisHitDamage / damage) >= 2f)
			{
				damageText.color = Color.red;
			}
			damageText.enabled = true;
			damageText.transform.position = Camera.main.WorldToScreenPoint(impactPoint);
			timeAtDamageText = Time.time;
			bDamageText = true;

			// Visuals
			Transform newDamage = Instantiate(damageParticles, transform.position, transform.rotation);
			newDamage.transform.parent = other.transform;
			Destroy(newDamage.gameObject, 15f);
		}

		// Physics
		if (other.GetComponent<Rigidbody>())
		{
			Vector3 impactVelocity = rb.velocity * Mathf.Sqrt(rb.velocity.magnitude) * impact;
			Rigidbody hitRb = other.GetComponent<Rigidbody>();
			hitRb.AddForce(impactVelocity, ForceMode.Impulse);
		}

		// Set recoverable
		bodyCollider.enabled = true;

		if (!recovery)
			recovery = GetComponentInChildren<ToolRecovery>();
		if (recovery != null)
			recovery.SetColliderActive(true);
	}

	void UpdateDamageText()
	{
		damageText.rectTransform.position += (Vector3.up * Time.smoothDeltaTime * 10f);
		
		if (Time.time > (timeAtDamageText + damageTextDuration))
		{
			damageText.enabled = false;
		}
	}

	public void RecoverSpear()
	{
		tool.reserveAmmo += tool.throwCost;
		if ((tool.owner.GetComponent<Character>().GetEquippedItem() != null)
			&& tool.owner.GetComponent<Character>().GetEquippedItem() == tool.gameObject)
		{
			tool.GetHudInfo().SetToolReserve(tool.reserveAmmo.ToString());
		}

		if (tool.reserveAmmo == 1)
		{
			tool.RecoverMockFast();
		}

		Destroy(gameObject);
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
}
