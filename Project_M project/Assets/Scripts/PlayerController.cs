using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.ParticleSystem;

public class PlayerController : MonoBehaviour
{
    public static event Action<int> WallBounceEvent;

    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private TextLable slimeSporeLable;
    [SerializeField]
    private ParticleSystem gainSporeParticles;
    [SerializeField]
    private ParticleSystem loseSporeParticles;
    [SerializeField]
    private LayerMask collisionMask;
    [SerializeField]
    private float pointerHoldTime = 0.1f;
    [SerializeField]
    private LineRenderer tentacleRenderer;
    [SerializeField]
    private SpringJoint2D tentaclejoint;
    [SerializeField]
    private Transform tentacleAnchor;
    [SerializeField]
    private float tentacleRange = 10f;
    [SerializeField]
    private float tentacleLaunchTime = 0.5f;
    [SerializeField]
    private float averageVelocityTime = 5f;
    [SerializeField]
    private float obstacleVelocityPenalty = 0.75f;
    [SerializeField]
    private Vector2 startingVelocity = new Vector2( 20, 10 );
    [SerializeField]
    private Vector2 tentacleDirection = new Vector2( 1f, 1.33f );
    [SerializeField]
    private Vector2 maxVelocity = new Vector2(10f, 50f);
    [SerializeField]
    private Vector2 minVelocity = new Vector2( 15f, 1f );
    [SerializeField]
    private float tentacleCooldown;
    [SerializeField]
    private int sporePerPickup = 2;
    [SerializeField]
    private int sporePenalty = 1;
    [SerializeField]
    private int slimeSporesToWin = 20;
    [SerializeField]
    private float tentacleAngleMin = 0f;
    [SerializeField]
    private float tentacleAngleMax = 45f;

    private RaycastHit2D[] tempPhysicsCastHits = new RaycastHit2D[10];
    private bool tentacleConnected;
    private bool tentacleAvailable;
    private float tentacleCooldownTime;
    private InputManager inputManager;
    private float averageVerticalVelocity;
    private ContactPoint2D[] contacts = new ContactPoint2D[10];
    private int slimeSpores;
    private bool gameOver;
    private Collider2D insideObstacle;
    private Animator animator;

    private void Start()
    {
        if (Application.isEditor)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 30;
        }
        Input.simulateMouseWithTouches = false;
        inputManager = new InputManager( pointerHoldTime );
        inputManager.PointerDown += OnPointerDown;
        inputManager.PointerFirstheld += OnPointerFirstheld;
        inputManager.PointerUp += OnPointerUp;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.AddForce( startingVelocity, ForceMode2D.Impulse );
    }
    void Update()
    {
        inputManager.ProcessInput();
        if (!tentacleAvailable && Time.time >= tentacleCooldownTime)
            TentacaleAvailable( true );
    }

    private void FixedUpdate()
    {
        if (tentacleConnected)
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
        if (Mathf.Abs( velocity.x ) < minVelocity.x)
        {
            velocity.x = Mathf.Sign( velocity.x ) * minVelocity.x;
        }

        velocity.y = Mathf.Clamp( velocity.y, minVelocity.y, maxVelocity.y );
        return velocity;
    }

    private void OnPointerDown(Vector3 pos )
    {
    }

    IEnumerator LaunchTentacle(Vector2 targetPos, bool hit )
    {
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
            TentacaleAvailable( false );
            tentacleCooldownTime = Time.time + tentacleCooldown;
            var positions = new Vector3[] { rb.position, targetPos };
            tentacleRenderer.SetPositions( positions );
            tentacleConnected = true;
            tentacleAnchor.position = targetPos;
            tentaclejoint.enabled = true;

            if (Mathf.Sign(rb.velocity.x) == Mathf.Sign( (targetPos - rb.position).x))
            {
                rb.velocity = new Vector2( 0f, rb.velocity.magnitude );
            }
        }
        else
        {
            tentacleRenderer.enabled = false;
        }
    }

    private void OnPointerFirstheld( Vector3 pos )
    {
        if (!tentacleAvailable)
            return;

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
        if (inputManager.PointerHeld)
        {
            ReleaseTentacle();
        }
    }

    private void ReleaseTentacle()
    {
        tentacleConnected = false;
        tentacleRenderer.enabled = false;
        tentaclejoint.enabled = false;
        StopCoroutine( "LaunchTentacle" );
    }
    private void OnCollisionEnter2D( Collision2D collision )
    {
        Vector2 newVelocity = -collision.relativeVelocity;

        if (inputManager.PointerHeld)
        {
            ReleaseTentacle();
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            WallBounceEvent?.Invoke( (int) Mathf.Sign( newVelocity.x ));
            newVelocity.x *= -1f;
        }

        TentacaleAvailable( true );
        newVelocity = ClampVelocity( newVelocity );
        rb.velocity = newVelocity;
    }

    IEnumerator GameOver(bool victory )
    {
        Debug.Log( "GameOver sequence start" );
        yield return new WaitForSeconds( 0.25f );

        Debug.Log( "GameOver sequence done" );
        if (victory)
        {
            GameManager.Victory();
        }
        else
        {
            GameManager.GameOver();
        }
    }

    private void TentacaleAvailable(bool value)
    {
        tentacleAvailable = value;
        animator?.SetBool( "HideTentacle", !tentacleAvailable );
    }
    private void OnTriggerExit( Collider other )
    {
        if (insideObstacle && other == insideObstacle)
        {
            insideObstacle = null;
        }
    }
    private void OnTriggerEnter2D( Collider2D collision )
    {
        if (gameOver)
            return;

        if (!insideObstacle)
        {
            insideObstacle = collision;
        }

        if (collision.CompareTag( "Lava" ))
        {
            gameOver = true;
            Debug.Log( "GameOver" );
            StartCoroutine( GameOver( false ) );
            return;
        }

        if (collision.gameObject.CompareTag( "Obstacle" ))
        {
            if (insideObstacle == collision)
            {
                return;
            }
            if (inputManager.PointerHeld)
            {
                ReleaseTentacle();
                TentacaleAvailable( true );
            }
            Vector2 newVelocity = rb.velocity;
            rb.velocity = Vector2.zero;
            collision.GetContacts( contacts );
            newVelocity.x = -newVelocity.x;
            newVelocity *= obstacleVelocityPenalty;
            rb.velocity = newVelocity;
            slimeSpores -= sporePenalty;

            OnTakeDamage();
            loseSporeParticles.Play();
        }

        if (collision.CompareTag("Debris"))
        {
            if (inputManager.PointerHeld)
            {
                ReleaseTentacle();
            }

            var v = rb.velocity;
            v.y *= obstacleVelocityPenalty;
            rb.velocity = v;
            slimeSpores -= sporePenalty;

            OnTakeDamage();
            loseSporeParticles.Play();
        }
        else if (collision.CompareTag( "SlimeSpore" ))
        {
            TentacaleAvailable( true );
            slimeSpores += sporePerPickup;

            var s = collision.GetComponentInParent<Spawnable>();
            s?.ReturnToPool();
            gainSporeParticles.Play();
        }

        slimeSpores = Mathf.Clamp( slimeSpores, 0, slimeSporesToWin );
        slimeSporeLable.UpdateText( ((100 * slimeSpores) / slimeSporesToWin).ToString() );

        if (slimeSpores == slimeSporesToWin)
        {
            StartCoroutine( GameOver( true ) );
            return;
        }
    }
    private void OnTakeDamage()
    {
        animator.SetTrigger( "Damaged" );
    }
    private void SetTimeScale(float scale )
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
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

    private void OnDrawGizmosSelected()
    {
        Color c = Color.green;
        Debug.DrawRay( rb.position, Quaternion.Euler( 0f, 0f, tentacleAngleMin ) * Vector2.right * tentacleRange, c );
        Debug.DrawRay( rb.position, Quaternion.Euler( 0f, 0f, -tentacleAngleMin ) * Vector2.left * tentacleRange, c );
        Debug.DrawRay( rb.position, Quaternion.Euler( 0f, 0f, tentacleAngleMax ) * Vector2.right * tentacleRange, c );
        Debug.DrawRay( rb.position, Quaternion.Euler( 0f, 0f, -tentacleAngleMax ) * Vector2.left * tentacleRange, c );
    }
}
