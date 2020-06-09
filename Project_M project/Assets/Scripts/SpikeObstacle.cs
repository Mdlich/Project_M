using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeObstacle : Spawnable
{
    public void SetSpikesWidth(float width )
    {
        var spriteRenderer = renderer as SpriteRenderer;
        spriteRenderer.size = new Vector2( width * 2f, 2f );
    }
}
