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
    private float chargingTime = 2f;
    [SerializeField]
    private float chargingStepPower = 8f;
    [SerializeField]
    private Vector2 chargeMaxSqueeze = new Vector2(0.5f, 1f);

    private RaycastHit2D[] tempSphereCastHits = new RaycastHit2D[10];
    private Vector2 lastCollisionPoint;
    private Vector2 lastCollisionCenter;
    private Vector2? closestCollision;
    private Vector2? closestCollisionCenter;
    private bool chargingBounce;
    private bool inTapRange;
    private bool inPerfectTapRange;
    private bool collisionHandled;
    private InputManager inputManager;
    private int momentum = 0;
    private float indicatorRadius;
    private float colliderRadius = 0f;
    private float momentumDecayTimer;
    private bool tapped;
    private CircleCollider2D collider;

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
        collider = GetComponent<CircleCollider2D>();
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
        GetClosestCollision();
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
        }

        if (inputManager.PointerHeld && inputManager.Delta.HasValue && arrow)
        {
            arrow.SetRot( inputManager.Delta.Value );
            //arrow.gameObject.SetActive( true );
        }
        /*else if (arrow && arrow.gameObject.activeSelf)
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

    private void OnPointerFirstheld( Vector3 pos )
    {
        SetTimeScale( timeSlowMod );
        arrow.gameObject.SetActive( true );
    }

    private void OnPointerUp( Vector3 pos )
    {

        if (inputManager.PointerHeld)
        {
            arrow.gameObject.SetActive( false );
            SetTimeScale( 1f );
            rb.velocity = Vector2.zero;
            rb.AddForce( inputManager.Delta.Value * forceMod, ForceMode2D.Impulse );
            return;
        }
        else if(chargingBounce)
        {
            tapped = true;
        }
        /*else if(inTapRange && !collisionHandled)
        {
            HandleTap( closestCollisionCenter.HasValue ? closestCollisionCenter.Value : lastCollisionCenter);
        }*/
    }

    private void HandleTap(Vector2 collisionPoint)
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
    }
    private void OnCollisionEnter2D( Collision2D collision )
    {
        if (collisionMask != (collisionMask | (1 << collision.gameObject.layer)) || chargingBounce)
            return;

        chargingBounce = true;
        Vector2 newVelocity;
        if (collision.gameObject.CompareTag( "RightWall" ))
            newVelocity = Vector2.left;
        else if (collision.gameObject.CompareTag( "LeftWall" ))
            newVelocity = Vector2.right;
        else
            newVelocity = collision.GetContact( 0 ).normal;

        if (newVelocity.y == 0f)
            newVelocity.y = 0.1f;
        if (newVelocity.x == 0)
            newVelocity.x = 0.33f;
        var momentumRatio = (float)momentum / (maxMomentum);

        
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
        StartCoroutine( ChargeBounce(newVelocity) );

        /*Vector2 newVelocity;
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
        lastCollisionCenter = lastCollisionPoint + colliderRadius * collision.GetContact( 0 ).normal;*/
    }

    /*private void OnCollisionExit2D( Collision2D collision )
    {
        Debug.Log( $"exiting: {collision.collider.name}" );
        chargingBounce = false;
        tapped = false;
    }*/

    private void SetTimeScale(float scale )
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    IEnumerator ChargeBounce(Vector2 baseVelocity)
    {
        var tappedMod = 0f;
        var lastChargeStartTime = Time.time;
        var baseScale = transform.localScale;
        var timeMod = 0f;
        var arrowDirection = baseVelocity;
        rb.angularVelocity = 0f;
        arrow.gameObject.SetActive( true );
        arrow.SetRot( baseVelocity.normalized );

        while (Time.time - lastChargeStartTime < chargingTime)
        {
            timeMod = (Time.time - lastChargeStartTime) / chargingTime;
            timeMod = Mathf.Pow( timeMod, chargingStepPower );
            Debug.Log( $"{timeMod}" );
            arrowDirection.y = Mathf.Lerp( baseVelocity.y, baseVelocity.y + 2f, timeMod );
            //arrowDirection.Normalize();
            arrow.SetRot( arrowDirection );
            transform.localScale = Vector3.Lerp( baseScale, chargeMaxSqueeze, timeMod );
            if (tapped)
            {
                tappedMod = timeMod;
                break;
            }
            yield return new WaitForFixedUpdate();
        }

        if (!tapped)
        {
            lastChargeStartTime = Time.time;
            while (Time.time - lastChargeStartTime < chargingTime)
            {
                timeMod = 1f - (Time.time - lastChargeStartTime) / chargingTime;
                timeMod = Mathf.Pow( timeMod, chargingStepPower );
                arrowDirection.y =  Mathf.Lerp( baseVelocity.y, baseVelocity.y + 2f, timeMod );
                //arrowDirection.Normalize();
                arrow.SetRot( arrowDirection );
                transform.localScale = Vector3.Lerp( baseScale, chargeMaxSqueeze, timeMod );
                if (tapped)
                {
                    tappedMod = timeMod;
                    break;
                }
                yield return new WaitForFixedUpdate();
            }
        }

        arrow.gameObject.SetActive( false );
        transform.localScale = baseScale;
        rb.isKinematic = false;
        Vector2 newVelocity = baseVelocity;
        newVelocity.y = Mathf.Lerp( baseVelocity.y, baseVelocity.y + 2f, tappedMod );
        rb.velocity = newVelocity * forceMod;

        if (momentumParticles)
        {
            momentumParticles.transform.position = rb.position;
            var particlesMain = momentumParticles.main;
            if (tapped)
                particlesMain.startColor = boost2Color;
            else
                particlesMain.startColor = boost1Color;
            momentumParticles.Play();
        }

        yield return new WaitForFixedUpdate();

        chargingBounce = false;
        tapped = false;
    }

    private void GetClosestCollision()
    {
        if(rb.velocity != Vector2.zero)
        {
            var hits = Physics2D.CircleCastNonAlloc( rb.position, colliderRadius, rb.velocity.normalized, tempSphereCastHits, sensorRange, collisionMask );
            if(hits > 0)
            {
                if (tempSphereCastHits[0].collider)
                {
                    closestCollision = tempSphereCastHits[0].point;
                    closestCollisionCenter = closestCollision + tempSphereCastHits[0].normal * colliderRadius;
                    return;
                }
            }
        }
        closestCollision = null;
        closestCollisionCenter = null;
    }

    private void OnDrawGizmos()
    {
        Debug.DrawRay( rb.position, rb.velocity.normalized * Mathf.Clamp( sensorRange * rb.velocity.magnitude / 30, 2f, 5f ) );
    }
}
