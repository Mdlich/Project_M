using UnityEngine;
using System.Collections;
using System;

public class SpikeSpawnModule : BaseSpawnerModule
{
    public static event Action<int, float> WallOfSpikesSpawnedEvent;
    private enum SpawnSide {Left, Right};
    [Space( 20 )]
    [SerializeField] private SpawnSide side;
    [SerializeField] private Vector2Int SpikeWallWidthMinMax = new Vector2Int( 2, 4 );
    [SerializeField] private int penaltyMultiplier = 10;
    [SerializeField] private int penaltyBounces;
    private int Width => UnityEngine.Random.Range( 
        Penalty ? SpikeWallWidthMinMax.x * penaltyMultiplier : SpikeWallWidthMinMax.x,
        Penalty ? SpikeWallWidthMinMax.y * penaltyMultiplier : SpikeWallWidthMinMax.y);

    private bool Penalty => (side == SpawnSide.Left && spawner.LeftWallBounces > penaltyBounces) ||
                            (side == SpawnSide.Right && spawner.RightWallBounces > penaltyBounces);
    protected override void Spawn()
	{
        var currentWidth = Width;
        switch (side)
        {
            case SpawnSide.Left:
            {
                spawner.LastSpawnedModuleL = this;
                SpawnSpike( -1, currentWidth );
                break;
            }
            case SpawnSide.Right:
            {
                spawner.LastSpawnedModuleR = this;
                SpawnSpike( 1, currentWidth );
                break;
            }
        }
    }

    private void SpawnSpike( int side, float width )
    {
        var spike = spawner.GetFromPool<SpikeObstacle>() as SpikeObstacle;
        if (!spike)
            spike = Instantiate( prefab ).GetComponent<SpikeObstacle>();

        spike.transform.SetParent( transform );

        var spawnPoint = side < 0 ? spawner.LeftSpawner.position : spawner.RightSpawner.position;

        var lastPos = spawnPoint;
        lastPos.y += width * spike.Width;
        lastSpawnEndPos = lastPos;

        spike.transform.position = spawnPoint + new Vector3( 0, width * spike.Width / 2f );
        spike.transform.localScale = new Vector3( -side, 1f, 1f );

        spike.gameObject.SetActive( true );
        spike.SetSpikesWidth( width );

        if (Penalty)
        {
            WallOfSpikesSpawnedEvent?.Invoke( side, spawnPoint.y );
            if (this.side == SpawnSide.Left)
                spawner.LeftWallBounces = 0;
            else
                spawner.RightWallBounces = 0;
        }
    }

    protected override bool CanSpawn()
	{
        if ( side == SpawnSide.Left)
        {
            var lastModuleL  = spawner.LastSpawnedModuleL;
            var lastPosL = 0f;
            if (lastModuleL != null)
                lastPosL = lastModuleL.LastSpawnedPosY;
            return spawner.LeftSpawner.position.y - lastPosL >= minDistanceFromLastSpawn;
        }
        if (side == SpawnSide.Right )
        {
            var lastModuleR = spawner.LastSpawnedModuleR;
            var lastPosR = 0f;
            if (lastModuleR != null)
                lastPosR = lastModuleR.LastSpawnedPosY;
            return spawner.RightSpawner.position.y - lastPosR >= minDistanceFromLastSpawn;
        }
        return false;
    }

    public override bool ReadyToSpawn()
    {
        return base.ReadyToSpawn() && CanSpawn();
    }
}
