using UnityEngine;
using System.Collections;
using System;

public class DoubleSpikeWallModule : BaseSpawnerModule
{
    public static event Action<int, float> WallOfSpikesSpawnedEvent;
    [Space( 20 )]
    [SerializeField] private Vector2Int SpikeWallWidthMinMax = new Vector2Int(15, 30);
    [SerializeField] private float momentumThreshold = 0.5f;

    private int Width => UnityEngine.Random.Range( SpikeWallWidthMinMax.x, SpikeWallWidthMinMax.y );

    protected override void Spawn()
    {
        spawner.LasBigSpawnTime = Time.time;
        spawner.LastSpawnedModuleL = this;
        spawner.LastSpawnedModuleR = this;

        var currentWidth = Width;

        SpawnSpikeWall(-1, currentWidth);
        SpawnSpikeWall( 1, currentWidth );
    }
    private void SpawnSpikeWall(int side, float width)
    {
        var spike = spawner.GetFromPool<SpikeObstacle>() as SpikeObstacle;
        if (!spike)
            spike = Instantiate( prefab ).GetComponent<SpikeObstacle>();

        spike.transform.SetParent( transform );

        var spawnPoint = side < 0 ? spawner.LeftSpawner.position : spawner.RightSpawner.position;

        var lastPos = spawnPoint;
        lastPos.y += width * spike.Width;
        lastSpawnEndPos = lastPos;

        spike.transform.position = spawnPoint + new Vector3( 0, width * spike.Width / 2 );
        spike.transform.localScale = new Vector3( -side, 1f, 1f );

        spike.gameObject.SetActive( true );
        spike.SetSpikesWidth( width );

        WallOfSpikesSpawnedEvent?.Invoke( side, spawnPoint.y );
    }

    protected override bool CanSpawn()
    {
        var lastModuleL = spawner.LastSpawnedModuleL;
        var lastModuleR = spawner.LastSpawnedModuleR;
        var lastPosL = 0f;
        var lastPosR = 0f;
        if (lastModuleL != null)
            lastPosL = lastModuleL.LastSpawnedPosY;
        if (lastModuleR != null)
            lastPosR = lastModuleR.LastSpawnedPosY;

        if (spawner.LeftSpawner.position.y - lastPosL >= minDistanceFromLastSpawn &&
            spawner.RightSpawner.position.y - lastPosR >= minDistanceFromLastSpawn)
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
