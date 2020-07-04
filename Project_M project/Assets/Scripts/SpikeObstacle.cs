using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class SpikeObstacle : Spawnable
{
    [SerializeField] private float spriteWidth = 2f;
    /*[SerializeField] private Light2D light2d;

    private Vector3[] lightBox = new Vector3[4];*/
    public float Width { get => spriteWidth;}

    /*private void Start()
    {
        if (!light2d) light2d = GetComponentInChildren<Light2D>();
        lightBox[0] = new Vector3( -1f, -1f, 0f );
        lightBox[1] = new Vector3( -1f, 1f, 0f );
        lightBox[2] = new Vector3( 1f, 1f, 0f );
        lightBox[3] = new Vector3( 1f, -1f, 0f );
    }*/
    public void SetSpikesWidth(float width )
    {
        var spriteRenderer = renderer as SpriteRenderer;
        spriteRenderer.size = new Vector2( width * spriteWidth, 2f );
        /*lightBox[0].x = -width;
        lightBox[1].x = -width;
        lightBox[2].x = width;
        lightBox[3].x = width;
        for (int i = 0; i < light2d.shapePath.Length; i++)
        {
            Debug.Log( $"setting vertex {i}" );
            light2d.shapePath[i].x = lightBox[i].x;
        }*/
    }
}
