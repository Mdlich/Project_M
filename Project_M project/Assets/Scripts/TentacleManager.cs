using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TentacleManager : MonoBehaviour
{
	private readonly Vector2[] initialDirections = new Vector2[] { new Vector2( -1, -1 ), new Vector2( -1, 0 ), new Vector2( -1, 1 ),
		new Vector2( 1, -1 ), new Vector2( 1, 0 ), new Vector2( 1, 1 ) };

	[SerializeField]
	private GameObject tentaclePrefab;
	[SerializeField]
	private LayerMask hitMask;
	[SerializeField]
	private float tentacleRange;
	[SerializeField]
	private int maxActiveTentacles = 6;
	[SerializeField]
	private int spareTentaclesCount = 2;
	[SerializeField]
	private float distanceBetweententacles = 3f;
	[SerializeField]
	private float tentacleInterpolationTime = 0.5f;

	private Rigidbody2D player;
	private List<TentacleController> attachedTentacles;
	private List<TentacleController> spareTentacles;
	private bool retractingAll;
	private readonly int[] sideDirection = new int[2] { -1, 1 };

	public float TentacleRange { get => tentacleRange;}
	public float TentacleInterpolationTime { get => tentacleInterpolationTime; }

	private bool dispatching;
	private void Awake()
	{
		player = GetComponentInParent<Rigidbody2D>();
		attachedTentacles = new List<TentacleController>();
		spareTentacles = new List<TentacleController>();
		for (int i = 0; i < maxActiveTentacles + spareTentaclesCount; i++)
		{
			var newTentacle = Instantiate( tentaclePrefab, transform ).GetComponent<TentacleController>();
			newTentacle.name = $"tentacle_{i + 1}";
			newTentacle.Init( player, this);
			newTentacle.StartedRetracting += OnRetracting;
			newTentacle.BecameAvailable += OnRetracted;
			spareTentacles.Add( newTentacle );
		}
		dispatching = false;
	}
	private void Update()
	{
		if (!dispatching)
			return;
		if (!retractingAll && attachedTentacles.Count < maxActiveTentacles && spareTentacles.Count > 0)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < GetMissingTentacles( i ); j++)
				{
					TryLaunchTentacle( GetLaunchDirection( i ) );
				}
			}
		}
	}

	private int GetMissingTentacles(int side)
	{
		int count = 0;
		foreach (var tentacle in attachedTentacles)
		{
			if (tentacle.Side == side)
				count++;
		}
		return maxActiveTentacles / 2 - count;
	}
	public void QueueRetractAll()
	{
		StopAllCoroutines();
		retractingAll = true;
		StartCoroutine( RetractAndDeactiavate() );

	}

	public void QueueEnable()
	{
		//gameObject.SetActive( true );
		dispatching = true;
		StopAllCoroutines();
		retractingAll = false;
		for (int i = 0; i < maxActiveTentacles; i++)
		{
			TryLaunchTentacle( initialDirections[i] );
		}
	}

	private IEnumerator RetractAndDeactiavate()
	{
		for (int i = attachedTentacles.Count - 1; i >= 0; i--)
		{
			attachedTentacles[i].RequestRetract();
		}

		while (spareTentacles.Count != maxActiveTentacles + spareTentaclesCount)
		{
			yield return null;
		}
		retractingAll = false;
		dispatching = false;
		//gameObject.SetActive( false );
	}

	private Vector2? GetAttachPoint( Vector2 direction)
	{
		direction.Normalize();
		var result = Physics2D.Raycast( player.position, direction, tentacleRange, hitMask );
		if (result.collider != null)
		{
			foreach (var tentacle in attachedTentacles)
			{
				if (Vector2.Distance( result.point, tentacle.TargetPos) < distanceBetweententacles)
				{
					return null;
				}
			}
			return result.point;
		}
		return null;
	}

	private Vector2 GetLaunchDirection(int side )
	{
		var yDirection = player.velocity.y >= 0 ? 1 : -1;
		return new Vector2( sideDirection[side], yDirection );
	}
	private void OnRetracting(TentacleController tentacle )
	{
		attachedTentacles.Remove( tentacle );
	}

	private void OnRetracted(TentacleController tentacle )
	{
		spareTentacles.Add( tentacle );
	}

	private bool TryLaunchTentacle(Vector2 direction )
	{
		var attachPoint = GetAttachPoint( direction );
		if (attachPoint.HasValue && spareTentacles.Count > 0)
		{
			var tentacle = spareTentacles[0];
			tentacle.Expand( attachPoint.Value );
			spareTentacles.Remove( tentacle );
			attachedTentacles.Add( tentacle );
			return true;
		}
		return false;
	}
}
