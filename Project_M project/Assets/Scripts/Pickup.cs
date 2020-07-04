using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Pickup : Spawnable
{
	[SerializeField]
	private PickupEffect effect;
	[SerializeField]
	private float pullingDistance = 3f;
	[SerializeField]
	private float pullingSpeed = 10f;
	[SerializeField]
	private float freeMovementPeriod = 2f;
	[SerializeField]
	private float freeMovementMod = 10f;

	private Transform player;
	private Rigidbody2D rb;
	private Collider2D collider;
	private float freeMovementTimer;
	private Vector2 currentFreeMovement;

	private Collider2D[] collisions = new Collider2D[5];

	private List<DynamicFrameData> frameData = new List<DynamicFrameData>();
	private bool rewinding;
	private float consumedTime;
	private int consumedFrame; 
	private void Start()
	{
		player = GameObject.FindGameObjectWithTag( "Player" ).transform;
		rb = GetComponent<Rigidbody2D>();
		collider = rb.GetComponentInChildren<Collider2D>();
		GameManager.RewindTimeInProgressEvent += OnRewindTime;
	}

	private void OnDestroy()
	{
		GameManager.RewindTimeInProgressEvent -= OnRewindTime;
	}

	private void OnRewindTime( bool inProgress )
	{
		rewinding = inProgress;
	}

	private void FixedUpdate()
	{
		if (rewinding)
		{
			RewindFrame();
			return;
		}
		else
		{
			RecordFrame();
		}

		consumedFrame++;

		if (rb.position.x > 8)
		{
			rb.position = new Vector2( 8, rb.position.y );
		}
		if (rb.position.x < -8)
		{
			rb.position = new Vector2( -8, rb.position.y );
		}

		// Cache player instead of looking for it every frame
		var colCount = Physics2D.OverlapCircleNonAlloc( rb.position, 4f, collisions );
		for (int i = 0; i < colCount; i++)
		{
			if (!collisions[i].CompareTag( "Player" ) && collisions[i] != collider)
			{
				var closestPoint = collisions[i].ClosestPoint( rb.position );
				var fromContactPointVector = rb.position - closestPoint;
				var distance = fromContactPointVector.magnitude;
				if (distance == 0f)
				{
					distance = 0.001f;
				}
				rb.AddForce( fromContactPointVector * (4f / distance) * (4f / distance) );
			}
		}

		Vector2 sporeToPlayer = player.transform.position - transform.position;
		float toPlayerDistance = sporeToPlayer.magnitude;
		if (toPlayerDistance <= pullingDistance)
		{
			var proximityMod = pullingDistance / toPlayerDistance;
			rb.velocity = Vector2.zero;
			rb.AddForce( sporeToPlayer * pullingSpeed * proximityMod, ForceMode2D.Impulse);
		}
		else
		{
			if (freeMovementTimer >= freeMovementPeriod)
			{
				freeMovementTimer = 0;
				currentFreeMovement = UnityEngine.Random.insideUnitCircle;
			}

			if(colCount <= 0)
				rb.AddForce( currentFreeMovement * Time.deltaTime * freeMovementMod );
			freeMovementTimer += Time.deltaTime;
		}
	}

	private void RecordFrame()
	{
		if (frameData.Count > Mathf.Round( GameManager.RewindTime / Time.fixedDeltaTime ))
		{
			frameData.RemoveAt( frameData.Count - 1 );
		}
		frameData.Insert( 0, new DynamicFrameData( rb.position, rb.velocity ) );
	}

	private void RewindFrame()
	{
		if (frameData.Count > 0)
		{
			transform.position = frameData[0].position;
			rb.velocity = frameData[0].velocity;
			frameData.RemoveAt( 0 );
		}
		else
		{
			ReturnToPool();
			frameData.Clear();
		}
	}

	private void OnTriggerEnter2D( Collider2D collision )
	{
		if (rewinding)
			return;
		var player = collision.gameObject.GetComponent<PlayerController>();
		if (player)
		{
			effect.ApplyEffect( player );
			player.AbsorbPickup(this);
			ReturnToPool();
		}
	}

	public void RespawnByRewindTime()
	{
		transform.parent = SpawningSystem.instance.transform;
		gameObject.SetActive( true );
	}
	public void SetEffect(PickupEffect newEffect )
	{
		if (effect)
			transform.Find( effect.graphicsObjectName ).gameObject.SetActive( false );

		effect = newEffect;
		transform.Find( effect.graphicsObjectName ).gameObject.SetActive( true );
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere( rb.position, 4f );
	}
}
