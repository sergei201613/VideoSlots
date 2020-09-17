using UnityEngine;
using GoogleMobileAds.Api;

public class AdAdmob : MonoBehaviour
{
    public string bannerAndroid, bannerIOS, interAnroid, interIOS, rewardAndroid, rewardIOS;
    private string bannerAdId, interstitialAdId, rewardVideoAdId;
    public bool isOnTop;
    public bool useBanner = false;
    private RewardBasedVideoAd rewardBasedVideo;
    private BannerView bannerView;
    private InterstitialAd interstitial;

    void Awake()
    {
        AddID();
    }

    void AddID() {
#if UNITY_ANDROID
        bannerAdId = bannerAndroid;
        interstitialAdId = interAnroid;
        rewardVideoAdId = rewardAndroid;
#else
        bannerAdId = bannerIOS;
        interstitialAdId = interIOS;
        rewardVideoAdId = rewardIOS;
#endif
    }

    public void CallStart()
    {
        loadRewardVideo();
        loadInterstitial();
        if (useBanner) showBannerAd();
    }

    public void showBannerAd()
    {
        AddID();

        if (isOnTop)
            bannerView = new BannerView(bannerAdId, AdSize.Banner, AdPosition.Top);
        else
            bannerView = new BannerView(bannerAdId, AdSize.Banner, AdPosition.Bottom);
        AdRequest request = new AdRequest.Builder().Build();
        bannerView.LoadAd(request);
    }

    public void hideBannerAd()
    {
        if (bannerView != null)
            bannerView.Hide();
    }

    public void removeBannerAd()
    {
        if (bannerView != null)
            bannerView.Destroy();
    }

    public void loadInterstitial()
    {
        AddID();

        interstitial = new InterstitialAd(interstitialAdId);
        AdRequest request = new AdRequest.Builder().Build();
        interstitial.LoadAd(request);
    }

    public void showInterstitial()
    {
        if (!interstitial.IsLoaded())
            loadInterstitial();
        interstitial.Show();
        interstitial.OnAdOpening -= onInterstitialEvent;
        interstitial.OnAdClosed -= onInterstitialCloseEvent;
        interstitial.OnAdOpening += onInterstitialEvent;
        interstitial.OnAdClosed += onInterstitialCloseEvent;
    }

    public bool CheckHaveInterAds()
    {
        return interstitial.IsLoaded();
    }

    void onInterstitialEvent(object sender, System.EventArgs args)
    {

    }
    void onInterstitialCloseEvent(object sender, System.EventArgs args)
    {
        loadInterstitial();
    }

    AdsState rewardState = AdsState.None;
    public void loadRewardVideo()
    {
        if (rewardState == AdsState.Loaded || rewardState == AdsState.Loading) return;
        AddID();
        rewardBasedVideo = RewardBasedVideoAd.Instance;
        AdRequest request = new AdRequest.Builder().Build();
        rewardBasedVideo.LoadAd(request, rewardVideoAdId);
        rewardState = AdsState.Loading;
    }

    public void showRewardVideo()
    {
        if (rewardBasedVideo.IsLoaded())
        {
            rewardBasedVideo.OnAdRewarded -= onRewardedVideoEvent;
            rewardBasedVideo.OnAdClosed -= onRewardedVideoSkippedEvent;
            rewardBasedVideo.OnAdFailedToLoad -= onRewardedVideoFailedEvent;

            rewardBasedVideo.OnAdRewarded += onRewardedVideoEvent;
            rewardBasedVideo.OnAdClosed += onRewardedVideoSkippedEvent;
            rewardBasedVideo.OnAdFailedToLoad += onRewardedVideoFailedEvent;
            rewardState = AdsState.None;
            rewardBasedVideo.Show();
#if !UNITY_EDITOR
            Time.timeScale = 0;
#endif
        }
        else
        {
            loadRewardVideo();
        }
    }

    public bool HaveRewardVideo()
    {
        return rewardBasedVideo.IsLoaded();
    }

    void onRewardedVideoSkippedEvent(System.Object sender, System.EventArgs args)
    {
        rewardState = AdsState.None;
        Time.timeScale = 1;
        loadRewardVideo();     
    }

    void onRewardedVideoFailedEvent(System.Object sender, System.EventArgs args)
    {
        rewardState = AdsState.Error;
        Time.timeScale = 1;
        loadRewardVideo();
        //AdController.Instance.CallbackSuccessReward();
    }

    void onRewardedVideoEvent(object sender, Reward args)
    {
        rewardState = AdsState.None;
        string type = args.Type;
        double amount = args.Amount;
        Time.timeScale = 1;
        loadRewardVideo();
        CancelInvoke("RewardVideo");
        Invoke("RewardVideo", 0.1f);
    }

    void RewardVideo()
    {
        AdController.Instance.CallbackSuccessReward();
    }
}

enum AdsState
{
    None,
    Created,
    Loading,
    Loaded,
    Error
}