using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexPanel : MonoBehaviour
{
	public Material normalMaterial;
	public Material touchMaterial;
	public Material connectedMaterial;
	public Transform destructParticles;
	public float fallForce = 1.0f;
	public float explodeForce = 10.0f;
	public float neighborAffectRange = 0.5f;
	public float neighborNotifyDelay = 0.15f;
	public bool bDrawVelocity = false;
	public bool bDrawTrail = false;
	public float trailLength = 0.3f;
	public bool bConnected = false;
	public bool bFirstTime = true;
	public bool bEnemy = false;

	private SpriteRenderer sprite;
	private Rigidbody rb;
	private PeopleConnection connection;
	private GameSystem game;
	private LineRenderer line;
	private TrailRenderer trail;

	private bool bPopulated = false;
	private bool bPhysic = false;
	private bool bFrozen = false;
	private bool bMovedThisTurn = false;
	private bool bMoving = false;

	private float restTimer = 0.0f;
	private float notifyTimer = 0.0f;
	private float timeAtPhysical = 0.0f;
	private Vector3 gravityPosition = Vector3.zero;

	
	public void SetGravityPosition(Vector3 position)
	{
		gravityPosition = position;
	}

	public bool GetMovedThisTurn()
	{
		return bMovedThisTurn;
	}
	public void SetMovedThisTurn(bool value)
	{
		bMovedThisTurn = value;
	}
    
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
		trail = GetComponent<TrailRenderer>();

		//SetPhysical(true);

		if (!bDrawVelocity)
		{
			line.enabled = false;
		}

		if (!bDrawTrail)
		{
			trail.enabled = false;
		}

		game = FindObjectOfType<GameSystem>();
	}


	void Update()
	{
		if (bPhysic && !bFrozen)
		{
			UpdatePhysics();
		}

		if (notifyTimer > 0.0f)
		{
			if (rb.velocity.magnitude > 0.5f)
			{
				WaitForNotify();
			}
		}
	}


	void UpdatePhysics()
	{
		// This helps the tile around inward obstacles
		AssistiveGravity();

		// Falling solution
		Vector3 inwardGravity = (gravityPosition - transform.position).normalized;
		Vector3 gravityOffset = Random.insideUnitCircle * 0.3f;
		inwardGravity += gravityOffset;
		inwardGravity = (inwardGravity).normalized * fallForce * Time.deltaTime;
		rb.AddForce(inwardGravity);

		// And return to freeze
		if ((rb.velocity.magnitude <= 0.2f)
			&& ((Time.time - timeAtPhysical) >= 0.3f))
		{
			restTimer += Time.deltaTime;

			float timeToBeat = 0.1f;
			if (bPopulated)
			{
				timeToBeat += 0.1f;
			}
			if (restTimer >= timeToBeat)
			{
				SetPhysical(false);

				game.UpdateHexMovers(-1);
				bMoving = false;

				bMovedThisTurn = true;

				restTimer = 0.0f;
			}
		}

		if (bDrawVelocity)
		{
			UpdateLineRender(bPhysic);
		}
	}


	void AssistiveGravity()
	{
		RaycastHit[] hits;
		Vector3 start = Camera.main.transform.position;
		Vector3 direction = (Vector3.zero - start) * 1.5f;
		gravityPosition = Vector3.zero;

		hits = Physics.RaycastAll(start, direction, 10.0f);
		int numHits = hits.Length;
		if (numHits >= 1)
		{
			if (hits[0].rigidbody != rb)
			{
				RaycastHit hit = hits[0];
				Vector3 normal = hit.normal;
				gravityPosition += (hit.point + normal);
			}
			else if (hits[1].rigidbody != null)
			{
				RaycastHit hit = hits[1];
				Vector3 normal = hit.normal;
				gravityPosition += (hit.point + normal);
			}
		}
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

		NotifyNeighbors(true);

		sprite.color *= 0.5f;

		HexCharacter character = GetComponentInChildren<HexCharacter>();
		if (character != null)
		{
			character.DestructCharacter();
		}

		Transform newEffect = Instantiate(destructParticles, transform.position, Quaternion.identity);
		Destroy(newEffect.gameObject, 2.5f);

		if (bMoving)
		{
			game.UpdateHexMovers(-1);
		}

		Destroy(gameObject, 0.05f);
	}


	public void SetPhysical(bool value)
	{
		if (game == null)
		{
			game = FindObjectOfType<GameSystem>();
		}

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
				transform.localScale *= 0.9f;

				notifyTimer = 0.001f; /// kick it off

				timeAtPhysical = Time.time;
				game.UpdateHexMovers(1);
				bMoving = true;
				bMovedThisTurn = true;
			}
			else
			{
				transform.localScale *= 1.1f;

				if (line != null)
				{
					line.enabled = false;
				}
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

		if (line != null)
		{
			line.enabled = false;
		}

		if (!bFrozen && bMoving)
		{
			game.UpdateHexMovers(-1);
		}

		SetPhysical(false);

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
			NotifyNeighbors(false);
			notifyTimer = 0.0f;
		}
	}


	void NotifyNeighbors(bool terminal)
	{
		float thisHexDistance = Vector3.Distance(transform.position, gravityPosition);
		bool bLivingNeighbor = false;
		float scaledRange = neighborAffectRange;

		Collider[] rawNeighbors = Physics.OverlapSphere(transform.position, scaledRange);
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
					float thatHexDistance = Vector3.Distance(hex.transform.position, gravityPosition);
					if (thatHexDistance > thisHexDistance)
					{
						if (!hex.bFrozen) //  && !hex.bMovedThisTurn
						{
							hex.SetPhysical(true);

							if (rb.velocity.magnitude > 0.5f)
							{
								Vector3 force = (hex.transform.position - transform.position).normalized;

								if (terminal)
								{
									force *= explodeForce;
								}

								hex.GetComponent<Rigidbody>().AddForce(force * explodeForce);
							}
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

			Vector3 myVelocity = transform.position + (rb.velocity * Time.deltaTime).normalized;
			line.SetPosition(1, myVelocity * -trailLength);
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
