using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GoogleMobileAds.Api;
using GoogleMobileAds.Common;


public class AdManager : MonoBehaviour
{

    private static AdManager adManager = null;
    private bool adsInitiated = false;
    private bool adLoaded = false;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if (adManager == null)
        {
            adManager = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private InterstitialAd interstitial;

    void Start()
    {
        Debug.Log("AdManager Started");
        List<string> deviceIds = new List<string>();
        deviceIds.Add("19EB01B237AD9FBA44A428C0A30235E7");
        RequestConfiguration requestConfiguration = new RequestConfiguration
            .Builder()
            .SetTestDeviceIds(deviceIds)
            .build();
        MobileAds.SetRequestConfiguration(requestConfiguration);
        Debug.Log("Test devices are set");
        MobileAds.Initialize(HandleInitCompleteAction);
    }

    private void HandleInitCompleteAction(InitializationStatus initstatus)
    {
        // Callbacks from GoogleMobileAds are not guaranteed to be called on
        // main thread.
        // In this example we use MobileAdsEventExecutor to schedule these calls on
        // the next Update() loop.
        MobileAdsEventExecutor.ExecuteInUpdate(() => {
            Debug.Log("Initialization complete");
            adsInitiated = true;
            RequestInterstitial();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void RequestInterstitial()
    {
        string adUnitId = "ca-app-pub-3098073576690056/1275464862";
        //string adUnitId = "ca-app-pub-3940256099942544/1033173712";

        if (interstitial != null)
        {
            interstitial.Destroy();
        }


        this.interstitial = new InterstitialAd(adUnitId);

        this.interstitial.OnAdLoaded += HandleOnAdLoaded;
        this.interstitial.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        this.interstitial.OnAdOpening += HandleOnAdOpened;
        this.interstitial.OnAdClosed += HandleOnAdClosed;
        this.interstitial.OnAdLeavingApplication += HandleOnAdLeavingApplication;

        AdRequest request = new AdRequest.Builder().Build();
        this.interstitial.LoadAd(request);
    }

    public void LoadAd()
    {
        if(adsInitiated && !interstitial.IsLoaded())
        {
            Debug.Log("ad is not loaded, requesting to load");
            RequestInterstitial();
        }
        else
        {
            Debug.Log("Add is loaded already");
        }
    }

    public void HandleOnAdLoaded(object sender, EventArgs args)
    {
        Debug.Log("HandleAdLoaded event received");
    }

    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        adLoaded = false;
        Debug.Log("HandleFailedToReceiveAd event received with message: "+ args.Message);
    }

    public void HandleOnAdOpened(object sender, EventArgs args)
    {
        Debug.Log("HandleAdOpened event received");
        if (!interstitial.IsLoaded())
        {
            Debug.Log("Ad is not loaded");
        }
    }

    public void HandleOnAdClosed(object sender, EventArgs args)
    {
        Debug.Log("HandleAdClosed event received");
    }

    public void HandleOnAdLeavingApplication(object sender, EventArgs args)
    {
        Debug.Log("HandleAdLeavingApplication event received");
    }

    public void DisplayAd()
    {
        if (this.interstitial.IsLoaded())
        {
            this.interstitial.Show();
        }
        else
        {
            Debug.Log("Interstitial is unloaded"); 
        }
    }



    void OnDestroy()
    {
        if (interstitial != null)
        {
            interstitial.Destroy();
        }
    }
}
