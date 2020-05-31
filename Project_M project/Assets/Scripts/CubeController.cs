using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class CubeController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private ArrowController arrow;
    [SerializeField]
    private CircleRenderer inRangeIndicator;
    [SerializeField]
    private CircleRenderer perfectTapIndicator;
    [SerializeField]
    private TextLable momentumLable;
    [SerializeField]
    private ParticleSystem momentumParticles;
    [SerializeField]
    private Color boost2Color;
    [SerializeField]
    private Color boost1Color;
    [SerializeField]
    private LayerMask collisionMask;
    [SerializeField]
    private int momentumIncrement = 5;
    [SerializeField]
    private int maxMomentum = 200;
    [SerializeField]
    [Tooltip( "amount of momentum to reduce each tick" )]
    private int momentumDecayRate = 1;
    [SerializeField]
    private float forceMod = 1f;
    [SerializeField]
    private float pointerHoldTime = 0.1f;
    [SerializeField]
    private float sensorRange = 1f;
    [SerializeField]
    private float timeSlowMod = 0.1f;
    [SerializeField]
    private float goodReactionRadius = 3f;
    [SerializeField]
    [Tooltip("time between momentum decay ticks")]
    private float momentumDecayTime = 2f;
    [SerializeField]
    private LineRenderer tentacleRenderer;
    [SerializeField]
    private SpringJoint2D tentaclejoint;
    [SerializeField]
    private Transform tentacaleAnchor;
    [SerializeField]
    private float tentacleRange = 10f;
    [SerializeField]
    private float tentacleLaunchTime = 0.5f;

    private RaycastHit2D[] tempPhysicsCastHits = new RaycastHit2D[10];
    private Vector2 lastCollisionPoint;
    private Vector2 lastCollisionCenter;
    private Vector2? closestCollision;
    private Vector2? closestCollisionCenter;
    private bool tentacleConnected;
    private bool chargingBounce;
    private bool inTapRange;
    private bool inPerfectTapRange;
    private bool collisionHandled;
    private InputManager inputManager;
    private int momentum = 0;
    private float indicatorRadius;
    private float colliderRadius = 0f;
    private float momentumDecayTimer;

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
        var collider = GetComponent<CircleCollider2D>();
        if (collider)
        {
            colliderRadius = collider.radius;
        }
        rb = GetComponent<Rigidbody2D>();
        rb.AddForce( new Vector2( 20, 10 ), ForceMode2D.Impulse );
    }
    void Update()
    {
        inputManager.ProcessInput();
        /*GetClosestCollision();
        indicatorRadius = goodReactionRadius * (1f - 0.5f * momentum / (maxMomentum));
        var perfectRangeRadius = 0.33f * indicatorRadius * (1f - 0.5f * momentum / (maxMomentum));
        if (closestCollision.HasValue || Vector2.Distance( lastCollisionCenter, rb.position ) <= indicatorRadius)
        {
            var contactCenter = closestCollisionCenter.HasValue ? closestCollisionCenter.Value : lastCollisionCenter;
            var distanceToContactCenter = Vector2.Distance( contactCenter, rb.position );
            inTapRange = distanceToContactCenter <= indicatorRadius;
            inPerfectTapRange = distanceToContactCenter <= perfectRangeRadius;
            var distanceToIndicator = distanceToContactCenter - indicatorRadius;
            perfectTapIndicator.transform.position = inRangeIndicator.transform.position = contactCenter;
            if(distanceToIndicator > 0f)
            {
                inRangeIndicator.SetRadiusAndOpacity( indicatorRadius, Mathf.Clamp( indicatorRadius / distanceToIndicator, 0f, 1f ));
                perfectTapIndicator.SetRadiusAndOpacity( perfectRangeRadius, Mathf.Clamp( indicatorRadius / distanceToIndicator, 0f, 1f ) );
            }
            inRangeIndicator.gameObject.SetActive(true);
        }
        else
        {
            inPerfectTapRange = false;
            inTapRange = false;
            inRangeIndicator.gameObject.SetActive( false );
            collisionHandled = false;
        }*/

        if (tentacleConnected)
        {
            tentacleRenderer.SetPosition( 0, rb.position );
        }

        /*if (inputManager.PointerHeld && inputManager.Delta.HasValue && arrow)
        {
            arrow.SetRot( inputManager.Delta.Value );
            arrow.gameObject.SetActive( true );
        }
        else if (arrow)
        {
            arrow.gameObject.SetActive( false );
        }*/

        momentumDecayTimer += Time.deltaTime;
        if (momentumDecayTimer >= momentumDecayTime)
        {
            momentumDecayTimer = 0f;
            momentum -= momentumDecayRate * (1 + 3 * momentum / maxMomentum);
            momentum = Mathf.Clamp( momentum, 0, maxMomentum );
            momentumLable.UpdateText( momentum.ToString() );
        }
    }

    private void OnPointerDown(Vector3 pos )
    {

    }

    IEnumerator launchTentacle(Vector2 targetPos, bool hit )
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
            var positions = new Vector3[] { rb.position, targetPos };
            tentacleRenderer.SetPositions( positions );
            tentacleConnected = true;
            tentacaleAnchor.position = targetPos;
            tentaclejoint.enabled = true;
        }
        else
        {
            tentacleRenderer.enabled = false;
        }

    }

    private void OnPointerFirstheld( Vector3 pos )
    {
        Vector2 tentacleDirection;
        if (rb.velocity.x >= 0)
        {
            tentacleDirection = new Vector2( -1f, 1.5f );
        }
        else
        {
            tentacleDirection = new Vector2( 1f, 1.5f );
        }
        //var tentacleDirection = (Camera.main.ScreenToWorldPoint( pos ) - transform.position).normalized;
        var tentacleHit = GetRaycastHitPoint( tentacleDirection, tentacleRange );
        tentacleRenderer.positionCount = 2;
        StartCoroutine( launchTentacle( tentacleHit ?? Camera.main.ScreenToWorldPoint( pos ), tentacleHit.HasValue ) );

        /*var positions = new Vector3[] {rb.position, tentacaleHit.Value};
        tentacaleAnchor.position = tentacaleHit.Value;
        tentacleRenderer.SetPositions( positions );
        tentacleRenderer.enabled = true;
        if (!tentacaleHit.HasValue) return;
        tentaclejoint.enabled = true;*/
        //SetTimeScale( timeSlowMod );
    }

    private void OnPointerUp( Vector3 pos )
    {
        if (inputManager.PointerHeld)
        {
            tentacleConnected = false;
            tentacleRenderer.enabled = false;
            tentaclejoint.enabled = false;
            StopAllCoroutines();
        }
        /*if (inputManager.PointerHeld)
        {
            SetTimeScale( 1f );
            rb.velocity = Vector2.zero;
            rb.AddForce( inputManager.Delta.Value * forceMod, ForceMode2D.Impulse );
            return;
        }
        else if(inTapRange && !collisionHandled)
        {
            HandleTap( closestCollisionCenter.HasValue ? closestCollisionCenter.Value : lastCollisionCenter);
        }*/
    }

    /*private void HandleTap(Vector2 collisionPoint)
    {
        bool perfectTap = false;
        collisionHandled = true;

        if (inPerfectTapRange)
        {
            momentum += 2 * momentumIncrement;
            perfectTap = true;
        }
        else
        {
            momentum += momentumIncrement;
        }

        momentum = Mathf.Clamp( momentum, 0, maxMomentum );
        momentumLable.UpdateText( momentum.ToString() );

        if (momentumParticles)
        {
            momentumParticles.transform.position = collisionPoint;
            var particlesMain = momentumParticles.main;
            if (perfectTap)
                particlesMain.startColor = boost2Color;
            else
                particlesMain.startColor = boost1Color;
            momentumParticles.Play();
        }
    }*/
    private void OnCollisionEnter2D( Collision2D collision )
    {
        if (collisionMask != (collisionMask | (1 << collision.gameObject.layer)))
            return;

        if (inputManager.PointerHeld)
        {
            tentacleConnected = false;
            tentacleRenderer.enabled = false;
            tentaclejoint.enabled = false;
            StopAllCoroutines();
        }

        Vector2 newVelocity;
        if (collision.gameObject.CompareTag( "RightWall" ))
            newVelocity = Vector2.left;
        else if (collision.gameObject.CompareTag( "LeftWall" ))
            newVelocity = Vector2.right;
        else
            newVelocity = collision.GetContact( 0 ).normal;

        if (newVelocity.y == 0f)
            newVelocity.y = 1f;
        if (newVelocity.x == 0)
            newVelocity.x = 0.33f;
        var momentumRatio = (float)momentum / (maxMomentum);
        newVelocity *= new Vector2( 1f, 0.66f + momentumRatio * momentumRatio);
        rb.velocity = newVelocity * forceMod;
        lastCollisionPoint = collision.GetContact( 0 ).point;
        lastCollisionCenter = lastCollisionPoint + colliderRadius * collision.GetContact( 0 ).normal;
    }

    private void SetTimeScale(float scale )
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    private void GetClosestCollision()
    {
        if(rb.velocity != Vector2.zero)
        {
            var hits = Physics2D.CircleCastNonAlloc( rb.position, colliderRadius, rb.velocity.normalized, tempPhysicsCastHits, sensorRange, collisionMask );
            if(hits > 0)
            {
                if (tempPhysicsCastHits[0].collider)
                {
                    closestCollision = tempPhysicsCastHits[0].point;
                    closestCollisionCenter = closestCollision + tempPhysicsCastHits[0].normal * colliderRadius;
                    return;
                }
            }
        }
        closestCollision = null;
        closestCollisionCenter = null;
    }

    private Vector2? GetRaycastHitPoint(Vector2 direction, float range)
    {
        Debug.DrawRay( rb.position, direction * range );
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
    private void OnDrawGizmos()
    {
        Debug.DrawRay( rb.position, rb.velocity.normalized * Mathf.Clamp( sensorRange * rb.velocity.magnitude / 30, 2f, 5f ) );
    }
}
