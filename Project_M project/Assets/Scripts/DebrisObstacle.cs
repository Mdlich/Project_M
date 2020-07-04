using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebrisObstacle : Spawnable
{
    private List<DynamicFrameData> frameData = new List<DynamicFrameData>();
    private bool rewinding;
    private Rigidbody2D rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void OnEnable()
    {
        GameManager.RewindTimeInProgressEvent += OnRewindTime;
    }

    private void OnDisable()
    {
        GameManager.RewindTimeInProgressEvent -= OnRewindTime;
    }
    private void OnRewindTime( bool inProgress )
    {
        rewinding = inProgress;
        rb.simulated = !inProgress;
    }

    private void RecordFrame()
    {
        if (frameData.Count > Mathf.Round( GameManager.RewindTime / Time.fixedDeltaTime ))
        {
            frameData.RemoveAt( frameData.Count - 1 );
        }
        frameData.Insert( 0, new DynamicFrameData( rb.position, rb.velocity ) );
    }

    private void RewindFrame()
    {
        if (frameData.Count > 0)
        {
            transform.position = frameData[0].position;
            rb.velocity = frameData[0].velocity;
            frameData.RemoveAt( 0 );
        }
        else
        {
            ReturnToPool();
            frameData.Clear();
        }
    }

    private void FixedUpdate()
    {
        if (!renderer.isVisible && Camera.main.transform.position.y - Camera.main.orthographicSize > transform.position.y)
        {
            rb.velocity = Vector2.zero;
        }
        if (rewinding)
        {
            RewindFrame();
            return;
        }
        else
        {
            RecordFrame();
        }
    }

    public void SetDebrisSize( float size = 2f )
    {
        var spriteRenderer = renderer as SpriteRenderer;
        spriteRenderer.size = new Vector2( 0.5f, size );
    }
}
