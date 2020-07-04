using System;
using UnityEngine;

public class Spawnable : MonoBehaviour
{
    protected Renderer renderer;

    protected float LasPosY => renderer.bounds.center.y + renderer.bounds.extents.y;
    private void Awake()
    {
        renderer = GetComponentInChildren<Renderer>();
    }
    protected virtual void Update()
    {
        if (Camera.main.transform.position.y - Camera.main.orthographicSize - SpawningSystem.DespawningDistance >= LasPosY)
        {
            ReturnToPool();
        }
    }

    public void ReturnToPool()
    {
        gameObject.SetActive( false );
        transform.SetParent( SpawningSystem.Pool.transform );
    }
}
