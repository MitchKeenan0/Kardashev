using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : MonoBehaviour
{
	public float gravity = 1f;
	public Transform impactParticles;
	public Transform damageParticles;

	private Rigidbody rb;
	private bool bStuck = false;
	private bool bDone = false;
	private float despawnTimer = 0f;

    void Start()
    {
		rb = GetComponent<Rigidbody>();
    }

    
    void Update()
    {
		if (!bStuck)
		{
			Vector3 deltaV = rb.velocity * 2f;
			if (deltaV.magnitude > 0f)
			{
				Quaternion rotation = Quaternion.LookRotation(deltaV, Vector3.up);
				transform.rotation = rotation;

				rb.AddForce(Vector3.up * -gravity);
			}
		}
		else
		{
			despawnTimer += Time.deltaTime;
			if (despawnTimer >= 1f)
			{
				bDone = true;
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
    }


	private void OnTriggerEnter(Collider other)
	{
		Vector3 impactVelocity = rb.velocity * 0.1f;

		if (!other.isTrigger)
		{
			bStuck = true;
			rb.isKinematic = true;
			if (other.gameObject.GetComponent<BodyCharacter>())
			{
				transform.parent = other.gameObject.transform; /// this has some issues if other has irregular transform scale
			}

			if (impactParticles != null)
			{
				Transform newImpact = Instantiate(impactParticles, transform.position, transform.rotation);
				Destroy(newImpact.gameObject, 3f);
			}

			// Force-push hit enemies
			if (other.gameObject.GetComponent<BodyCharacter>())
			{
				Transform newDamage = Instantiate(damageParticles, transform.position, transform.rotation);
				Destroy(newDamage.gameObject, 3f);

				other.gameObject.GetComponent<BodyCharacter>().AddMoveCommand(impactVelocity);
			}
		}
	}
}
