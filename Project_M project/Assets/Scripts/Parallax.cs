using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    public float parallaxMult = 1f;
    private Transform cameraTransform;
    private Vector3 lastCameraPos;
    private float textureunitSize;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPos = cameraTransform.position;
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        Sprite sprite = renderer.sprite;
        textureunitSize = sprite.bounds.size.y;
    }

    void Update()
    {
        var deltaMovement = cameraTransform.position - lastCameraPos;
        transform.position += deltaMovement * parallaxMult;
        lastCameraPos = cameraTransform.position;

        if(Mathf.Abs(cameraTransform.position.y - transform.position.y) > textureunitSize)
        {
            float offsetPosition = 0; // (cameraTransform.position.y - transform.position.y) % textureunitSize;
            var newPos = new Vector3( transform.position.x, cameraTransform.position.y + offsetPosition );
            transform.position = newPos;
        }
    }
}
