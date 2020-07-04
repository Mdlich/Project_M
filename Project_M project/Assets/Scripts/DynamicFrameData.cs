using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DynamicFrameData
{
	public float VerticalVelocity => velocity.y;

	public Vector2 position;
	public Vector2 velocity;
	public DynamicFrameData(Vector2 pos, Vector2 vel )
	{
		position = pos;
		velocity = vel;
	}

	public DynamicFrameData( Vector2 pos, float verticalVelocity )
	{
		position = pos;
		velocity = new Vector2( 0, verticalVelocity );
	}
}