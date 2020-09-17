using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
//using Facebook.Unity;

public class MenuManager : MonoBehaviour {
    public static MenuManager Instance;
    public GameObject gameObjectLoad;
    private void Awake()
    {
        Instance = this;
        //InitFB();
    }

    // Initialize the Facebook for tracking
    //void InitFB()
    //{
    //    if (!FB.IsInitialized)
    //    {   
    //        FB.Init(
    //            ()=> {}, 
    //            (isGameShown) => {
    //            if (!isGameShown)
    //                Time.timeScale = 0;
    //            else
    //                Time.timeScale = 1;
    //        });
    //    }
    //    else
    //        FB.ActivateApp();
    //}

    void Start()
    {
        LoadGameList();
        StartCoroutine(DestroyVideo());

        LoadIAPList();
        double.TryParse(PlayerPrefs.GetString("mm", "10000000"), out money);
        moneyLabel.text = (money).ToString("#,##0");
        if (Time.time < 13)
        {
            //float t = 3f;
            //Invoke("DestroyVideo", 5 + t);
            //Invoke("PlayVideo", t);
        }
    }

    public Animator superdealIAP;
    public void SuperDeal()
    {
        int i = PlayerPrefs.GetInt("soffer", 0);
        if (i == 0)
            SpecialOffer(0);
        else if (i == 1)
            SpecialOffer(1);
        else if (i == 2)
            SpecialOffer(2);
        else
        {
            superdealIAP.SetTrigger("Open");
            SuperDeal("Open");
            ChangeSound(1);
        }
    }

    public Animator[] specialOffer;
    public AudioClip specialClip;
    public void SpecialOffer(int i)
    {
        specialOffer[i].SetTrigger("Open");
        if(specialClip)
        AudioSource.PlayClipAtPoint(specialClip, Camera.main.transform.position, 1f);
    }

    public AudioSource audioSource;
    public AudioClip[] audioClips;
    public void ChangeSound(int index)
    {
        if (index >= audioClips.Length || !audioClips[index]) return;
        audioSource.clip = audioClips[index];
        audioSource.Play();
    }

    void PlayVideo()
    {
        gameObjectLoad.transform.GetChild(2).GetComponent<VideoPlayer>().Play();
        gameObjectLoad.transform.GetChild(2).GetComponent<RawImage>().color = new Color(1f, 1f, 1f, 1f);
    }

    IEnumerator DestroyVideo()
    {
        //Destroy(gameObjectLoad.transform.GetChild(2).GetComponent<VideoPlayer>());
        //Destroy(gameObjectLoad.transform.GetChild(2).gameObject);

        while (true)
        {
            if (AssetBundleController.Instance.bundleUI)
            {
                gameObjectLoad.SetActive(false);
                StopCoroutine(DestroyVideo());
                break;
            }
            yield return new WaitForSeconds(.5f);
        }
    }

    public Transform parentSuper;
    public void SuperDeal(string trigger)
    {
        StartCoroutine(FXSuperDeal(trigger));
    }

    IEnumerator FXSuperDeal(string trigger)
    {
        yield return new WaitForSeconds(0.1f);
        parentSuper.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

        foreach (Transform tran in parentSuper)
        {
            tran.gameObject.GetComponent<Animator>().SetTrigger(trigger);
            yield return new WaitForSeconds(0.25f);
        }
        yield break;
    }

    public GameObject itemIAP;
    public void LoadIAPList()
    {
        itemIAP.SetActive(false);
        for(int i =0; i< Purchaser.Instance.infomationProducts.Count; i++)
        {
            var p = Purchaser.Instance.infomationProducts[i];
            if(p.Modification == Modification.Coin && p.ID != "com.slots.credit07" && p.ID != "com.slots.credit08")
            {
                GameObject temp = Instantiate(itemIAP, itemIAP.transform.parent, false);
                temp.GetComponent<ItemIAP>().UpdateData(i);
                temp.SetActive(true);
            }
        }
    }

    double money;
    public void PlusMoney(double val)
    {
        SetMoney(money + val);
    }

    void SetMoney(double val)
    {
        tweenMoney = money;
        money = val;
        TweenMoney(money - tweenMoney);
        if (money < 0) money = 0;
        PlayerPrefs.SetString("mm", money.ToString());
        PlayerPrefs.Save();
    }

    public Text moneyLabel;
    void TweenMoneyUpdate()
    {
        moneyLabel.text = (tweenMoney).ToString("#,##0");
    }

    double tweenMoney;
    public CoinFly fxCoin;
    void TweenMoney(double val)
    {
        fxCoin.RunAnim(3);
        DOTween.To(() => tweenMoney, x => tweenMoney = x, tweenMoney + val, 3).OnUpdate(() => {
            moneyLabel.text = (tweenMoney).ToString("#,##0");
        }).SetDelay(1f);
    }

    public GameObject itemGame;
    [HideInInspector]
    public int currentLevel = 0;
    public void LoadGameList()
    {
        currentLevel = PlayerPrefs.GetInt("playerLevel", 1);
        foreach (Transform tran in itemGame.transform.parent)
            if (tran.gameObject != itemGame) Destroy(tran.gameObject);

        for (int i = 0; i < AssetBundleController.Instance.bundleDatas.Length; i++)
        {
            GameObject temp;
            if (i == 0)
                temp = itemGame;
            else
                temp = Instantiate(itemGame, itemGame.transform.parent, false);

            temp.GetComponent<ItemGame>().UpdateData(i);
            temp.SetActive(true);
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}
