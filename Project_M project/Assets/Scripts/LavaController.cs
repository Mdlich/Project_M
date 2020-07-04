using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.SceneManagement;

public class LavaController : MonoBehaviour
{
    [SerializeField]
    private float lavaPauseTime = 2f;
    [SerializeField]
    private float startingSpeed = 10f;
    [SerializeField]
    private float maxSpeed = 30f;
    [SerializeField]
    private float timeToMaxSpeed = 60f;
    [SerializeField]
    private float teleportRange = 20f;
    [SerializeField]
    private ParticleSystem[] lavaParticles;

    [Space]
    [SerializeField]
    private bool mainMenu;
    [SerializeField]
    private float speedMod;
    [SerializeField]
    private float sineAmplitude;

    private List<DynamicFrameData> frameData = new List<DynamicFrameData>();

    private Rigidbody2D rb;
    private float currentSpeed;
    private float timer;

    private bool rewinding;
    private bool paused;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        GameManager.RewindTimeInProgressEvent += OnRewindTime;
        if(!mainMenu) SetUpFromRemote();
    }

    private void SetUpFromRemote()
    {
        int index = SceneManager.GetActiveScene().buildIndex;
        if (index > 3)
            index -= 3;
        startingSpeed = RemoteConfigManager.LavaStartingSpeed[index - 1];
        maxSpeed = RemoteConfigManager.MaxLavaSpeed[index - 1];
        timeToMaxSpeed = RemoteConfigManager.LavaSpeedUpTime[index - 1];
    }

    private void OnDestroy()
    {
        GameManager.RewindTimeInProgressEvent -= OnRewindTime;
    }
    private void OnRewindTime( bool inProgress )
    {
        rewinding = inProgress;
        if (!inProgress)
        {
            StartCoroutine( PauseLava() );
        }
    }

    private void Start()
    {
        currentSpeed = startingSpeed;
        timer = 0;
    }

    private void FixedUpdate()
    {
        if (rewinding)
        {
            RewindFrame();
            return;
        }
        else
        {
            RecordFrame();
        }

        if (paused)
            return;

        if (mainMenu)
        {
            var followPos = Camera.main.transform.position;
            var sineMod = Mathf.Sin( Time.time * speedMod ) * sineAmplitude;
            followPos.y += sineMod - 10f;
            rb.MovePosition( followPos );
        }
        else
        {
            currentSpeed = Mathf.Lerp( startingSpeed, maxSpeed, timer / timeToMaxSpeed );
            var newPos = transform.position += new Vector3( 0, currentSpeed * Time.deltaTime );
            newPos.y = Mathf.Max( newPos.y, Camera.main.transform.position.y - teleportRange );
            transform.position = newPos;
        }
        timer += Time.deltaTime;
    }

    private IEnumerator PauseLava()
    {
        paused = true;
        yield return new WaitForSecondsRealtime( lavaPauseTime );
        paused = false;
    }
    private void RecordFrame()
    {
        if (frameData.Count > Mathf.Round( GameManager.RewindTime / Time.fixedDeltaTime ))
            frameData.RemoveAt( frameData.Count - 1 );
        frameData.Insert( 0, new DynamicFrameData( transform.position, currentSpeed ) );
    }

    private void RewindFrame()
    {
        if (frameData.Count > 0)
        {
            transform.position = frameData[0].position;
            currentSpeed = frameData[0].VerticalVelocity;
            frameData.RemoveAt( 0 );
            timer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D( Collider2D collision )
    {
        if (collision.CompareTag("Player") || collision.CompareTag( "Debris" ))
        {
            foreach (var p in lavaParticles)
            {
                if (!p.isPlaying)
                {
                    p.transform.position = collision.transform.position - new Vector3( 0, 1f, 0 );
                    p.Play();
                    break;
                }
            }
            SoundManager.PlaySound( SoundManager.Sound.LavaCollision );
        }
    }
}
