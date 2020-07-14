using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GoogleMobileAds.Api;
using GoogleMobileAds.Common;

using GoogleMobileAds.Api.Mediation.UnityAds;


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
    private RewardedAd rewardedAd;

    public delegate void RewardAdCallBack(bool x);

    public LoginClient loginClient;

    public RewardAdCallBack rwCB;

    void Start()
    {
        Debug.Log("AdManager Started");
        List<string> deviceIds = new List<string>();
        deviceIds.Add("19EB01B237AD9FBA44A428C0A30235E7");
       /* RequestConfiguration requestConfiguration = new RequestConfiguration
            .Builder()
            .SetTestDeviceIds(deviceIds)
            .build();
        MobileAds.SetRequestConfiguration(requestConfiguration);*/
        Debug.Log("Test devices are set");
        MobileAds.Initialize(HandleInitCompleteAction);
        UnityAds.SetGDPRConsentMetaData(true);
        loginClient = GameObject.Find("LoginClient").GetComponent<LoginClient>();
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
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RequestInterstitial()
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
        if(adsInitiated && (interstitial==null || !interstitial.IsLoaded()))
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


    public void RequestRewardAd()
    {
        string adUnitId = "ca-app-pub-3098073576690056/5557037375";
        //string adUnitId = "ca-app-pub-3940256099942544/1033173712";

        if (rewardedAd != null)
        {
            rewardedAd = null;
        }


        this.rewardedAd = new RewardedAd(adUnitId);

        ServerSideVerificationOptions.Builder ssvb = new ServerSideVerificationOptions.Builder();
        ssvb.SetUserId(loginClient.awsCredentials.GetIdentityId());
        ServerSideVerificationOptions ssv = ssvb.Build();
        this.rewardedAd.SetServerSideVerificationOptions(ssv);

        this.rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        this.rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        this.rewardedAd.OnAdOpening += HandleRewardedAdOpening;
        this.rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;

        AdRequest request = new AdRequest.Builder().Build();
        this.rewardedAd.LoadAd(request);
    }

    public void LoadRewardAd()
    {
        if (adsInitiated && (rewardedAd == null || !rewardedAd.IsLoaded()))
        {
            Debug.Log("ad is not loaded, requesting to load");
            RequestRewardAd();
        }
        else
        {
            Debug.Log("Add is loaded already");
        }
    }

    public void HandleRewardedAdLoaded(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardedAdLoaded event received");
    }

    public void HandleRewardedAdFailedToLoad(object sender, AdErrorEventArgs args)
    {
        Debug.Log("HandleRewardedAdFailedToLoad event received with message: "+ args.Message);
    }

    public void HandleRewardedAdOpening(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardedAdOpening event received");
    }

    public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        Debug.Log("HandleRewardedAdFailedToShow event received with message: "+ args.Message);
    }

    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        rwCB(false);
        Debug.Log("HandleRewardedAdClosed event received");
    }

    public void HandleUserEarnedReward(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        rwCB(true);
        Debug.Log("HandleRewardedAdRewarded event received for "+ amount.ToString() + " " + type);
    }

    public void DisplayRewardAd(RewardAdCallBack rwCB)
    {
        if (this.rewardedAd.IsLoaded())
        {
            this.rewardedAd.Show();
            this.rwCB = rwCB;
        }
        else
        {
            Debug.Log("Reward Ad is unloaded");
        }
    }


    void OnDestroy()
    {
        if (interstitial != null)
        {
            interstitial.Destroy();
        }
        if (rewardedAd != null)
        {
            rewardedAd = null;
        }
    }
}
