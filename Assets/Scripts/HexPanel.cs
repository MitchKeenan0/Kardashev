using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexPanel : MonoBehaviour
{
	public Material normalMaterial;
	public Material touchMaterial;
	public float neighborAffectRange = 0.5f;

	private SpriteRenderer sprite;
	private Rigidbody rb;

	private bool bPhysic = false;
	private float restTimer = 0.0f;
    

    void Start()
    {
		sprite = GetComponent<SpriteRenderer>();
		rb = GetComponent<Rigidbody>();

		SetPhysical(false);
    }


	void Update()
	{
		UpdatePhysics();
	}


	void UpdatePhysics()
	{
		if (rb != null)
		{
			if (bPhysic)
			{
				// Falling motion
				rb.AddForce((Vector3.zero - transform.position) * Time.deltaTime);

				// And return to freeze
				if (rb.velocity.magnitude <= 0.01f)
				{
					restTimer += Time.deltaTime;

					if (restTimer > 0.3f)
					{
						SetPhysical(false);
						restTimer = 0.0f;
					}
				}
			}
		}
	}
	

	public void ReceiveTouch()
	{
		if (sprite != null)
		{

			if (touchMaterial != null)
			{
				sprite.material = touchMaterial;
			}
		}
	}

	public void LoseTouch()
	{
		if (sprite != null)
		{
			NotifyNeighbors();

			Destroy(gameObject, 0.1f);
		}
	}


	public void SetPhysical(bool value)
	{
		bPhysic = value;

		rb.isKinematic = !value;

		if (value)
		{
			NotifyNeighbors();
		}
	}

	public bool IsPhysical()
	{
		return bPhysic;
	}


	void NotifyNeighbors()
	{
		float thisHexDistance = Vector3.Distance(transform.position, Vector3.zero);
		Collider[] hits = Physics.OverlapSphere(transform.position, neighborAffectRange);
		int numHits = hits.Length;
		if (numHits > 0)
		{

			for (int i = 0; i < numHits; i++)
			{
				// Validate each tile..
				HexPanel hex = hits[i].transform.gameObject.GetComponent<HexPanel>();
				if ((hex != null) 
					&& (hex.gameObject != gameObject) 
						&& !hex.IsPhysical())
				{
					// Only notify "higher" tiles that are further from centre of gravity
					float thatHexDistance = Vector3.Distance(hex.transform.position, Vector3.zero);
					if (thatHexDistance > thisHexDistance)
					{
						hex.SetPhysical(true);
					}
				}
			}
		}
	}

}
