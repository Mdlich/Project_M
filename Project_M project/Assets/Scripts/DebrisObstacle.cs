using UnityEngine;
using System.Collections;

public class DebrisObstacle : Spawnable
{
    public void SetDebrisSize( float size )
    {
        var spriteRenderer = renderer as SpriteRenderer;
        spriteRenderer.size = new Vector2( 0.5f, size );
    }
}
