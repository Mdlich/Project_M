using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class CubeController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D rb;
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
    private float pointerHoldTime = 0.1f;
    [SerializeField]
    private float timeSlowMod = 0.1f;
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
    [SerializeField]
    private float averageVelocityTime = 5f;
    [SerializeField]
    private Vector2 startingVelocity = new Vector2( 20, 10 );
    [SerializeField]
    private Vector2 tentacleDirection = new Vector2( 1f, 1.33f );


    private RaycastHit2D[] tempPhysicsCastHits = new RaycastHit2D[10];
    private bool tentacleConnected;
    private InputManager inputManager;
    private float averageVerticalVelocity;

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
        rb.AddForce( startingVelocity, ForceMode2D.Impulse );
        averageVerticalVelocity = startingVelocity.y;
    }
    void Update()
    {
        inputManager.ProcessInput();

        if (tentacleConnected)
        {
            tentacleRenderer.SetPosition( 0, rb.position );
        }
    }

    private void FixedUpdate()
    {
        averageVerticalVelocity += (rb.velocity.y - averageVerticalVelocity) / (averageVelocityTime / Time.fixedUnscaledDeltaTime);
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
        //Vector2 tentacleDirection;
        if (rb.velocity.x >= 0)
        {
            tentacleDirection.x = -1f; // = new Vector2( -1f, 1.33f );
        }
        else
        {
            tentacleDirection.x = 1f;
        }
        var tentacleHit = GetRaycastHitPoint( tentacleDirection, tentacleRange );
        tentacleRenderer.positionCount = 2;
        StartCoroutine( LaunchTentacle( tentacleHit ?? Camera.main.ScreenToWorldPoint( pos ), tentacleHit.HasValue ) );
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
    }

    private void OnCollisionEnter2D( Collision2D collision )
    {
        Vector2 newVelocity = Vector2.zero;

        if (inputManager.PointerHeld)
        {
            tentacleConnected = false;
            tentacleRenderer.enabled = false;
            tentaclejoint.enabled = false;
            StopAllCoroutines();
        }

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            newVelocity = collision.GetContact( 0 ).normal;
            newVelocity.x *= startingVelocity.x;
            averageVerticalVelocity *= 0.5f;
            newVelocity.y = averageVerticalVelocity;
            var main = momentumParticles.main;
            main.startColor = boost1Color;
            momentumParticles.Play();
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            if (collision.gameObject.CompareTag( "RightWall" ))
                newVelocity = Vector2.left;
            else if (collision.gameObject.CompareTag( "LeftWall" ))
                newVelocity = Vector2.right;
            else
                newVelocity = collision.GetContact( 0 ).normal;

            newVelocity.x *= startingVelocity.x;
            newVelocity.y = averageVerticalVelocity;
        }

        rb.velocity = newVelocity;
    }

    private void SetTimeScale(float scale )
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    /*private void GetClosestCollision()
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
    }*/

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
    /*private void OnDrawGizmos()
    {
        Debug.DrawRay( rb.position, rb.velocity.normalized * Mathf.Clamp( sensorRange * rb.velocity.magnitude / 30, 2f, 5f ) );
    }*/
}
