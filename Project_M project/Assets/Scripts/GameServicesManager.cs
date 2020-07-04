using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using UnityEngine.SocialPlatforms;
using GooglePlayGames.BasicApi;
using System;

public class GameServicesManager : MonoBehaviour
{
    public static event Action<bool> Authenticated;
    public static bool UserSignedIn => PlayGamesPlatform.Instance.IsAuthenticated();

    private void Start()
    {
        Init();
    }
    private void Init()
    {
        Debug.Log( "initializing play services" );
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        SignInUser(false);
    }

    public static void SignInUser(bool withPrompt)
    {
        PlayGamesPlatform.Instance.Authenticate( withPrompt ? SignInInteractivity.CanPromptAlways : SignInInteractivity.CanPromptOnce, ( result ) => {
            if (result == SignInStatus.Success)
            {
                Authenticated?.Invoke( UserSignedIn );
            }
            else
            {
                Authenticated?.Invoke( UserSignedIn );
                Debug.LogError( $"PGS user sign in failed with result: {result}" );
            }
        } );
    }
    public static void SignOutUser()
    {
        PlayGamesPlatform.Instance.SignOut();
        Authenticated?.Invoke( false );
    }

    public static void OpenAchievements()
    {
        if (UserSignedIn)
        {
            PlayGamesPlatform.Instance.ShowAchievementsUI();
        }
        else
        {
            Debug.LogError( "Can not connect to google play services" );
        }
    }

    public static void OpenLeaderboard()
    {
        if (UserSignedIn)
        {
            PlayGamesPlatform.Instance.ShowLeaderboardUI();
        }
        else
        {
            Debug.LogError( "Can not connect to google play services" );
        }
    }

    public static void UnlockAchievement(string id )
    {
        if (UserSignedIn)
        {
            PlayGamesPlatform.Instance.ReportProgress( id, 100, ( bool success ) => {
                if (success)
                {
                    Debug.Log( $"successfully unlocked achievement" );
                }
                else
                {
                    Debug.LogError( "failed to unlocked achievement" );
                }
            } );
        }
        else
        {
            Debug.LogError( "Can not connect to google play services" );
        }
    }

    public static void SubmitScore(string tableID, int score )
    {
        if (UserSignedIn)
        {
            PlayGamesPlatform.Instance.ReportScore( score, tableID, ( bool success ) => {
                if (success)
                {
                    Debug.Log( $"score reported successfully" );
                }
                else
                {
                    Debug.LogError( "failed to report score" );
                }
            } );
        }
        else
        {
            Debug.LogError( "Can not connect to google play services" );
        }
    }

    public static void RecordEvent(string eventID, int increment )
    {
        if (UserSignedIn)
            PlayGamesPlatform.Instance.Events.IncrementEvent( "YOUR_EVENT_ID", 1 );
        else
            Debug.LogError( "Can not connect to google play services" );
    }
}
