using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainJob /// : MonoBehaviour
{
	public Vector3 Location;
	public float EffectIncrement;
	public float RadiusOfEffect;
	public float Duration;
	public float RadiusFalloff;

	private float _timeAtCreation;
	public float timeAtCreation
	{
		get { return _timeAtCreation; }
		set { _timeAtCreation = value; }
	}

	private float _lifeTime;
	public float lifeTime
	{
		get { return _lifeTime; }
		set { _lifeTime = value; }
	}

	private float _radius;
	public float radius
	{
		get { return _radius; }
		set { _radius = value; }
	}


	public TerrainJob(Vector3 location, float effectIncrement, float radiusOfEffect, float duration, float radiusFalloff)
	{
		Location = location;
		EffectIncrement = effectIncrement;
		RadiusOfEffect = radiusOfEffect;
		Duration = duration;
		RadiusFalloff = radiusFalloff;
		_lifeTime = 0f;
		_radius = RadiusOfEffect;
	}

}
