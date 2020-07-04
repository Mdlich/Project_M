using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{

	[SerializeField] [TextArea] private string reloadDescription;
	[SerializeField] [TextArea] private string watchAdButtontext;
	[SerializeField] [TextArea] private string respawnDescription;
	[SerializeField] [TextArea] private string restartDesription;
	[SerializeField] [TextArea] private string adSkippedText;

	[SerializeField] private Text respawnsLeftText;
	[SerializeField] private Text gameoverText;
	[SerializeField] private Text gameoverButtonText;

	[SerializeField] private GameObject respawnPanel;

	private void Start()
	{
		if (!GameManager.AdsOn || (GameManager.LivesLeft <= 0 && SceneManager.GetActiveScene().buildIndex == 1))
		{
			gameObject.SetActive( false );
			respawnPanel.SetActive( true );
			return;
		}
		AdsManager.RewardedVideoWatched += OnRewardedVideoWatched;
	}

	private void OnDestroy()
	{
		AdsManager.RewardedVideoWatched -= OnRewardedVideoWatched;
	}

	public void WatchRewardedVideo()
	{
		AdsManager.ShowRewardedVideo();
	}
	private void OnRewardedVideoWatched( bool watchedThrought )
	{
		if (watchedThrought)
		{
			gameObject.SetActive( false );
			respawnPanel.SetActive( true );
		}
		else
		{
			gameoverText.text = adSkippedText;
		}
	}

	private void OnEnable()
	{
		if (!GameManager.AdsOn || GameManager.LivesLeft <= 0)
		{
			gameObject.SetActive( false );
			respawnPanel.SetActive( true );
			return;
		}

		if (GameManager.LivesLeft > 0)
		{
			gameoverText.text = respawnDescription;
			gameoverButtonText.text = watchAdButtontext;
			respawnsLeftText.text = $"Respawns left: {GameManager.LivesLeft}";
			respawnsLeftText.gameObject.SetActive( true );
		}
		else
		{
			/*respawnsLeftText.gameObject.SetActive( false );
			gameoverText.text = reloadDescription;
			gameoverButtonText.text = watchAdButtontext;*/
		}
	}
}
