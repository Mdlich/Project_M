using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LaserController : MonoBehaviour
{
    [SerializeField]
    private float startingVerticalSpeed = 5f;
    [SerializeField]
    private float balanceDistance = 15f;
    [SerializeField]
    private float minSpeedMod = 0.5f;
    [SerializeField]
    private float difficultyMod = .001f;

    private float currentSpeed;
    private void Start()
    {
        currentSpeed = startingVerticalSpeed;
    }
    private void Update()
    {
        var camDistance = Camera.main.transform.position.y - transform.position.y;
        currentSpeed = Camera.main.transform.position.y / (Time.time + 0.01f);
        currentSpeed *= Mathf.Clamp(camDistance / balanceDistance, minSpeedMod, float.PositiveInfinity);
        currentSpeed *= 1f + Camera.main.transform.position.y * difficultyMod;
        transform.position += new Vector3( 0, currentSpeed * Time.deltaTime );
    }

    private void OnCollisionEnter2D( Collision2D collision )
    {
        if (collision.collider.CompareTag( "Player" ))
        {
            Debug.Log( "gameover" );
            SceneManager.LoadScene( 0 );
        }
    }
}
