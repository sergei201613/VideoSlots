using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BonusGame : MonoBehaviour
{
    public AudioClip[] audioClips;
    public Text timeCoins, timeScrath;
    public Button butCoins, butScrath;
    private void Start()
    {
        ReadTime();
        if (checkTime != null) StopCoroutine(checkTime);
        checkTime = StartCoroutine(UpdateTime());
        LoadMenuRank();
    }

    void LoadMenuRank()
    {
        StartCoroutine(PostData("getrank"));
    }

    public Image[] sprites;
    UserRank[] lstRank;
    public RankingController controller;
    IEnumerator PostData(string type)
    {
        string id = SystemInfo.deviceUniqueIdentifier;
        string url = "https://appstore.com.ru/SLOTS/PHP/Control.php";
        WWWForm form = new WWWForm();
        form.AddField("type", type);
        form.AddField("game", Application.identifier);
        form.AddField("name", PlayerPrefs.GetString("playerName", "PLAYER"));
        form.AddField("device", id);
        form.AddField("exp", PlayerPrefs.GetString("playerEXP", "0"));
        form.AddField("level", PlayerPrefs.GetInt("playerLevel", 1).ToString());
        form.AddField("credit", PlayerPrefs.GetString("mm", "0"));
        form.AddField("avatar", PlayerPrefs.GetString("playerAvatar", "-1"));
        WWW www = new WWW(url, form);
        while (!www.isDone)
            yield return new WaitForSeconds(0.1f);
        print(www.text);
        switch (type)
        {
            case "getrank":
#if UNITY_WSA
                string rs = www.text.Replace("{\"Items\":", "").Replace("]}", "]");
                lstRank = JsonConvert.DeserializeObject<UserRank[]>(rs);
#else
                lstRank = JsonHelper.FromJson<UserRank>(www.text);
#endif
                if (lstRank.Length > 0)
                {
                   for(int i = 0; i < 5; i++)
                    {
                        int a = -1;
                        if (i < lstRank.Length) break;
                        if (int.TryParse(lstRank[i].Avatar, out a))
                        {
                            if (a > 0)
                                sprites[i].sprite = controller.avatar[a];
                        }
                        else
                            StartCoroutine(GetAvatar(lstRank[i].Avatar, sprites[i]));
                    }
                    break;
                }
                yield break;
        }
    }

    IEnumerator GetAvatar(string avatar, Image sprite)
    {
        if (avatar != "" && avatar != "-1")
        {
            string url = "https://appstore.com.ru/SLOTS/PHP/avatars/" + Application.identifier + "/" + avatar + ".jpg";
            WWW www = new WWW(url);
            while (!www.isDone)
                yield return new WaitForSeconds(0.1f);
            if (string.IsNullOrEmpty(www.error))
            {
                Rect rec = new Rect(0, 0, www.texture.width, www.texture.height);
                sprite.sprite = Sprite.Create(www.texture, rec, new Vector2(0.5f, 0.5f), 100f);
            }
        }
        yield break;
    }

    Coroutine checkTime;

    void ReadTime()
    {
        long lastTimeCoins;
        if (long.TryParse(PlayerPrefs.GetString("lastGetCoins"), out lastTimeCoins))
        {
            DateTime oldDate = DateTime.FromBinary(lastTimeCoins);
            TimeSpan diff = DateTime.Now - oldDate;
            curTimeCoins = 10800 - diff.TotalSeconds;
        }

        else
            curTimeCoins = 0;

        long lastTimeScrath;
        if (long.TryParse(PlayerPrefs.GetString("lastGetScrath"), out lastTimeScrath))
        {
            DateTime oldDate = DateTime.FromBinary(lastTimeScrath);
            TimeSpan diff = DateTime.Now - oldDate;
            curTimeScrath = 18000 - diff.TotalSeconds;
        }
        else
            curTimeScrath = 0;
    }

    private void OnApplicationFocus(bool focus)
    {
        ReadTime();
        if (checkTime != null) StopCoroutine(checkTime);
        checkTime = StartCoroutine(UpdateTime());
    }

    public void GetCoinReward()
    {
        AudioSource.PlayClipAtPoint(audioClips[0], Camera.main.transform.position, 1f);
        PlayerPrefs.SetString("lastGetCoins", DateTime.Now.ToBinary().ToString());
        MenuManager.Instance.PlusMoney(250000);
        ReadTime();
    }

    public Animator animScratch;
    public Button butQuickScratch, butCollect;
    public GameObject cardScratch;
    public ScratchCardManager manager;
    public Text scratchReward;
    int rew = 0;
    public void GetScrathReward()
    {
        AudioSource.PlayClipAtPoint(audioClips[1], Camera.main.transform.position, 1f);
        cardScratch.SetActive(true);
        butCollect.gameObject.SetActive(false);
        rew = UnityEngine.Random.Range(100, 500);
        scratchReward.text = (rew * 1000).ToString("#,##0");
        butCollect.onClick.RemoveAllListeners();
        butCollect.onClick.AddListener(() => {
            manager.ResetScratchCard();
            PlayerPrefs.SetString("lastGetScrath", DateTime.Now.ToBinary().ToString());
            MenuManager.Instance.PlusMoney(rew * 1000);
            ReadTime();
            animScratch.SetTrigger("Close");
        });

        butQuickScratch.gameObject.SetActive(true);
        butQuickScratch.onClick.RemoveAllListeners();
        butQuickScratch.onClick.AddListener(() => {
            cardScratch.SetActive(false);
            butCollect.gameObject.SetActive(true);
        });

        animScratch.SetTrigger("Open");
    }

    double curTimeCoins, curTimeScrath;
    IEnumerator UpdateTime()
    {

        while (true)
        {
            if (curTimeCoins > 0)
                curTimeCoins--;
            else curTimeCoins = 0;

            if (curTimeScrath > 0)
                curTimeScrath--;
            else curTimeScrath = 0;

            if (curTimeCoins > 0)
            {
                string t = TimeSpan.FromSeconds(curTimeCoins).ToString();
                timeCoins.text = t.Remove(8, t.Length - 8);
            }
            else
                timeCoins.text = "GET NOW";


            if (curTimeScrath > 0)
            {
                string t = TimeSpan.FromSeconds(curTimeScrath).ToString();
                timeScrath.text = t.Remove(8, t.Length - 8);
            }
            else
                timeScrath.text = "SCRATCH CARD";
            butCoins.gameObject.SetActive(curTimeCoins == 0);
            butScrath.interactable = curTimeScrath == 0;

            yield return new WaitForSeconds(1f);
        }
    }
}
