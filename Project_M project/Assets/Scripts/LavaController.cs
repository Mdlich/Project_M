using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LavaController : MonoBehaviour
{
    [SerializeField]
    private float startingSpeed = 10f;
    [SerializeField]
    private float maxSpeed = 30f;
    [SerializeField]
    private float timeToMaxSpeed = 60f;
    [SerializeField]
    private float teleportRange = 20f;
    [SerializeField]
    private ParticleSystem lavaParticles;

    private float currentSpeed;
    private float timer;
    private void Start()
    {
        currentSpeed = startingSpeed;
        timer = 0;
    }
    private void Update()
    {
        timer += Time.deltaTime;
        currentSpeed = Mathf.Lerp( startingSpeed, maxSpeed, timer / timeToMaxSpeed );
        var newPos = transform.position += new Vector3( 0, currentSpeed * Time.deltaTime );
        newPos.y = Mathf.Max( newPos.y, Camera.main.transform.position.y - teleportRange );
        transform.position = newPos;
    }

    private void OnTriggerEnter2D( Collider2D collision )
    {
        if (collision.CompareTag("Player") || collision.CompareTag( "Debris" ))
        {
            lavaParticles.transform.position = collision.transform.position - new Vector3( 0, 1f, 0 );
            lavaParticles?.Play();
        }
    }
}
