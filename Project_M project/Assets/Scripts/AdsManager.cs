using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour, IUnityAdsListener
{
    [SerializeField] private bool enabled;
    [SerializeField] private bool testMode = true;

    public static AdsManager instance;
    public static event Action<bool> RewardedVideoWatched;
	public static string GameID { get; } = "3673383";
    public static bool TestMode => instance.testMode;

    private static string rewarderVideoID = "rewardedVideo";
    private static string bannerID = "banner";
    IEnumerator Start()
    {
        instance = this;
        if (!enabled)
            yield break;
        if (IAPManager.AdsRemoved)
        {
            gameObject.SetActive( false );
            yield break;
        }
        IAPManager.AdsRemovedEvent += OnAdsRemoved;

        Advertisement.AddListener( this );
        Advertisement.Initialize( GameID, TestMode );

        while (!Advertisement.isInitialized && !Advertisement.IsReady())
        {
            yield return null;
        }

        Advertisement.Banner.SetPosition( BannerPosition.BOTTOM_CENTER );
        Advertisement.Banner.Show( bannerID );
    }

    private void OnAdsRemoved()
    {
        Advertisement.Banner.Hide(true);
        gameObject.SetActive( false );
        IAPManager.AdsRemovedEvent -= OnAdsRemoved;
    }

    public static void ShowRewardedVideo()
    {
        Advertisement.Show( rewarderVideoID );
    }
    public void OnUnityAdsDidError( string message )
    {
        Debug.Log( $"ads failed: {message}" );
    }

    public void OnUnityAdsDidFinish( string placementId, ShowResult showResult )
    {
        if (placementId == rewarderVideoID)
        {
            if (showResult == ShowResult.Finished)
            {
                RewardedVideoWatched?.Invoke(true);
            }
            else if (showResult == ShowResult.Skipped)
            {
                RewardedVideoWatched?.Invoke( false );
            }
            else
            {
                GameManager.RewardedVideoFailed();
            }
        }
    }

    public void OnUnityAdsDidStart( string placementId )
    {
    }

    public void OnUnityAdsReady( string placementId )
    {
    }

	internal static void DisableAds()
	{
		throw new NotImplementedException();
	}
}
