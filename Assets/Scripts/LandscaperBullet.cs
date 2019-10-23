using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandscaperBullet : Bullet
{
	public float falloff = 5f;
	public bool bImpartVelocity = true;

	public override void Start()
	{
		base.Start();
	}

	public override void Update()
	{
		base.Update();
	}

	public override void LandHit(RaycastHit hit, Vector3 hitPosition)
	{
		base.LandHit(hit, hitPosition);

		if (impactParticles != null)
		{
			// Spawning impact particles
			Transform hitParticles = Instantiate(impactParticles, hitPosition, transform.rotation);
			Destroy(hitParticles.gameObject, 1.1f);

			// Detach lifetime particles
			if (onboardParticles != null)
			{
				onboardParticles.parent = null;
				Destroy(onboardParticles.gameObject, 1.0f);
			}

			// Damage & Radius
			float thisHitDamage = damage;
			float thisHitRadius = radiusOfEffect;

			// Ground Manipulations
			if (hit.transform.CompareTag("Land"))
			{

				Collider[] cols = Physics.OverlapSphere(transform.position, radiusOfEffect * 2f);
				if (cols.Length > 0)
				{
					for (int i = 0; i < cols.Length; i++)
					{
						// "Bubbling" player, vehicle and others just over rising terrain
						if ((thisHitDamage > 0f) && cols[i].gameObject.GetComponent<CharacterController>())
						{
							CharacterController controller = cols[i].gameObject.GetComponent<CharacterController>();
							PlayerBody player = controller.gameObject.GetComponent<PlayerBody>();
							bool canMove = true;
							if ((player != null) && player.IsRiding())
								canMove = false;

							if (canMove)
							{
								controller.Move(Vector3.up * thisHitDamage);
							}
						}

						// Mesh movement
						else if (cols[i].gameObject.CompareTag("Land"))
						{
							MeshFilter mFilter = cols[i].transform.gameObject.GetComponent<MeshFilter>();
							if (mFilter != null)
							{
								// Moving the verts
								Mesh mesh = mFilter.mesh;
								Vector3[] vertices = mesh.vertices;
								int numVerts = vertices.Length;
								for (int j = 0; j < numVerts; j++)
								{
									float distToHit = Vector3.Distance(hit.point, GetVertexWorldPosition(vertices[j], mFilter.transform));
									if (distToHit <= (thisHitRadius * cols[i].transform.localScale.magnitude))
									{
										// Calc movement of the ground
										Vector3 advanceVector = Vector3.up;
										if (bImpartVelocity)
										{
											advanceVector += transform.forward;
										}

										Vector3 vertToHit = GetVertexWorldPosition(vertices[j], mFilter.transform) - hit.point;
										vertToHit.y *= 0f;
										float proximityScalar = (thisHitRadius * cols[i].transform.localScale.magnitude) - vertToHit.magnitude;
										proximityScalar = Mathf.Clamp(proximityScalar, 0f, 1f);

										vertices[j] += advanceVector * thisHitDamage * proximityScalar;
									}
								}

								// Recalculate the mesh & collision
								mesh.vertices = vertices;
								mFilter.mesh = mesh;
								mesh.RecalculateBounds();

								MeshCollider meshCollider = cols[i].transform.GetComponent<MeshCollider>();
								if (meshCollider)
									meshCollider.sharedMesh = mFilter.mesh;
							}
						}
					}
				}
			}

			Destroy(gameObject);
		}
	}

	public Vector3 GetVertexWorldPosition(Vector3 vertex, Transform owner)
	{
		return owner.localToWorldMatrix.MultiplyPoint3x4(vertex);
	}
}
