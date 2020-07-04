using System;
using UnityEngine;
public struct TentacleFrameData
{
	public Vector3[] positions;
	public bool enabled;

	public TentacleFrameData(bool enabled, Vector3[] pos )
	{
		this.enabled = enabled;
		positions = pos.Clone() as Vector3[];
	}
}

public struct TrailFrameData
{
	public Vector3[] positions;
	public bool enabled;
	public TrailFrameData(int count, Vector3[] pos, bool enabled )
	{
		this.enabled = enabled;

		positions = new Vector3[count];

		Array.Copy( pos, positions, count );
	}
}