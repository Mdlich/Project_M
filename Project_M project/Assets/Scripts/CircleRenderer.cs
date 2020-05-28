using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleRenderer : MonoBehaviour
{
    [SerializeField]
    private int pointsCount = 16;
    [SerializeField]
    private float radius;
    [SerializeField]
    Color color;
    [SerializeField]
    private LineRenderer lineRenderer;
    private Vector3[] positions;

    public void SetRadiusAndOpacity(float newRadius, float opacity )
    {
        radius = newRadius;
        color.a = opacity;
        UpdateCircle();
    }
    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        UpdateCircle();
    }
    private void OnValidate()
    {
        lineRenderer = GetComponent<LineRenderer>();
        UpdateCircle();
    }
    private void UpdateCircle()
    {
        positions = new Vector3[pointsCount];
        for (int i = 0; i < pointsCount; i++)
        {
            positions[i] = GetSphericalDirection( i, pointsCount ) * radius;
        }
        lineRenderer.positionCount = pointsCount;
        lineRenderer.SetPositions( positions );
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }
    private Vector3 GetSphericalDirection( int i, int directionsNum )
    {
        return Quaternion.AngleAxis( i * 360 / directionsNum, Vector3.forward ) * (Vector3.right);
    }
}
