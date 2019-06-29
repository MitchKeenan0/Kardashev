using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexPanel : MonoBehaviour
{
	public Material normalMaterial;
	public Material touchMaterial;
	public Material connectedMaterial;
	public float fallForce = 1.0f;
	public float neighborAffectRange = 0.5f;
	public float neighborNotifyDelay = 0.15f;
	public bool bConnected = false;

	private SpriteRenderer sprite;
	private Rigidbody rb;
	private PeopleConnection connection;

	private bool bPopulated = false;
	private bool bPhysic = false;
	private bool bFrozen = false;
	private float restTimer = 0.0f;
	private float notifyTimer = 0.0f;
    
	public bool IsFrozen()
	{
		return bFrozen;
	}

	public void SetPopulated(bool value)
	{
		bPopulated = value;
	}
	public bool IsPopulated()
	{
		return bPopulated;
	}

    void Start()
    {
		sprite = GetComponent<SpriteRenderer>();
		rb = GetComponent<Rigidbody>();
		connection = FindObjectOfType<PeopleConnection>();

		SetPhysical(false);
    }


	void Update()
	{
		if (bPhysic && !bFrozen)
		{
			UpdatePhysics();
		}

		if (notifyTimer > 0.0f)
		{
			WaitForNotify();
		}
	}


	void UpdatePhysics()
	{
		// Falling motion
		Vector3 inwardGravity = (Vector3.zero - transform.position).normalized;
		rb.AddForce(inwardGravity * fallForce * Time.deltaTime);

		// And return to freeze
		if (rb.velocity.magnitude <= 0.01f)
		{
			restTimer += Time.deltaTime;

			if (restTimer > 0.3f)
			{
				SetPhysical(false);

				if (connection != null)
				{
					connection.TestFromPoint(transform.position);
				}

				restTimer = 0.0f;
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
		if (!bFrozen)
		{
			bPhysic = value;

			if (rb != null)
			{
				rb.isKinematic = !value;
			}

			if (value)
			{
				notifyTimer = 0.001f; /// kick it off
			}
		}
	}

	public void Freeze()
	{
		SetPhysical(false);

		bFrozen = true;

		ReceiveTouch();
	}

	public bool IsPhysical()
	{
		return bPhysic;
	}


	void WaitForNotify()
	{
		notifyTimer += Time.deltaTime;

		if (notifyTimer >= neighborNotifyDelay)
		{
			NotifyNeighbors();
			notifyTimer = 0.0f;
		}
	}


	void NotifyNeighbors()
	{
		float thisHexDistance = Vector3.Distance(transform.position, Vector3.zero);

		Collider[] rawNeighbors = Physics.OverlapSphere(transform.position, neighborAffectRange);
		int numHits = rawNeighbors.Length;
		if (numHits > 0)
		{

			for (int i = 0; i < numHits; i++)
			{
				// Validate each tile..
				HexPanel hex = rawNeighbors[i].transform.gameObject.GetComponent<HexPanel>();
				if ((hex != null) 
					&& (hex.gameObject != gameObject) 
						&& !hex.IsPhysical())
				{

					// Only notify "higher" tiles that are further from centre of gravity
					float thatHexDistance = Vector3.Distance(hex.transform.position, Vector3.zero);
					if (thatHexDistance > thisHexDistance)
					{
						if (!hex.bFrozen)
						{
							hex.SetPhysical(true);
						}
					}
				}
			}
		}
	}


}
