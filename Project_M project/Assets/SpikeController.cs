using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeController : MonoBehaviour 
{
    [SerializeField]
    private SpriteRenderer spikeRenderer;

    public void SetSpikesWidth(float width )
    {
        spikeRenderer.size = new Vector2( width, 1f );
    }
}
