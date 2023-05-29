using GoogleMobileAds;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using System.Collections;
using UnityEngine;

public class Interstitial : MonoBehaviour
{
    // These ad units are configured to always serve test ads. // ca-app-pub-3940256099942544/1033173712 for test 
#if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-1595077627022236/2009014145";
#elif UNITY_IPHONE
      private string _adUnitId = "ca-app-pub-1595077627022236/2009014145";
#else
      private string _adUnitId = "unused";
#endif

    private InterstitialAd interstitialAd;
    public static Interstitial Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }

        MobileAds.RaiseAdEventsOnUnityMainThread = true; // Main Thread
    }

    public void Start()
    {


        MobileAds.Initialize(HandleInitCompleteAction);


        StartCoroutine(LoadingAd());
    }

    private IEnumerator LoadingAd()
    {
        yield return new WaitForSeconds(5f);
        LoadInterstitialAd();
    }

    public void LoadInterstitialAd()
    {
        // Clean up the old ad before loading a new one.
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            //interstitialAd = null;
        }

        Debug.Log("Loading the interstitial ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest.Builder()
                .AddKeyword("unity-admob-sample")
                .Build();

        // send the request to load the ad.
        InterstitialAd.Load(_adUnitId, CreateAdRequest(),
            (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                interstitialAd = ad;
            });
    }

    private AdRequest CreateAdRequest()
    {
        return new AdRequest.Builder()
            .AddKeyword("unity-admob-sample")
            .Build();
    }

    public void ShowInterstitialAd()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            interstitialAd.Show();
        }
        else
        {
            //Debug.LogError("Interstitial ad is not ready yet.");
            DestroyInterstitialAd();
            LoadInterstitialAd();
        }
    }

    public void DestroyInterstitialAd()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
        }
    }

    private void HandleInitCompleteAction(InitializationStatus initstatus)
    {
        Debug.Log("Initialization complete.");

        // Callbacks from GoogleMobileAds are not guaranteed to be called on
        // the main thread.
        // In this example we use MobileAdsEventExecutor to schedule these calls on
        // the next Update() loop.
        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            // statusText.text = "Initialization complete.";
        });
    }

    private void OnDestroy()
    {
        if (interstitialAd != null)
            interstitialAd.Destroy();
    }
}