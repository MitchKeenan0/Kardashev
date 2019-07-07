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
	public bool bFirstTime = true;

	private SpriteRenderer sprite;
	private Rigidbody rb;
	private PeopleConnection connection;
	private LineRenderer line;

	private bool bPopulated = false;
	private bool bPhysic = false;
	private bool bFrozen = false;
	private float restTimer = 0.0f;
	private float notifyTimer = 0.0f;
	private float timeAtPhysical = 0.0f;
    
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
		line = GetComponent<LineRenderer>();

		SetPhysical(true);
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
		if ((rb.velocity.magnitude <= 0.1f)
			&& ((Time.time - timeAtPhysical) >= 0.2f))
		{
			restTimer += Time.deltaTime;

			float timeToBeat = 0.1f;
			if (bPopulated)
			{
				timeToBeat += 0.1f;
			}
			if (restTimer > timeToBeat)
			{
				SetPhysical(false);

				restTimer = 0.0f;
			}
		}

		UpdateLineRender(bPhysic);
	}
	

	public void ReceiveTouch()
	{
		if (touchMaterial != null)
		{
			sprite.material = touchMaterial;
			sprite.color *= 2.0f;
		}
	}

	public void LoseTouch()
	{
		bFirstTime = false;

		NotifyNeighbors();

		sprite.color *= 0.5f;

		Destroy(gameObject, 0.05f);
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
			else
			{
				rb = GetComponent<Rigidbody>();
				if (rb != null)
				{
					rb.isKinematic = !value;
				}
			}

			if (value)
			{
				notifyTimer = 0.001f; /// kick it off
				timeAtPhysical = Time.time;
			}
			else if (line != null)
			{
				line.enabled = false;
			}
		}
	}

	public void Freeze()
	{
		if (sprite != null)
		{
			sprite.material = connectedMaterial;
		}
		else
		{
			sprite = GetComponent<SpriteRenderer>();
			if (sprite != null)
			{
				sprite.material = connectedMaterial;
			}
		}

		SetPhysical(false);

		if (line != null)
		{
			line.enabled = false;
		}


		bFrozen = true;
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
		bool bLivingNeighbor = false;

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

					// replace below with dot product check
					Vector3 toCentre = (Vector3.zero - transform.position);

					// Only notify "higher" tiles that are further from centre of gravity
					float thatHexDistance = Vector3.Distance(hex.transform.position, Vector3.zero);
					if (thatHexDistance > thisHexDistance)
					{
						if (!hex.bFrozen)
						{
							hex.SetPhysical(true);
						}
					}

					// Look for neighbor
					if (hex.IsPopulated())
					{
						bLivingNeighbor = true;
					}
				}
			}
		}

		if (!bLivingNeighbor)
		{
			if (bPopulated)
			{
				PeopleConnection connection = FindObjectOfType<PeopleConnection>();
				if (connection != null)
				{
					connection.DisconnectHex(this);
				}
			}
		}
	}


	void UpdateLineRender(bool On)
	{
		if (On)
		{
			if (!line.enabled)
			{
				line.enabled = true;
			}

			Vector3 myVelocity = transform.position + (rb.velocity.normalized * Time.deltaTime);
			line.SetPosition(1, myVelocity);
		}

		if (!On)
		{
			if (line.enabled)
			{
				line.enabled = false;
			}

			line.SetPosition(1, transform.position);
		}
	}


}
