using UnityEngine;

public class AdController : MonoBehaviour
{
    public static AdController Instance;
    public bool usingAdmob;
    //[Range(0, 100)]
    //public int percentAdmob;

    public bool usingChartboost;
    //[Range(0, 100)]
    //public int percentCharboost;

    private void Awake()
    {
        if(Instance)
        {
            DestroyImmediate(this.gameObject);
            return;
        }
        Instance = this;
        admob = GetComponent<AdAdmob>();
        DontDestroyOnLoad(this.gameObject);
        //chartboost = GetComponent<AdChartboost>();
    }

    AdAdmob admob;
    //AdChartboost chartboost;
    private void Start()
    {
        if (usingAdmob)
        {
            admob.CallStart();
        }
        //else if (usingChartboost)
        //{
        //    chartboost.CallStart();
        //}
        //RequestBanner();
    }

    public void RequestBanner()
    {
        //if (usingAdmob)
        //{
            admob.showBannerAd();
        //}
        //else if (usingChartboost)
        //{

        //}
    }

    public void DestroyBanner()
    {
        //if (usingAdmob)
        //{
            admob.removeBannerAd();
        //}
        //else if (usingChartboost)
        //{

        //}
    }

    bool autoshowInterstital = false;
    public void RequestInterstitial(bool autoshow = false)
    {
        autoshowInterstital = autoshow;
        if (usingAdmob)
        {
            admob.loadInterstitial();
        }
        //else if (usingChartboost)
        //{         
        //    //chartboost.CacheIntersitial();
        //}
    }

    public void CallbackLoadedInterstitial()
    {
        if (autoshowInterstital)
            ShowInterstitial();
    }

    public void ShowInterstitial()
    {
        if (usingAdmob)
        {
            admob.showInterstitial();
        }
        //else if (usingChartboost)
        //{
        //    if (Random.value < DisableOnline.Instance.per)
        //        admob.showInterstitial();
        //    //else
        //    //    chartboost.ShowInterstitial();
        //}
    }

    bool autoshowReward = false;
    public void RequestReward(bool autoshow = false, System.Action action = null)
    {
        autoshowReward = autoshow;
        callbackAction = action;
        if (usingAdmob)
        {
            admob.loadRewardVideo();
        }
        //else if (usingChartboost)
        //{
        //    chartboost.CacheRewardVideo();
        //}
    }

    public bool HaveRewardVideo()
    {
        return admob.HaveRewardVideo();
    }

    System.Action callbackAction;
    public void ShowReward(System.Action action = null) {
        callbackAction = action;
        if (usingAdmob)
        {
            admob.showRewardVideo();
        }
        //else if (usingChartboost)
        //{
        //    if (Random.value < DisableOnline.Instance.per)
        //        admob.showRewardVideo();
        //    else
        //        chartboost.ShowRewardVideo();
        //}
    }

    public void CallbackLoadedReward()
    {
        if (autoshowReward)
            ShowReward();
    }

    public void CallbackSuccessReward()
    {
        if (callbackAction != null)
            callbackAction();
    }
}
