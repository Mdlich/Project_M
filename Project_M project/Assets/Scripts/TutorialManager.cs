using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
	[SerializeField]
	private GameObject tapAndHold;
	[SerializeField]
	private GameObject release;
	[SerializeField]
	private float offsetFromCenter;
	[SerializeField]
	private float timeBetweenTips = 1f;
	[SerializeField]
	private int showCount = 5;
	[SerializeField]
	private float tutorialTimeScale = 0.5f;
	[SerializeField]
	private float timeScaleAdjustSpeed = 2f;
	[SerializeField] private bool hideTtutorial;

	private int tipShowedCount;
	private bool tipInProgress;
	private float lastTipTime;

	private PlayerController player;
	private Rigidbody2D playerRB;

	private void Awake()
	{
		GameManager.ActivateTutorialEvent += Activate;
		player = FindObjectOfType<PlayerController>();
		playerRB = player.GetComponent<Rigidbody2D>();
		gameObject.SetActive( GameManager.TutorialActive == 1 ? true : false );
	}

	private void OnDestroy()
	{
		GameManager.ActivateTutorialEvent -= Activate;
	}

	private void Activate(int active )
	{
		bool a = active == 1 ? true : false;
		if (gameObject.activeSelf != a)
			gameObject.SetActive( a );
	}

	private void Update()
	{
		if (GameManager.GamePaused || hideTtutorial)
			return;

		if (tipShowedCount >= showCount)
		{
			GameManager.SetTutorialActive( 0 );
			gameObject.SetActive( false );
			return;
		}

		if (!tipInProgress && player.TentacleAvailable && Time.time - lastTipTime >= timeBetweenTips)
		{
			StartCoroutine( ShowTip() );
			tipInProgress = true;
			lastTipTime = Time.time;
		}
	}
	private void OnEnable()
	{
		tipShowedCount = 0;
		DeactivateAll();
	}

	private IEnumerator ShowTip()
	{
		yield return null;
		var initialDirection = Mathf.Sign( playerRB.velocity.x );
		while ((playerRB.velocity.x > 0 && playerRB.position.x < -offsetFromCenter) ||
			(playerRB.velocity.x < 0 && playerRB.position.x > offsetFromCenter))
		{
			if (Mathf.Sign(playerRB.velocity.x) != initialDirection)
			{
				Debug.Log( "tut canceled due to wrong direction" );
				tipInProgress = false;
				yield break;
			}
			yield return null;
		}

		if (player.TentacleAvailable)
		{
			tapAndHold.SetActive( true );
		}
		else
		{
			Debug.Log( "tut canceled due to tentacle unavailable" );
			tipInProgress = false;
			yield break;
		}

		var timeScalemod = 1f;
		while (!player.TentacleLaunching)
		{
			timeScalemod = IterateTimescale( timeScalemod );
			GameManager.SetTimeScale( timeScalemod );
			if (!player.TentacleAvailable)
			{
				GameManager.SetTimeScale( 1f );
				tapAndHold.SetActive( false );
				tipInProgress = false;
				yield break;
			}
			yield return null;
		}

		while (!player.TentacleConnected)
		{
			timeScalemod = IterateTimescale( timeScalemod );
			GameManager.SetTimeScale( timeScalemod );
			if (!player.TentacleLaunching)
			{
				GameManager.SetTimeScale( 1f );
				tapAndHold.SetActive( false );
				tipInProgress = false;
				yield break;
			}
			yield return null;
		}

		tapAndHold.SetActive( false );
		release.SetActive( true );

		while (player.TentacleConnected)
		{
			timeScalemod = IterateTimescale( timeScalemod );
			GameManager.SetTimeScale( timeScalemod );
			yield return null;
		}
		GameManager.SetTimeScale( 1f );
		release.SetActive( false );
		tipShowedCount++;
		tipInProgress = false;
	}

	private float IterateTimescale(float timeScalemod )
	{
		timeScalemod -= Time.unscaledDeltaTime * timeScaleAdjustSpeed;
		timeScalemod = Mathf.Max( timeScalemod, tutorialTimeScale );
		return timeScalemod;
	}
	private void DeactivateAll()
	{
		StopAllCoroutines();
		tapAndHold.SetActive( false );
		release.SetActive( false );
	}
}
