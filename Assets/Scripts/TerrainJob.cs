using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainJob /// : MonoBehaviour
{
	public Vector3 Location;
	public float EffectIncrement;
	public float RadiusOfEffect;
	public float Duration;

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


	public TerrainJob(Vector3 location, float effectIncrement, float radiusOfEffect, float duration)
	{
		Location = location;
		EffectIncrement = effectIncrement;
		RadiusOfEffect = radiusOfEffect;
		Duration = duration;
		_lifeTime = 0f;
	}

}
