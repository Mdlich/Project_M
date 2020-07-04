using UnityEngine;
using System.Collections;

public interface ISpawnModule
{
	int Priority { get;}
	bool ReadyToSpawn();
	IEnumerator SpawnRoutine();
	float LastSpawnedPosY { get; }
	float LastSpawnedTime { get; }
}
