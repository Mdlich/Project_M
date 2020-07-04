using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour, IStoreListener
{
    public static event Action AdsRemovedEvent;
    public static bool AdsRemoved 
    {   private set
        {
            if ((!instance.adsRemoved.HasValue || !instance.adsRemoved.Value) && value)
            {
                AdsRemovedEvent?.Invoke();
            }
            instance.adsRemoved = value;
        }
        get 
        {
            if (!instance)
                return false;
            if (!instance.adsRemoved.HasValue)
                instance.adsRemoved = instance.GetAdsRemoved();
            return instance.adsRemoved.Value;
        }
    }

    public static IAPManager instance;

    private static IStoreController m_StoreController;
    private static IExtensionProvider m_StoreExtensionProvider;

	private readonly string noAdsID = "slime_ascent.remove_ads";
    private bool? adsRemoved;

    public void InitializePurchasing()
    {
        if (IsInitialized())
        { return; }
        Debug.Log( "Initializing IAP" );
        var builder = ConfigurationBuilder.Instance( StandardPurchasingModule.Instance() );

        builder.AddProduct( noAdsID, ProductType.NonConsumable );
        UnityPurchasing.Initialize( this, builder );
    }


    private bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    public void BuyRemoveAds()
    {
        BuyProductID( noAdsID );
    }
    private bool GetAdsRemoved()
    {
        if (!IsInitialized())
        {
            Debug.Log( "called before init" );
            return false;
        }
        Product product = m_StoreController.products.WithID( noAdsID );
        if (product != null && product.hasReceipt)
        {
            return true;
        }
        return false;
    }

    public PurchaseProcessingResult ProcessPurchase( PurchaseEventArgs args )
    {
        if (string.Equals( args.purchasedProduct.definition.id, noAdsID, StringComparison.Ordinal ))
        {
            AdsRemoved = true;
        }
        else
        {
            Debug.Log( "Purchase Failed" );
        }
        return PurchaseProcessingResult.Complete;
    }

    private void Awake()
    {
        TestSingleton();
        if (m_StoreController == null)
        { InitializePurchasing(); }
    }

    private void TestSingleton()
    {
        if (instance != null)
        { Destroy( gameObject ); return; }
        instance = this;
    }

    void BuyProductID( string productId )
    {
        if (IsInitialized())
        {
            Product product = m_StoreController.products.WithID( productId );
            if (product != null && product.hasReceipt)
            {
                if (productId == noAdsID)
                {
                    Debug.Log( "Product already owned" );
                    AdsRemoved = true;
                }
            }
            else if (product != null && product.availableToPurchase)
            {
                Debug.Log( string.Format( "Purchasing product asychronously: '{0}'", product.definition.id ) );
                m_StoreController.InitiatePurchase( product );
            }
            else
            {
                Debug.Log( "BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase" );
            }
        }
        else
        {
            Debug.Log( "BuyProductID FAIL. Not initialized." );
        }
    }

    public void RestorePurchases()
    {
        if (!IsInitialized())
        {
            Debug.Log( "RestorePurchases FAIL. Not initialized." );
            return;
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log( "RestorePurchases started ..." );

            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions( ( result ) => {
                Debug.Log( "RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore." );
            } );
        }
        else
        {
            Debug.Log( "RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform );
        }
    }

    public void OnInitialized( IStoreController controller, IExtensionProvider extensions )
    {
        Debug.Log( "OnInitialized: PASS" );
        m_StoreController = controller;
        m_StoreExtensionProvider = extensions;

        AdsRemoved = GetAdsRemoved();
    }


    public void OnInitializeFailed( InitializationFailureReason error )
    {
        Debug.Log( "OnInitializeFailed InitializationFailureReason:" + error );
    }

    public void OnPurchaseFailed( Product product, PurchaseFailureReason failureReason )
    {
        Debug.Log( string.Format( "OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason ) );
    }
}
