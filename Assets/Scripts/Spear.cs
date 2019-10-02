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

    void Start()
    {
		rb = GetComponent<Rigidbody>();
    }

    
    void Update()
    {
		if (!bStuck)
		{
			Vector3 deltaV = rb.velocity;
			if (deltaV.magnitude > 0f)
			{
				Quaternion rotation = Quaternion.LookRotation(deltaV, Vector3.up);
				transform.rotation = rotation;

				rb.AddForce(Vector3.up * -gravity);
			}
		}
    }


	private void OnTriggerEnter(Collider other)
	{
		if (!other.isTrigger)
		{
			bStuck = true;
			rb.isKinematic = true;
			//transform.parent = other.gameObject.transform; /// this has some issues if other has irregular transform scale

			if (impactParticles != null)
			{
				Transform newImpact = Instantiate(impactParticles, transform.position, transform.rotation);
				Destroy(newImpact.gameObject, 3f);
			}

			if (other.gameObject.GetComponent<BodyCharacter>())
			{
				Transform newDamage = Instantiate(damageParticles, transform.position, transform.rotation);
				Destroy(newDamage.gameObject, 3f);

				Destroy(other.gameObject, 0.15f);
			}

			Destroy(gameObject, 1f);
		}
	}
}
