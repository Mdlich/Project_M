using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField]
    private Transform pool;
    [SerializeField]
    private GameObject wallSpikePrefab;
    [SerializeField]
    private GameObject debrisPrefab;
    [SerializeField]
    private GameObject sporePrefab;
    [SerializeField]
    private Transform leftSpawner;
    [SerializeField]
    private Transform midSpawner;
    [SerializeField]
    private Transform rightSpawner;
    [SerializeField]
    private float minSpawnDistance = 2f;
    [SerializeField]
    private float maxSpawnDistance = 10f;
    [SerializeField]
    private int bouncesForWallSpike = 4;
    [SerializeField]
    private int spikeWallMultiplier = 20;
    [SerializeField]
    private int minSpikesWidth = 1;
    [SerializeField]
    private int maxSpikesWidth = 3;
    [SerializeField]
    private float stalactiteSpawnTimerMin = 3f;
    [SerializeField]
    private float stalactiteSpawnTimerMax = 6f;
    [SerializeField]
    private float sporeSpawnTimerMin = 3f;
    [SerializeField]
    private float sporeSpawnTimerMax = 6f;
    [SerializeField]
    private float sporeMinDistance = 5f;

    private int RandomSpikeWidth { get { return UnityEngine.Random.Range( minSpikesWidth, maxSpikesWidth ); } }

    private float lastLeftObstaclePosY;
    private float lastSporePosY;
    private float lastRightObstaclePosY;
    private float obstacleSpawnDistance;
    private float sporeHalfSize;
    private float nextDebrisSpawnTime;
    private float nextSporeSpawnTime;
    private int leftWallBounces;
    private int rightWallBounces;

    void Start()
    {
        obstacleSpawnDistance = UnityEngine.Random.Range( minSpawnDistance, maxSpawnDistance );
        sporeHalfSize = sporePrefab.GetComponentInChildren<SpriteRenderer>().size.y / 2f;
        PlayerController.WallBounceEvent += OnWallBounce;
    }

    void Update()
    {
        var currentObstacleWidth = leftWallBounces >= bouncesForWallSpike ? RandomSpikeWidth * spikeWallMultiplier : RandomSpikeWidth;
        if (rightSpawner.transform.position.y > obstacleSpawnDistance + lastLeftObstaclePosY)
        {
            obstacleSpawnDistance = UnityEngine.Random.Range( minSpawnDistance, maxSpawnDistance );
            SpawnWallSpike( -1, currentObstacleWidth );
            lastLeftObstaclePosY = leftSpawner.position.y + currentObstacleWidth * 2f;
        }

        currentObstacleWidth = rightWallBounces >= bouncesForWallSpike ? RandomSpikeWidth * spikeWallMultiplier : RandomSpikeWidth;
        if (rightSpawner.transform.position.y >= obstacleSpawnDistance + lastRightObstaclePosY)
        {
            obstacleSpawnDistance = UnityEngine.Random.Range( minSpawnDistance, maxSpawnDistance );
            SpawnWallSpike( 1, currentObstacleWidth );
            lastRightObstaclePosY = rightSpawner.position.y + currentObstacleWidth * 2f;
        }

        if (Time.time > nextDebrisSpawnTime)
        {
            nextDebrisSpawnTime = Time.time + UnityEngine.Random.Range( stalactiteSpawnTimerMin, stalactiteSpawnTimerMax );

            SpawnDebris( 2f );
        }

        if (Time.time > nextSporeSpawnTime && midSpawner.transform.position.y - lastSporePosY >= sporeMinDistance)
        {
            nextSporeSpawnTime = Time.time + UnityEngine.Random.Range( sporeSpawnTimerMin, sporeSpawnTimerMax );

            SpawnSpore();

            lastSporePosY = midSpawner.position.y + sporeHalfSize;
        }
    }

    private Vector2 GetRandomMidSpawnPos()
    {
        return new Vector2( UnityEngine.Random.Range( -7f, 7f ), midSpawner.transform.position.y );
    }
    private void SpawnSpore()
    {
        var spore = pool.GetComponentInChildren<Spore>( true );
        if (!spore)
            spore = Instantiate( sporePrefab ).GetComponent<Spore>();

        spore.transform.SetParent( transform );
        spore.transform.position = GetRandomMidSpawnPos();

        spore.gameObject.SetActive( true );
        spore.ReadyToPush += PushObstacle;
    }
    private void OnWallBounce(int side )
    {
        if (side == -1)
        {
            rightWallBounces = 0;
            leftWallBounces ++;
        }
        else
        {
            leftWallBounces = 0;
            rightWallBounces ++;
        }
    }
    private void SpawnDebris(float size)
    {
        var debris = pool.GetComponentInChildren<DebrisObstacle>( true );
        if (!debris)
            debris = Instantiate( debrisPrefab ).GetComponent<DebrisObstacle>();

        debris.transform.SetParent( transform );
        debris.transform.position = GetRandomMidSpawnPos();

        debris.gameObject.SetActive( true );
        var rb = debris.GetComponentInChildren<Rigidbody2D>( true );
        debris.transform.rotation = Quaternion.identity;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        debris.SetDebrisSize( size );
        debris.ReadyToPush += PushObstacle;
    }
    private void SpawnWallSpike(int side, int width)
    {
        var spike = pool.GetComponentInChildren<SpikeObstacle>(true);
        if(!spike)
            spike = Instantiate( wallSpikePrefab ).GetComponent<SpikeObstacle>();

        spike.transform.SetParent( transform );
        if (side < 0)
        {
            spike.transform.position = leftSpawner.position + new Vector3(0, width);
            spike.transform.localScale = new Vector3( 1f, 1f, 1f );
        }
        else
        {
            spike.transform.position = rightSpawner.position + new Vector3(0, width);
            spike.transform.localScale = new Vector3( -1f, 1f, 1f );
        }

        spike.gameObject.SetActive( true );
        spike.SetSpikesWidth( width );
        spike.ReadyToPush += PushObstacle;
    }

    private void PushObstacle(Spawnable obstacle)
    {
        obstacle.gameObject.SetActive( false );
        obstacle.transform.SetParent( pool );
        obstacle.ReadyToPush -= PushObstacle;
    }
}

public enum Obstacles { WallSpikes, Debris};
