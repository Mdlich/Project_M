using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField]
    private Transform staticPool;
    [SerializeField]
    private Transform dynamicPool;
    [SerializeField]
    private Transform staticActive;
    [SerializeField]
    private Transform dynamicActive;
    [SerializeField]
    private GameObject wallSpike;
    [SerializeField]
    private Transform leftSpawner;
    [SerializeField]
    private Transform rightSpawner;
    [SerializeField]
    private float minSpawnDistance = 2f;
    [SerializeField]
    private float maxSpawnDistance = 10f;
    [SerializeField]
    private float minSpikesWidth = 2f;
    [SerializeField]
    private float maxSpikesWidth = 10f;

    private float lastLeftObstaclePosY;
    private float lastRightObstaclePosY;
    private float obstacleWidthRMod;
    private float obstacleSpawnDistance;
    private List<GameObject> staticPoolList = new List<GameObject>();
    private List<GameObject> staticActiveList = new List<GameObject>();
    void Start()
    {
        obstacleWidthRMod = Random.Range( 0, 1f );
        obstacleSpawnDistance = Random.Range( minSpawnDistance, maxSpawnDistance );
        foreach (var obstacle in staticPool.GetComponentsInChildren<SpikeController>())
        {
            obstacle.gameObject.SetActive( false );
            staticPoolList.Add( obstacle.gameObject );
        }
    }

    // Update is called once per frame
    void Update()
    {
        var currentSpikesWidth = Mathf.Lerp( minSpikesWidth, maxSpikesWidth, obstacleWidthRMod );
        if (leftSpawner.transform.position.y - lastLeftObstaclePosY > obstacleSpawnDistance + currentSpikesWidth / 2f)
        {
            obstacleWidthRMod = Random.Range( 0, 1f );
            obstacleSpawnDistance = Random.Range( minSpawnDistance, maxSpawnDistance );
            SpawnWallSpike( -1, currentSpikesWidth );
            lastLeftObstaclePosY = leftSpawner.position.y + currentSpikesWidth / 2f;
        }

        currentSpikesWidth = Mathf.Lerp( minSpikesWidth, maxSpikesWidth, obstacleWidthRMod );
        if (rightSpawner.transform.position.y - lastRightObstaclePosY > obstacleSpawnDistance + currentSpikesWidth / 2f)
        {
            obstacleWidthRMod = Random.Range( 0, 1f );
            obstacleSpawnDistance = Random.Range( minSpawnDistance, maxSpawnDistance );
            SpawnWallSpike( 1, currentSpikesWidth );
            lastRightObstaclePosY = rightSpawner.position.y + currentSpikesWidth / 2f;
        }

        for (int i = staticActiveList.Count - 1; i >= 0; i--)
        {
            var obstacle = staticActiveList[i];
            if (obstacle.transform.position.y < Camera.main.transform.position.y && !obstacle.GetComponentInChildren<SpriteRenderer>().isVisible)
            {
                obstacle.gameObject.SetActive( false );
                obstacle.transform.SetParent( staticPool );
                staticActiveList.Remove( obstacle );
                staticPoolList.Add( obstacle );
            }
        }
    }
    private void SpawnWallSpike(int side, float width)
    {
        var spike = staticPoolList[0].GetComponent<SpikeController>();
        if(!spike)
            spike = Instantiate( wallSpike ).GetComponent<SpikeController>();
        spike.transform.SetParent( staticActive );
        staticActiveList.Add( spike.gameObject );
        staticPoolList.Remove( spike.gameObject );
        if (side < 0)
        {
            spike.transform.position = leftSpawner.position;
            spike.transform.localScale = new Vector3( 1f, 1f, 1f );
        }
        else
        {
            spike.transform.position = rightSpawner.position;
            spike.transform.localScale = new Vector3( -1f, 1f, 1f );
        }
        spike.gameObject.SetActive( true );
        spike.SetSpikesWidth( width );
    }

    private void PushObstacle(GameObject obstacle)
    {

    }
}

public enum Obstacles { WallSpikes, Debris};
