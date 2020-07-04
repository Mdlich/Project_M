using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
	public static UIManager instance;

	[SerializeField]
	private GameObject deathmatchPanel;
	[SerializeField]
	private GameObject inGameUI;
	[SerializeField]
	private GameObject inGameMenu;
	[SerializeField]
	private GameObject gameOverPanel;
	[SerializeField]
	private GameObject victoryPanel;
	[SerializeField]
	private GameObject startGamePanel;
	[SerializeField]
	private GameObject rewindTip;
	[SerializeField]
	private TextMeshProUGUI gpgsButtonText;
	private void Awake()
	{
		if (!instance)
		{
			instance = this;
		}
		else
		{
			Destroy( gameObject );
		}
		if (gpgsButtonText)
		{
			GameServicesManager.Authenticated += OnGPGSAuthenticated;
			OnGPGSAuthenticated( GameServicesManager.UserSignedIn );
		}
	}

	private void OnGPGSAuthenticated( bool success )
	{
		if (success)
		{
			gpgsButtonText.text = "Sign out of Google";
		}
		else
		{
			gpgsButtonText.text = "sign in to Google";
		}
	}

	private void Start()
	{
		if (GameManager.InMainMenu)
			return;
		GameManager.GameOverEvent += OnGameOver;
		GameManager.PauseGameEvent += SetPauseGame;
		GameManager.RewindTimeInProgressEvent += OnRewindTime;
		gameOverPanel.gameObject.SetActive( false );
		deathmatchPanel.gameObject.SetActive( false );
		victoryPanel.gameObject.SetActive( false );
		inGameUI.gameObject.SetActive( false );
		startGamePanel.gameObject.SetActive( true );
	}
	private void OnDestroy()
	{
		GameManager.GameOverEvent -= OnGameOver;
		GameManager.PauseGameEvent -= SetPauseGame;
		GameManager.RewindTimeInProgressEvent -= OnRewindTime;
	}

	private void OnRewindTime( bool inProgress )
	{
		if (inProgress)
			rewindTip.SetActive( true );
		else
			rewindTip.SetActive( false );
	}

	public static void SetPauseGame(bool pause)
	{
		if (instance)
		{
			instance.inGameUI.SetActive( !pause );
			instance.inGameMenu.SetActive( pause );
		}
	}

	private static void OnGameOver(bool victory)
	{
		if (!instance)
			return;
		if (SceneManager.GetActiveScene().buildIndex > 3)
		{
			instance.deathmatchPanel.SetActive( true );
			return;
		}
		if (victory)
		{
			instance.victoryPanel.SetActive( true );
		}
		else
		{
			instance.gameOverPanel.SetActive( true );
		}
	}

	public void GoToMainMenu()
	{
		GameManager.GoToMainMenu();
	}


	public void ShowLeaderboard()
	{
		GameServicesManager.OpenLeaderboard();
	}

	public void ShowAchievements()
	{
		GameServicesManager.OpenAchievements();
	}

	public void SignInGPGS()
	{
		if (GameServicesManager.UserSignedIn)
			GameServicesManager.SignOutUser();
		else
			GameServicesManager.SignInUser(true);
	}
}
