using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class SpawningSystem : MonoBehaviour
{
    public static SpawningSystem instance;
    public static Transform Pool { get => instance.pool; }

    [SerializeField]
    private GameObject pickupPrefab;
    [SerializeField]
    private Transform leftSpawner;
    [SerializeField]
    private Transform midSpawner;
    [SerializeField]
    private Transform rightSpawner;
    [SerializeField]
    private List<PickupEffect> pickupEffects;

    [SerializeField]
    private float pickupSpawnTimerMin = 3f;
    [SerializeField]
    private float pickupSpawnTimerMax = 6f;
    [SerializeField]
    private float pickupMinDistance = 5f;
    [SerializeField]
    private float midSpawnerLeftEdge = -6f;
    [SerializeField]
    private float midSpawnerRightEdge = 6f;
    [SerializeField]
    private float spawnableDespawningDistance = 150f;

    public float LasBigSpawnTime { get; set; }
    public ISpawnModule LastSpawnedModuleM { get; set; }
    public ISpawnModule LastSpawnedModuleL { get; set; }
    public ISpawnModule LastSpawnedModuleR { get; set; }
	public Transform MidSpawner { get => midSpawner; }
    public Transform LeftSpawner { get => leftSpawner; }
    public Transform RightSpawner { get => rightSpawner; }
	public int LeftWallBounces { get; set; }
	public int RightWallBounces { get; set; }
    public static float DespawningDistance { get => instance ? instance.spawnableDespawningDistance : 0f; }

	private List<ISpawnModule> modules;
    private Transform pool;

    private float lastPickupPosY;
    private float pickupHalfSize;
    private float nextPickupSpawnTime;
    private bool rewinding;
    private float rewindStartHeight;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        if (pickupPrefab)
        {
            pickupHalfSize = pickupPrefab.GetComponentInChildren<SpriteRenderer>().size.y / 2f;
        }
        GameManager.RewindTimeInProgressEvent += OnRewindTime;
        PlayerController.WallBounceEvent += OnWallBounce;
        pool = GameObject.FindWithTag( "SpawnablesPool" ).transform;
        var constraintSource = new ConstraintSource();
        constraintSource.sourceTransform = Camera.main.transform;
        constraintSource.weight = 1f;

        leftSpawner.GetComponent<PositionConstraint>().SetSource( 0, constraintSource );
        rightSpawner.GetComponent<PositionConstraint>().SetSource( 0, constraintSource );
        midSpawner.GetComponent<PositionConstraint>().SetSource( 0, constraintSource );

        modules = new List<ISpawnModule>(GetComponents<ISpawnModule>());
        modules.Sort( ( ISpawnModule m1, ISpawnModule m2 ) => m1.Priority > m2.Priority ? -1 : 1 );

        StartCoroutine( SpawningCycle() );
    }

    private void OnDestroy()
    {
        GameManager.RewindTimeInProgressEvent -= OnRewindTime;
    }
    private void OnRewindTime( bool inProgress )
    {
        rewinding = inProgress;
        if(inProgress) rewindStartHeight = midSpawner.position.y;
    }

    private IEnumerator SpawningCycle()
    {
        yield return new WaitForSeconds( 1f );
        while (true)
        {
            if (!rewinding && rewindStartHeight < midSpawner.position.y)
            {
                foreach (var m in modules)
                {
                    if (m.ReadyToSpawn() && !rewinding && rewindStartHeight < midSpawner.position.y)
                    {
                        yield return StartCoroutine( m.SpawnRoutine() );
                        break;
                    }
                }
            }
            yield return null;
        }
    }

    void Update()
    {
        if (!pickupPrefab || rewinding || rewindStartHeight > midSpawner.position.y)
            return;
        if (PlayerController.instance.Momentum < 1f &&
            Time.time > nextPickupSpawnTime &&
            midSpawner.transform.position.y - lastPickupPosY >= pickupMinDistance)
        {
            nextPickupSpawnTime = Time.time + UnityEngine.Random.Range( pickupSpawnTimerMin, pickupSpawnTimerMax );

            SpawnPickup();

            lastPickupPosY = midSpawner.position.y + pickupHalfSize;
        }
    }

    private Vector2 GetRandomMidSpawnPos()
    {
        return new Vector2( UnityEngine.Random.Range( midSpawnerLeftEdge, midSpawnerRightEdge ), midSpawner.transform.position.y );
    }
    private void SpawnPickup()
    {
        var pickup = pool.GetComponentInChildren<Pickup>( true );
        if (!pickup)
            pickup = Instantiate( pickupPrefab ).GetComponent<Pickup>();

        pickup.transform.SetParent( transform );
        int r = UnityEngine.Random.Range( 0, pickupEffects.Count );
        pickup.SetEffect( pickupEffects[r] );
        pickup.transform.position = GetRandomMidSpawnPos();
        pickup.gameObject.SetActive( true );
    }
    private void OnWallBounce(int side )
    {
        if (side == -1)
        {
            RightWallBounces = 0;
            LeftWallBounces ++;
        }
        else
        {
            LeftWallBounces = 0;
            RightWallBounces ++;
        }
    }

    public Spawnable GetFromPool<T>() where T : Spawnable
    {
        return pool.GetComponentInChildren<T>(true);
    }
}