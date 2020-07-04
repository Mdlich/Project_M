using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(LineRenderer))]
public class TentacleController: MonoBehaviour
{
	public event Action<TentacleController> BecameAvailable;
	public event Action<TentacleController> StartedRetracting;
	public Vector2? CurrentAnchorPoint { get { return currentAnchorPoint; } }

	public int Side { get; private set; }
	public Vector2 TargetPos { get => targetAnchor.position; }

	private Transform targetAnchor;
	private LineRenderer lineRenderer;
	private Rigidbody2D player;
	private TentacleManager manager;
	private Vector2? currentAnchorPoint;
	private bool interpolating;

	private void Awake()
	{
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.enabled = false;
		targetAnchor = new GameObject( $"{gameObject.name}_target" ).transform;
	}

	private void OnDisable()
	{
		lineRenderer.enabled = false;
	}

	public void Init(Rigidbody2D player, TentacleManager manager)
	{
		this.manager = manager;
		this.player = player;
	}

	private void Update()
	{
		lineRenderer.SetPosition( 0, player.position );
		if (!interpolating && lineRenderer.enabled)
		{
			if(currentAnchorPoint.HasValue && Vector3.Distance(player.position, currentAnchorPoint.Value) > manager.TentacleRange + UnityEngine.Random.Range(-1f, 1f))
			{
				StartCoroutine( Retract() );
			}
		}
	}

	public void Expand(Vector2 target )
	{
		var direction = target - player.position;
		Side = (int)(Mathf.Sign( direction.x ) + 1) / 2;
		targetAnchor.SetParent( null );
		targetAnchor.position = target;
		StartCoroutine( InterpolateAnchor( player.position, targetAnchor ) );
		lineRenderer.enabled = true;
	}

	public void RequestRetract()
	{
		StartCoroutine( Retract() );
	}
	private IEnumerator Retract()
	{
		targetAnchor.SetParent( player.transform );
		//targetAnchor.position = player.position;
		StartedRetracting?.Invoke( this );
		if (currentAnchorPoint.HasValue)
		{
			yield return StartCoroutine( InterpolateAnchor( currentAnchorPoint.Value, player.transform ) );
		}
		lineRenderer.enabled = false;
		BecameAvailable?.Invoke( this );
	}
	private IEnumerator InterpolateAnchor(Vector2 origin, Transform target )
	{
		interpolating = true;
		float interpolator = 0f;
		currentAnchorPoint = origin;
		lineRenderer.SetPosition( 1, currentAnchorPoint.Value );
		while (Vector2.Distance( currentAnchorPoint.Value, target.position ) > 0f)
		{
			yield return null;
			interpolator += Time.deltaTime / manager.TentacleInterpolationTime;
			currentAnchorPoint = Vector2.Lerp( origin, target.position, interpolator );
			lineRenderer.SetPosition( 1, currentAnchorPoint.Value );
		}
		interpolating = false;
	}
}
