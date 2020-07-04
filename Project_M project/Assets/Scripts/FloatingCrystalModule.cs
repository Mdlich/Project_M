using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingCrystalModule : BaseSpawnerModule
{
	public static event Action<float> FloatingCrystalsSpawnedEvent;

	[Space( 20 )]
	[SerializeField] float spriteWidth = 2f;
	[SerializeField] int spawnCount = 30;
	[SerializeField] int maxTriesPerCount = 10;
	[SerializeField] float minSeparation = 3f;
	[SerializeField] Vector2 radiusRange = new Vector2( 0.5f, 3f );
	[SerializeField] Vector2 areaBounds = new Vector2( 7f, 10f );
	[SerializeField] private float momentumThreshold = 0.5f;

	//public float RadiusY { get => areaBounds.y; }

	private List<Circle> confirmedCircles = new List<Circle>();

	private struct Circle
	{
		public float radius;
		public Vector2 position;

		public Circle(Vector2 pos, float radius )
		{
			this.radius = radius;
			this.position = pos;
		}
	}
	protected override void Spawn()
	{
		spawner.LasBigSpawnTime = Time.time;
		spawner.LastSpawnedModuleL = this;
		spawner.LastSpawnedModuleR = this;

		var lastPos = spawner.MidSpawner.position;
		lastPos.y += 2f * areaBounds.y;
		lastSpawnEndPos = lastPos;

		GetSpawnPoints();
		SpawnCrystals();

		FloatingCrystalsSpawnedEvent?.Invoke( lastPos.y - areaBounds.y * 2f );
	}

	private void GetSpawnPoints()
	{
		confirmedCircles.Clear();
		int loopProtection = 0;
		Circle tempCircle;
		Vector2 pos = spawner.MidSpawner.position;
		pos.y += 2f * areaBounds.y;
		while (confirmedCircles.Count < spawnCount && loopProtection < maxTriesPerCount * spawnCount)
		{
			bool goodPos = true;
			var rX = UnityEngine.Random.Range( -areaBounds.x, areaBounds.x );
			var rY = UnityEngine.Random.Range( -areaBounds.y, areaBounds.y );
			tempCircle = new Circle( pos + new Vector2( rX, rY ), UnityEngine.Random.Range( radiusRange.x, radiusRange.y ) );
			foreach (var circle in confirmedCircles)
			{
				if (Vector2.Distance( circle.position, tempCircle.position ) < spriteWidth * (circle.radius + tempCircle.radius) + minSeparation)
				{
					goodPos = false;
					break;
				}
			}
			if (goodPos)
			{
				confirmedCircles.Add( tempCircle );
			}
			loopProtection++;
		}
	}

	private void SpawnCrystals()
	{
		foreach (var circle in confirmedCircles)
		{
			var crystal = spawner.GetFromPool<FloatingCrystal>() as FloatingCrystal;
			if (!crystal)
				crystal = Instantiate( prefab ).GetComponent<FloatingCrystal>();

			crystal.transform.SetParent( transform );
			crystal.transform.position = circle.position;
			crystal.SetRadius( circle.radius );
			crystal.gameObject.SetActive( true );
		}
	}

	protected override bool CanSpawn()
	{
		var lastModuleL = spawner.LastSpawnedModuleL;
		var lastModuleR = spawner.LastSpawnedModuleR;
		var lastPosL = 0f;
		var lastPosR = 0f;
		if (lastModuleL != null)
			lastPosL = lastModuleL.LastSpawnedPosY;
		if(lastModuleR != null)
			lastPosR = lastModuleR.LastSpawnedPosY;
		if (spawner.MidSpawner.position.y - lastPosL >= minDistanceFromLastSpawn &&
			spawner.MidSpawner.position.y - lastPosR >= minDistanceFromLastSpawn)
		{
			return true;
		}
		return false;
	}

	public override bool ReadyToSpawn()
	{
		return base.ReadyToSpawn() && PlayerController.instance.Momentum > momentumThreshold && Time.time - spawner.LasBigSpawnTime >= bigSpawnCooldown;
	}
}
