using System;
using UnityEngine;

public class Spawnable : MonoBehaviour
{
    protected Renderer renderer;
    public Action<Spawnable> ReadyToPush;
    private void Awake()
    {
        renderer = GetComponentInChildren<Renderer>();
    }
    void Update()
    {
        if (Camera.main.transform.position.y > transform.position.y && !renderer.isVisible)
        {
            ReturnToPool();
        }
    }

    public void ReturnToPool()
    {
        ReadyToPush?.Invoke( this );
    }

    private void OnTriggerEnter2D( Collider2D collision )
    {
        if(collision.CompareTag( "Lava" ))
        {
            Invoke( "ReturnToPool", 1f );
        }
    }
}
