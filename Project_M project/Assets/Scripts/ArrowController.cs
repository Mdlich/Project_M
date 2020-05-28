using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
	public Transform cubeTransform;
	private RectTransform arrowTransform;
	private void Start()
	{
		arrowTransform = transform as RectTransform;
	}
	private void Update()
	{
		arrowTransform.position = Camera.main.WorldToScreenPoint( cubeTransform.position );
	}
	public void SetRot(Vector3 direction )
	{
		arrowTransform.rotation = Quaternion.FromToRotation( Vector3.up, direction );
	}

	private void OnEnable()
	{
		if ( arrowTransform ) arrowTransform.position = Camera.main.WorldToScreenPoint( cubeTransform.position );
	}
}
