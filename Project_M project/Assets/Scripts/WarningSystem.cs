using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningSystem : MonoBehaviour
{
    [SerializeField]
    private GameObject leftWarning;
    [SerializeField]
    private GameObject rightWarning;
    [SerializeField]
    private GameObject middleWarning;

    private float lastMidPosY;
    private float lastLeftPosY;
    private float lastRightPosY;
    private void Start()
    {
        SpikeSpawnModule.WallOfSpikesSpawnedEvent += ObstacleSpawner_WallOfSpikesSpawnedEvent;
        DoubleSpikeWallModule.WallOfSpikesSpawnedEvent += ObstacleSpawner_WallOfSpikesSpawnedEvent;
        FloatingCrystalModule.FloatingCrystalsSpawnedEvent += FloatingCrystalModule_FloatingCrystalsSpawnedEvent;
    }

    private void FloatingCrystalModule_FloatingCrystalsSpawnedEvent( float yPos )
    {
        middleWarning.SetActive( true );
        lastMidPosY = yPos;
    }

    private void OnDestroy()
    {
        SpikeSpawnModule.WallOfSpikesSpawnedEvent -= ObstacleSpawner_WallOfSpikesSpawnedEvent;
        DoubleSpikeWallModule.WallOfSpikesSpawnedEvent -= ObstacleSpawner_WallOfSpikesSpawnedEvent;
        FloatingCrystalModule.FloatingCrystalsSpawnedEvent -= FloatingCrystalModule_FloatingCrystalsSpawnedEvent;
    }
    private void ObstacleSpawner_WallOfSpikesSpawnedEvent( int side, float yPos )
    {
        if (side < 0)
        {
            lastLeftPosY = yPos;
            leftWarning.SetActive( true );
        }
        else
        {
            lastRightPosY = yPos;
            rightWarning.SetActive( true );
        }
    }

    private void Update()
    {
        if (Camera.main.transform.position.y + Camera.main.orthographicSize >= lastLeftPosY)
        {
            leftWarning.SetActive( false );
        }
        if (Camera.main.transform.position.y + Camera.main.orthographicSize >= lastRightPosY)
        {
            rightWarning.SetActive( false );
        }
        if (Camera.main.transform.position.y + Camera.main.orthographicSize >= lastMidPosY)
        {
            middleWarning.SetActive( false );
        }
    }
}
