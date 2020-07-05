using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Analytics;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public const int MaxLives = 3;

    public static GameManager instance;

    public static event Action<bool> PauseGameEvent;
    public static event Action<bool> GameOverEvent;
    public static event Action<int> ActivateTutorialEvent;

	public static event Action<bool> RewindTimeInProgressEvent;

    public static bool InMainMenu { get { return SceneManager.GetActiveScene().name == "Main menu"; } }

    public static bool AdsOn => !IAPManager.AdsRemoved;
    public static bool GamePaused { private set; get; }
    public static int LivesLeft { private set; get; } = MaxLives;

    public static int ShowFPS
    {
        set
        {
            PlayerPrefs.SetInt( "showFPS", value );
        }
        get
        {
            return PlayerPrefs.GetInt( "showFPS", 0 );
        }
    }
    public static int PPActive
    {
        set
        {
            PlayerPrefs.SetInt( "postProcessing", value );
        }
        get
        {
            return PlayerPrefs.GetInt( "postProcessing", 1 );
        }
    }

    public static int Quality
    {
        set
        {
            QualitySettings.SetQualityLevel( value );
            PlayerPrefs.SetInt( "quality", value );
        }
        get
        {
            var qIndex = PlayerPrefs.GetInt( "quality", 1 );
            qIndex = Mathf.Clamp( qIndex, 0, 1 );
            QualitySettings.SetQualityLevel( qIndex );
            return QualitySettings.GetQualityLevel();
        }
    }

    public static int TutorialActive { 
        set 
        {
            PlayerPrefs.SetInt( "Tutorial", value);
        } 
        get 
        {
            return PlayerPrefs.GetInt( "Tutorial", 1 );
        }
    }

    public static int DeathMatchAvailable
    {
        set
        {
            PlayerPrefs.SetInt( "DeathMatchOn", value );
        }
        get
        {
            return PlayerPrefs.GetInt( "DeathMatchOn", 0 );
        }
    }

    public static int GameComplete
    {
        set
        {
            PlayerPrefs.SetInt( "GameComplete", value );
        }
        get
        {
            return PlayerPrefs.GetInt( "GameComplete", 0 );
        }
    }

	internal static void RewardedVideoFailed()
	{
        Debug.Log( "failed to display rewarded video" );
	}

    public static bool TestMode => instance.testMode;
    public static bool Deathmatch => SceneManager.GetActiveScene().buildIndex > 3;
	public static float WinningDistance { get { return instance.winningDistance; } }
    public static float RewindTime => instance.rewindTime;

    public static bool IsGameOver { get; private set; }

    [SerializeField]
    private bool testMode;
    [SerializeField]
    private AudioSource music;
    [SerializeField]
    private float winningDistance = 2000;
    [SerializeField] private float rewindTime;
    [SerializeField] private GameObject volumes;

    private ScoreManager score;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
            DontDestroyOnLoad( this );
        }
        else if (this != instance)
        {
            Destroy( gameObject );
            return;
        }

        InitGame();
        SceneManager.sceneLoaded += OnSceneLoaded;
        instance.score = new ScoreManager();
        music?.Play();
    }

    private void InitGame()
    {
        if (Application.isEditor)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 30;
        }

        volumes.SetActive( true );
        SetTutorialActive( TutorialActive );
    }

    private void OnSceneLoaded( Scene scene, LoadSceneMode mode )
    {
        IsGameOver = false;
        int index = scene.buildIndex;
        if (index == 0)
        {
            SetStopGameplay( false );
        }
        else
        {
            SetStopGameplay( true );
            Analytics.CustomEvent( "level_start", new Dictionary<string, object>
            {
                { "level_index", index},
            } );
        }
    }

    public static void StartGame()
    {
        if (instance.score == null)
            instance.score = new ScoreManager();
        IsGameOver = false;
        SetPauseGame( false );
    }
    public static void LoadLevel(int levelID )
    {
        LivesLeft = MaxLives;
        IsGameOver = false;
        SetStopGameplay( true );
        SceneManager.LoadScene( levelID );
    }
    public static void LoadNext()
    {
        LoadLevel( SceneManager.GetActiveScene().buildIndex + 1 );
    }

    public static void Reload()
    {
        IsGameOver = false;
        SceneManager.LoadScene( SceneManager.GetActiveScene().buildIndex );
    }

    public static void Respawn()
    {
        int index = SceneManager.GetActiveScene().buildIndex;
        Analytics.CustomEvent( "respawned", new Dictionary<string, object>
            {
                { "level_index", index},
            } );
        if (LivesLeft > 0 && index <= 3)
            instance.StartCoroutine( instance.RespawnRoutine() );
        else
            Reload();
    }
    private IEnumerator RespawnRoutine()
    {
        LivesLeft--;
        IsGameOver = false;
        float startTime = Time.time;
        RewindTimeInProgressEvent?.Invoke( true );
        SetStopGameplay( false );
        while (Time.time - startTime < rewindTime)
        {
            yield return null;
            if (Input.anyKeyDown || Input.touchCount > 0)
            {
                break;
            }
        }
        RewindTimeInProgressEvent?.Invoke( false );
    }

    public static void GameOver(bool victory)
    {
        IsGameOver = true;
        instance.StartCoroutine( GameOverCoroutine(victory) );
    }

    public static void SetPauseGame(bool pause)
    {
        if (GamePaused == pause)
            return;
        SetStopGameplay( pause );
        PauseGameEvent?.Invoke(pause);
    }

    private static void SetStopGameplay(bool stop )
    {
        if (stop)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;
        GamePaused = stop;
    }

    public static void QuitGame()
    {
        Application.Quit();
    }

    public static void GoToMainMenu()
    {
        SceneManager.LoadScene( "Main menu" );
    }

    public static void SetTutorialActive(int active )
    {
        TutorialActive = active;
        ActivateTutorialEvent?.Invoke( active );
    }

    private static IEnumerator GameOverCoroutine( bool victory )
    {
        yield return new WaitForSecondsRealtime( 0.25f );
        int index = SceneManager.GetActiveScene().buildIndex;
        if (victory || index > 3)
        {
            ScoreManager.UpdateAchievements( SceneManager.GetActiveScene().name );
            SoundManager.PlaySound( SoundManager.Sound.Victory );
            if (index == 3)
            {
                GameComplete = 1;
                DeathMatchAvailable = 1;
            }
        }
        else
        {
            SoundManager.PlaySound( SoundManager.Sound.Defeat );
        }
        Analytics.CustomEvent( "level_complete", new Dictionary<string, object>
            {
                { "level_index", index},
            } );
        SetStopGameplay( true );
        GameOverEvent?.Invoke( victory );
    }

    public static void SetTimeScale( float scale )
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
}
