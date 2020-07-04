using UnityEngine;
using System.Collections;
using System;

public class BaseSpawnerModule : MonoBehaviour, ISpawnModule
{
	[SerializeField] protected Spawnable prefab;
	[SerializeField] protected int modulePriority = 1;
	[SerializeField] protected Vector2 randomDistanceFromLastSpawn = new Vector2( 5f, 10f);
	[SerializeField] protected float spawnCooldown = 10f;
	[SerializeField] protected float bigSpawnCooldown;

	protected Vector2 lastSpawnEndPos;
	protected SpawningSystem spawner;

	protected float minDistanceFromLastSpawn;
	public int Priority => modulePriority;

	public float LastSpawnedPosY => lastSpawnEndPos.y;

	public float LastSpawnedTime { get; protected set; }

	private float lastSpawnTime;
	private void Awake()
	{
		spawner = GetComponent<SpawningSystem>();
	}

	public virtual bool ReadyToSpawn()
	{
		if (Time.time - lastSpawnTime >= spawnCooldown)
		{
			return true;
		}
		else
			return false;
	}

	public virtual IEnumerator SpawnRoutine()
	{
		while (!CanSpawn())
		{
			yield return null;
		}
		lastSpawnTime = Time.time;
		minDistanceFromLastSpawn = UnityEngine.Random.Range( randomDistanceFromLastSpawn.x, randomDistanceFromLastSpawn.y );
		Spawn();
	}

	protected virtual void Spawn()
	{
		Debug.Log( $"{name} spawning something" );
		//throw new NotImplementedException();
	}

	protected virtual bool CanSpawn()
	{
		Debug.Log( $"{name} can spawn" );
		return true;
		//throw new NotImplementedException();
	}
}
