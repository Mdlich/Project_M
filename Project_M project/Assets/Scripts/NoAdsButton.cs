using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoAdsButton : MonoBehaviour
{
	private void Awake()
	{
		IAPManager.AdsRemovedEvent += OnAdsRemoved;
	}
	private void Start()
	{
		if (IAPManager.AdsRemoved)
		{
			gameObject.SetActive( false );
			return;
		}
	}

	private void OnDestroy()
	{
		IAPManager.AdsRemovedEvent -= OnAdsRemoved;
	}
	private void OnAdsRemoved()
	{
		gameObject.SetActive( false );
		IAPManager.AdsRemovedEvent -= OnAdsRemoved;
	}

	public void DisableAds()
	{
		IAPManager.instance.BuyRemoveAds();
	}
}
