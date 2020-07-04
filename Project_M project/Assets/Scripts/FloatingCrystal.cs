using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingCrystal : Spawnable
{
	public float Radius { get; private set; }
	private float rotationSpeed;
	public void SetRadius(float radius )
	{
		Radius = radius;
		transform.localScale = new Vector3( radius, radius, 1f );
		transform.localRotation = Quaternion.Euler( 0, 0, Random.Range( 0f, 180f ) );
		rotationSpeed = Random.Range( -90f, 90f );
	}

	protected override void Update()
	{
		base.Update();
		transform.Rotate( 0, 0, rotationSpeed * Time.deltaTime );
	}
}
