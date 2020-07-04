using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.ParticleSystem;

public class PlayerController : MonoBehaviour
{
    public static event Action<int> WallBounceEvent;
    public static event Action<float, float> MomentumChangedEvent;
    public static event Action ObstacleCollisionEvent;
    public static event Action PickupEvent;
    public static event Action<int> ShieldCountChangedEvent;

    public static PlayerController instance;

    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private TentacleManager tentaclesManager;
    [SerializeField]
    private GameObject shield;
    [SerializeField]
    private ParticleSystem gainSporeParticles;
    [SerializeField]
    private ParticleSystem loseSporeParticles;
    [SerializeField]
    private LayerMask collisionMask;
    [SerializeField]
    private TrailRenderer trail;
    [SerializeField]
    private LineRenderer tentacleRenderer;
    [SerializeField]
    private SpringJoint2D tentaclejoint;
    [SerializeField]
    private Transform tentacleAnchor;

    [Space(20f)]
    [SerializeField]
    private bool tentacleRushAvailable;
    [SerializeField]
    private bool massOfTentaclesAvailable;
    [SerializeField]
    private float tentacleRange = 10f;
    [SerializeField]
    private float tentacleLaunchTime = 0.5f;
    [SerializeField]
    private float obstacleVelocityPenalty = 0.75f;
    [SerializeField]
    private float tentacleCooldown;
    [SerializeField]
    private float tentacleAngleMin = 0f;
    [SerializeField]
    private float tentacleAngleMax = 45f;
    [SerializeField]
    private float followPointerMod = 10f;
    [SerializeField]
    private float maxFreeVelocity = 20f;
    [SerializeField]
    private float massOfTentaclesCost = 0.2f;
    [SerializeField]
    private float momentumPenaltyOnDamage = 0.2f;
    [SerializeField]
    private float motTime = 5f;
    [SerializeField]
    private float tentacleRushCost = 0.1f;
    [SerializeField]
    private float motStartRange = 3f;
    [SerializeField]
    private Vector2 startingVelocity = new Vector2( 20, 10 );
    [SerializeField]
    private Vector2 tentacleDirection = new Vector2( 1f, 1.33f );
    [SerializeField]
    private Vector2 maxVelocity = new Vector2(10f, 50f);
    [SerializeField]
    private Vector2 minVelocity = new Vector2( 15f, 1f );

	public bool TentacleAvailable { get; private set; }
	public bool TentacleConnected { get; private set; }
    public bool MOTAvailable => massOfTentaclesAvailable;
    public bool MassOfTentacles => momentum >= massOfTentaclesCost;
	public float Momentum { get => momentum;
        set 
        {
            momentum = Mathf.Clamp( value, 0f, 1f );
            MomentumChangedEvent?.Invoke( momentum, 1f );
        } 
    }

    public Vector2 RuntimeMinVelocity { get => Vector2.Lerp( minVelocity, maxVelocity, momentum ); }
    public bool TentacleLaunching { get; private set; }

    public MOTController motController;

    private List<Pickup> pickupConsumedFrameData = new List<Pickup>();
    private List<float> momentumFrameData = new List<float>();
    private List<bool> massOfTentaclesFrameData = new List<bool>();
    private List<TentacleFrameData> tentacleFrameData = new List<TentacleFrameData>();
    private List<DynamicFrameData> frameData = new List<DynamicFrameData>();
    private List<TrailFrameData> trailFrameData = new List<TrailFrameData>();

    private InputManager inputManager;
    private Animator animator;
	private RaycastHit2D[] tempPhysicsCastHits = new RaycastHit2D[10];
    private Vector3[] tempPos = new Vector3[2];
    private Vector3[] tempPos2 = new Vector3[100];
    private Vector3[] lastTrailPos;
    private float tentacleCooldownTimer;
    private int shieldCount;
    private bool invulnerable;
    private bool tentacleRush;
    private bool massOfTentacles;
    private float motTimer;
    private float rmotTime;
    private bool rewindingTime;
    private float momentum;

    private void Awake()
    {
        instance = this;
        GameManager.RewindTimeInProgressEvent += OnRewindTimeEvent;
        Input.simulateMouseWithTouches = false;
        inputManager = new InputManager();
        InputManager.PointerFirstheld += OnPointerFirstheld;
        InputManager.PointerUp += OnPointerUp;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        motController = FindObjectOfType<MOTController>();
        if (motController)
        {
            motController.ControllerPressed += OnMOTControllerPressed;
            motController.ControllerReleased += StopMassOfTentacles;
        }
    }

    private void Start()
    {
        rb.velocity = startingVelocity;
        rmotTime = motTime;
    }

    private void OnDestroy()
    {
        GameManager.RewindTimeInProgressEvent -= OnRewindTimeEvent;
        InputManager.PointerFirstheld -= OnPointerFirstheld;
        InputManager.PointerUp -= OnPointerUp;
    }
    private void OnRewindTimeEvent( bool inProgress )
    {
        if (inProgress)
        {
            rewindingTime = true;
            rb.simulated = false;
        }
        else
        {
            rewindingTime = false;
            rb.simulated = true;
            tentacleRenderer.enabled = false;
            if (massOfTentacles)
            {
                massOfTentacles = false;
                tentaclesManager?.QueueRetractAll();
                trail.enabled = true;
            }
        }
    }

    void Update()
    {
        if (rewindingTime || GameManager.GamePaused)
        {
            return;
        }

        if (massOfTentacles)
        {
            motTimer += Time.deltaTime;
            if (motTimer >= rmotTime)
            {
                rmotTime *= 0.5f;
                motTimer = 0f;
                Momentum -= massOfTentaclesCost;
                if (Momentum <= 0f)
                {
                    rmotTime = motTime;
                    motTimer = 0f;
                    StopMassOfTentacles();
                }
            }
        }
        else
        {
            motTimer = 0f;
            rmotTime = motTime;
        }
        inputManager?.ProcessInput();
        if (tentacleCooldownTimer > 0f && !tentacleRush)
            tentacleCooldownTimer -= Time.deltaTime;
        if (!TentacleAvailable && tentacleCooldownTimer <= 0f)
            SetTentacleAvailable( true );

        if (!GameManager.IsGameOver && !GameManager.Deathmatch && transform.position.y >= GameManager.WinningDistance)
        {
            GameManager.GameOver( true );
        }
    }

    private void RecordFrame()
    {
        if (frameData.Count > Mathf.Round( GameManager.RewindTime / Time.fixedDeltaTime ))
        {
            frameData.RemoveAt( frameData.Count - 1 );
            tentacleFrameData.RemoveAt( tentacleFrameData.Count - 1 );
            trailFrameData.RemoveAt( trailFrameData.Count - 1 );
            massOfTentaclesFrameData.RemoveAt( massOfTentaclesFrameData.Count - 1 );
            momentumFrameData.RemoveAt( momentumFrameData.Count - 1 );
            pickupConsumedFrameData.RemoveAt( pickupConsumedFrameData.Count - 1 );
        }
        frameData.Insert( 0, new DynamicFrameData(rb.position, rb.velocity));

        int n = trail.GetPositions( tempPos2 );
        trailFrameData.Insert( 0, new TrailFrameData(n, tempPos2, trail.enabled ) );

        tentacleRenderer.GetPositions( tempPos );
        tentacleFrameData.Insert( 0, new TentacleFrameData( tentacleRenderer.enabled, tempPos) );

        massOfTentaclesFrameData.Insert( 0, massOfTentacles );

        momentumFrameData.Insert( 0, Momentum );

        pickupConsumedFrameData.Insert( 0, null );
    }

    private void RewindFrame()
    {
        if (frameData.Count > 0)
        {
            transform.position = frameData[0].position;
            rb.velocity = frameData[0].velocity;

            lastTrailPos = trailFrameData[0].positions;
            trail.enabled = trailFrameData[0].enabled;

            tentacleRenderer.enabled = tentacleFrameData[0].enabled;
            tentacleRenderer.SetPositions( tentacleFrameData[0].positions );

            Momentum = momentumFrameData[0];

            if (massOfTentacles != massOfTentaclesFrameData[0])
            {
                massOfTentacles = massOfTentaclesFrameData[0];
                if (massOfTentacles)
                    tentaclesManager.QueueEnable();
                else
                    tentaclesManager.QueueRetractAll();
            }
            pickupConsumedFrameData[0]?.RespawnByRewindTime();

            pickupConsumedFrameData.RemoveAt( 0 );
            momentumFrameData.RemoveAt( 0 );
            trailFrameData.RemoveAt( 0 );
            frameData.RemoveAt( 0 );
            tentacleFrameData.RemoveAt( 0 );
            massOfTentaclesFrameData.RemoveAt( 0 );
        }
    }

    private void FollowPointer() 
    {
        if (motController != null && motController.gameObject.activeSelf)
        {
            Vector2 newVelocity = motController.ControllerAxis;
            rb.velocity += newVelocity;
            rb.velocity = Vector2.Lerp( rb.velocity, Vector2.zero, motController.Damping );
            rb.velocity = Vector2.ClampMagnitude( rb.velocity, maxFreeVelocity );
        }
        else
        {
            Vector2 pointerPos = Camera.main.ScreenToWorldPoint( InputManager.CurrentPos.Value );
            Vector2 controllerPos;
            controllerPos = rb.position;

            Vector2 direction = pointerPos - controllerPos;
            if (direction.magnitude <= 0.5f)
                direction = Vector2.zero;

            var newForce = direction * followPointerMod;// * Time.deltaTime;
            rb.velocity = newForce;
            rb.velocity = Vector2.ClampMagnitude( rb.velocity, maxFreeVelocity );
        }
    }

    private void StartMassOfTentacles()
    {
        if (!tentaclesManager)
        {
            Debug.LogError( "MassOfTentacles available, but tentacles manager not assigned" );
            return;
        }
        trail.enabled = false;
        tentaclesManager.QueueEnable();
        massOfTentacles = true;

        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;

        Momentum -= massOfTentaclesCost;
    }

    private void StopMassOfTentacles()
    {
        trail.enabled = true;
        massOfTentacles = false;
        if(tentaclesManager.gameObject.activeSelf)
            tentaclesManager.QueueRetractAll();

        rb.gravityScale = 1f;
    }

    private void LateUpdate()
    {
        if (rewindingTime && lastTrailPos != null)
            trail.SetPositions( lastTrailPos );
        if (invulnerable && !animator.GetCurrentAnimatorStateInfo( 0 ).IsName( "Damaged" ))
        {
            invulnerable = false;
        }
    }

    private void FixedUpdate()
    {
        if (rewindingTime)
        {
            RewindFrame();
            return;
        }
        else
        {
            RecordFrame();
        }
        if (massOfTentacles)
        {
            FollowPointer();
        }
        if (TentacleConnected)
        {
            tentacleRenderer.SetPosition( 0, rb.position );
        }
    }

    private Vector2 ClampVelocity(Vector2 velocity )
    {
        if (Mathf.Abs( velocity.x ) > maxVelocity.x)
        {
            velocity.x = Mathf.Sign( velocity.x ) * maxVelocity.x;
        }
        if (Mathf.Abs( velocity.x ) < RuntimeMinVelocity.x)
        {
            velocity.x = Mathf.Sign( velocity.x ) * RuntimeMinVelocity.x;
        }

        velocity.y = Mathf.Clamp( velocity.y, RuntimeMinVelocity.y, maxVelocity.y );
        return velocity;
    }

    IEnumerator LaunchTentacle(Vector2 targetPos, bool hit )
    {
        TentacleLaunching = true;
        float startTime = Time.time;
        float time = Time.time - startTime;
        float distanceBias = Mathf.Min(Vector2.Distance( targetPos, rb.position ) / tentacleRange, 1f);
        tentacleRenderer.enabled = true;
        while (time < tentacleLaunchTime * distanceBias)
        {
            var positions = new Vector3[] { rb.position, Vector2.Lerp(rb.position, targetPos, time / (tentacleLaunchTime * distanceBias) ) };
            tentacleRenderer.SetPositions( positions );

            yield return new WaitForEndOfFrame();
            time = Time.time - startTime ;
        }

        if (hit)
        {
            SetTentacleAvailable( false );
            var positions = new Vector3[] { rb.position, targetPos };
            tentacleRenderer.SetPositions( positions );
            TentacleConnected = true;
            tentacleAnchor.position = targetPos;
            tentaclejoint.enabled = true;
            SoundManager.PlaySound( SoundManager.Sound.TentacleSwing );

            if (Mathf.Sign(rb.velocity.x) == Mathf.Sign( (targetPos - rb.position).x))
            {
                rb.velocity = new Vector2( 0f, rb.velocity.magnitude );
            }
        }
        else
        {
            tentacleRenderer.enabled = false;
        }
        TentacleLaunching = false;
    }

    private void OnMOTControllerPressed(Vector2 pos)
    {
        if (massOfTentaclesAvailable && !massOfTentacles && momentum >= massOfTentaclesCost)
        {
            StartMassOfTentacles();
            return;
        }
    }

    private void OnPointerFirstheld( Vector3 pos )
    {
        if (massOfTentacles)
            return;
        if ((!motController || !motController.gameObject.activeSelf) && massOfTentaclesAvailable && !massOfTentacles && momentum >= massOfTentaclesCost)
        {
            Vector2 pointerPos = Camera.main.ScreenToWorldPoint( pos );
            Vector2 controllerPos = transform.position;
            float distance = Vector2.Distance( pointerPos, controllerPos );
            if (distance <= motStartRange)
            {
                StartMassOfTentacles();
                return;
            }
        }

        if (!TentacleAvailable)
        {
            if (tentacleRushAvailable && Momentum >= tentacleRushCost)
            {
                Momentum = Momentum - tentacleRushCost;
                tentacleRush = true;
            }
            else
            {
                tentacleRush = false;
                return;
            }
        }
        if (rb.velocity.x >= 0)
        {
            tentacleDirection.x = -1f;
        }
        else
        {
            tentacleDirection.x = 1f;
        }
        var mousePos = Camera.main.ScreenToWorldPoint( pos );
        var tentacleHit = GetRaycastHitPoint( mousePos.normalized, tentacleRange );
        tentacleRenderer.positionCount = 2;
        StartCoroutine( LaunchTentacle( tentacleHit ?? mousePos, tentacleHit.HasValue ) );
    }

    private void OnPointerUp( Vector3 pos )
    {
        if (massOfTentacles)
        {
            StopMassOfTentacles();
            return;
        }
        if (inputManager.PointerHeld && TentacleConnected)
        {
            ReleaseTentacle();
            SetTentacleAvailable( false );
        }
    }

    private void ReleaseTentacle()
    {
        TentacleConnected = false;
        tentacleRenderer.enabled = false;
        tentaclejoint.enabled = false;
        TentacleLaunching = false;
        StopCoroutine( "LaunchTentacle" );
    }

    private void SetTentacleAvailable(bool value)
    {
        tentacleCooldownTimer = value ? 0f : tentacleCooldown;
        TentacleAvailable = value;
        animator?.SetBool( "HideTentacle", !TentacleAvailable );
    }
    private void OnCollisionEnter2D( Collision2D collision )
    {
        if (massOfTentacles || rewindingTime)
        {
            return;
        }
        Vector2 newVelocity = -collision.relativeVelocity;

        if (inputManager.PointerHeld)
        {
            ReleaseTentacle();
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            WallBounceEvent?.Invoke( (int) Mathf.Sign( newVelocity.x ));
            newVelocity.x *= -1f;
            SoundManager.PlaySound( SoundManager.Sound.Bounce );
        }

        tentacleRush = false;
        SetTentacleAvailable( true );
        newVelocity = ClampVelocity( newVelocity );
        rb.velocity = newVelocity;
    }

    private void OnTriggerEnter2D( Collider2D collision )
    {
        if (GameManager.IsGameOver || rewindingTime)
            return;

        if (collision.CompareTag( "Lava" ))
        {
            GameManager.GameOver( false );
            return;
        }

        if (collision.gameObject.CompareTag( "Obstacle" ))
        {
            if (TentacleConnected)
            {
                ReleaseTentacle();
            }
            if (massOfTentacles)
            {
                StopMassOfTentacles();
            }
            SetTentacleAvailable( true );
            Vector2 newVelocity = collision.transform.parent.localScale.normalized;
            newVelocity *= rb.velocity.magnitude;
            rb.velocity = Vector2.zero;

            if (shieldCount > 0)
            {
                shieldCount--;
                SoundManager.PlaySound( SoundManager.Sound.Bounce );
            }
            else if(!invulnerable)
            {
                newVelocity *= obstacleVelocityPenalty;
                OnTakeDamage();
                loseSporeParticles.Play();
            }
            WallBounceEvent( (int) -Mathf.Sign(collision.transform.localScale.x) );
            rb.velocity = newVelocity;
            tentacleRush = false;
        }

        if (collision.CompareTag("Debris") || collision.CompareTag("FloatingCrystal"))
        {
            if (shieldCount > 0)
            {
                shieldCount--;
            }
            else if(!invulnerable)
            {
                if (TentacleConnected)
                {
                    ReleaseTentacle();
                    SetTentacleAvailable( false );
                    tentacleCooldownTimer *= 0.33f;
                }

                if (massOfTentacles)
                {
                    StopMassOfTentacles();
                }

                var v = rb.velocity;
                v.y *= obstacleVelocityPenalty;
                rb.velocity = v;

                OnTakeDamage();
                loseSporeParticles.Play();
            }
            tentacleRush = false;
        }

        ShieldCountChangedEvent?.Invoke( shieldCount );
        if (shieldCount > 0)
        {
            shield.SetActive( true );
        }
        else
        {
            shield.SetActive( false );
        }
    }

    public void AbsorbPickup(Pickup pickup)
    {
        if (rewindingTime)
            return;
        pickupConsumedFrameData.Insert( 0, pickup );
        SetTentacleAvailable( true );
        gainSporeParticles.Play();
        SoundManager.PlaySound( SoundManager.Sound.Pickup );
        PickupEvent?.Invoke();
    }

    private void OnTakeDamage()
    {
        Momentum = Momentum - momentumPenaltyOnDamage;
        ObstacleCollisionEvent?.Invoke();
        animator.SetTrigger( "Damaged" );
        SoundManager.PlaySound( SoundManager.Sound.ObstacleCollision );
        invulnerable = true;
    }

    private Vector2? GetRaycastHitPoint(Vector2 direction, float range)
    {
        if (direction.x >= 0)
        {
            float angle = Mathf.Lerp( tentacleAngleMax, tentacleAngleMin, (8f - rb.position.x) / 16f );
            direction = Quaternion.Euler( 0f, 0f, angle ) * Vector2.right;
        }
        else if (direction.x < 0)
        {
            float angle = Mathf.Lerp( tentacleAngleMax, tentacleAngleMin, (8f + rb.position.x) / 16f );
            direction = Quaternion.Euler( 0f, 0f, -angle ) * Vector2.left;
        }
        var hits = Physics2D.RaycastNonAlloc( rb.position, direction, tempPhysicsCastHits, range, collisionMask );
        if (hits > 0)
        {
            if (tempPhysicsCastHits[0].collider)
            {
                return tempPhysicsCastHits[0].point;
            }
        }
        return null;
    }
    public void BoostShield(int boost )
    {
        shieldCount++;
        shieldCount = Mathf.Clamp( shieldCount, 0, 3 );
        ShieldCountChangedEvent?.Invoke( shieldCount );
        if (shieldCount > 0)
        {
            shield.SetActive( true );
        }
        else
        {
            shield.SetActive( false );
        }
    }

    private void OnDrawGizmosSelected()
    {
        Color c = Color.green;
        Debug.DrawRay( rb.position, Quaternion.Euler( 0f, 0f, tentacleAngleMin ) * Vector2.right * tentacleRange, c );
        Debug.DrawRay( rb.position, Quaternion.Euler( 0f, 0f, -tentacleAngleMin ) * Vector2.left * tentacleRange, c );
        Debug.DrawRay( rb.position, Quaternion.Euler( 0f, 0f, tentacleAngleMax ) * Vector2.right * tentacleRange, c );
        Debug.DrawRay( rb.position, Quaternion.Euler( 0f, 0f, -tentacleAngleMax ) * Vector2.left * tentacleRange, c );
    }
}
