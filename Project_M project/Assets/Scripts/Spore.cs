using UnityEngine;
using System.Collections;

public class Spore : Spawnable
{
	private Transform player;
	private Rigidbody2D rb;
	[SerializeField]
	private float pullingDistance = 3f;
	[SerializeField]
	private float pullingSpeed = 10f;
	private float pullingDistanceSquared;
	private void Start()
	{
		player = GameObject.FindGameObjectWithTag( "Player" ).transform;
		rb = GetComponent<Rigidbody2D>();
		pullingDistanceSquared = pullingDistance * pullingDistance;
	}
	private void FixedUpdate()
	{
		Vector2 sporeToPlayer = player.transform.position - transform.position;
		float sqrMag = sporeToPlayer.sqrMagnitude;
		if (sqrMag <= pullingDistanceSquared)
		{
			var proximityMod = 0.5f + (pullingDistanceSquared - sqrMag) / pullingDistanceSquared;
			var force = sporeToPlayer.normalized * pullingSpeed * Time.deltaTime * proximityMod;
			rb?.MovePosition( rb.position + force );
		}
	}
}
