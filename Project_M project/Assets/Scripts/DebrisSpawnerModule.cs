using UnityEngine;
using System.Collections;

public class DebrisSpawnerModule : BaseSpawnerModule
{
	[Space( 20 )]
	[SerializeField] private Vector2Int cooldownRange = new Vector2Int( 4, 7 );
	[SerializeField] private Vector2Int spawnPosXRange = new Vector2Int( -7, 7 );

	private Vector2 RandomgPos { get => new Vector2( Random.Range( spawnPosXRange.x, spawnPosXRange.y ), spawner.MidSpawner.position.y ); }
	private float nextCD;
	protected override void Spawn()
	{
		spawner.LastSpawnedModuleM = this;
		LastSpawnedTime = Time.time;
		nextCD = Random.Range( cooldownRange.x, cooldownRange.y );

		var debris = spawner.GetFromPool<DebrisObstacle>() as DebrisObstacle;
		if (!debris)
			debris = Instantiate( prefab ).GetComponent<DebrisObstacle>();

		debris.transform.SetParent( transform );
		debris.transform.position = RandomgPos;

		debris.gameObject.SetActive( true );
		var rb = debris.GetComponentInChildren<Rigidbody2D>( true );
		debris.transform.rotation = Quaternion.identity;
		rb.velocity = Vector2.zero;
		rb.angularVelocity = 0f;
		debris.SetDebrisSize();
	}

	protected override bool CanSpawn()
	{
		return true;
	}

	public override bool ReadyToSpawn()
	{
		if (Time.time - LastSpawnedTime > nextCD)
		{
			return true;
		}
		return false;
	}
}
