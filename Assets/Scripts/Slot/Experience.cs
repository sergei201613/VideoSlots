using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Experience : MonoBehaviour
{
    public int vLevel = 1;
    public double vCurrExp = 0;
    public double vExpLeft = 10;
    public double vExpMod = 1.15f;

    public GameObject fxLVUP;
    public GameObject fxUnlockLV;
    public GameObject fxRate;
    public Button butRate;
    public Image levelProcess;
    public Text levelValue;
    public static Experience Instance;
    double allEXP = 0;

    private void Awake()
    {
        Instance = this;
        vLevel = PlayerPrefs.GetInt("playerLevel", 1);
        string ex = PlayerPrefs.GetString("playerEXP", "0");
        double.TryParse(ex, out allEXP);

        ex = PlayerPrefs.GetString("playerCurEXP", "0");
        double.TryParse(ex, out vCurrExp);

        vExpLeft = ExperiencePoints(vLevel);
        if (levelValue) levelValue.text = vLevel.ToString();
        if (levelProcess) levelProcess.fillAmount = (float)vCurrExp / (float)vExpLeft;
    }

    public double ExperiencePoints(int level)
    {
        double growthModifier = vExpMod;
        return (double)((level * PayData.betline[3]) * (level * growthModifier));
    }

    public void SendNotification(string[] content)
    {
        //NotificationManager.CancelAll();
        //if (content.Length == 1)
        //    NotificationManager.Send(TimeSpan.FromSeconds(SecondOnDate()), "A Night in Vegas Casino", content[0], new Color(1, 0.3f, 0.15f));
        //else
        //{
        //    string str = "";
        //    foreach (var c in content) str += c + "\n";

        //    var notificationParams = new NotificationParams
        //    {
        //        Id = 1,
        //        Delay = TimeSpan.FromSeconds(SecondOnDate()),
        //        Title = "A Night in Vegas Casino",
        //        Message = str,
        //        Ticker = "Have Fun!!",
        //        Multiline = true
        //    };
        //    NotificationManager.SendCustom(notificationParams);
        //}

        //NotificationManager.Send(TimeSpan.FromSeconds(SecondOnDate() + 86400 * 3), "A Night in Vegas Casino", "How are you? We miss you! Become a billionaire with us!!", new Color(1, 0.3f, 0.15f));
        //NotificationManager.Send(TimeSpan.FromSeconds(SecondOnDate() + 86400 * 7), "A Night in Vegas Casino", "How are you? We miss you! Become a billionaire with us!!", new Color(1, 0.3f, 0.15f));
        //NotificationManager.Send(TimeSpan.FromSeconds(SecondOnDate() + 86400 * 15), "A Night in Vegas Casino", "How are you? We miss you! Become a billionaire with us!!", new Color(1, 0.3f, 0.15f));
        //NotificationManager.Send(TimeSpan.FromSeconds(SecondOnDate() + 86400 * 30), "A Night in Vegas Casino", "How are you? We miss you! Become a billionaire with us!!", new Color(1, 0.3f, 0.15f));
    }

    int SecondOnDate()
    {
        int second = 0;
        DateTime current = DateTime.Now;
        second = 86400 - (current.Hour * 3600 + current.Minute * 60 + current.Second);
        return second;
    }

    public Text reward;
    public Button collect;
    public GameObject fxGain;
    public void GainExp(double e)
    {
        if (fxGain && e > 0)
        {
            fxGain.SetActive(true);
            Invoke("OffGain", 0.8f);
        }
        vCurrExp += e;
        allEXP += e;
        PlayerPrefs.SetString("playerEXP", ((long)allEXP).ToString());
        if (vCurrExp >= vExpLeft)
        {
            vCurrExp -= vExpLeft;
            vLevel++;
            PlayerPrefs.SetInt("playerLevel", vLevel);
            PlayerPrefs.Save();
            vExpLeft = ExperiencePoints(vLevel);
            if (fxLVUP && vLevel >= 5)
            {
                reward.text = (10000 * vLevel).ToString("#,##0");
                levelValue.text = vLevel.ToString();
                fxLVUP.GetComponent<Animator>().SetTrigger("Open");
                fxLVUP.transform.GetChild(0).GetComponent<AudioSource>().Play();
                collect.onClick.RemoveAllListeners();
                collect.onClick.AddListener(() => {
                    fxLVUP.GetComponent<Animator>().SetTrigger("Close");
                    SlotMachine.Instance.PlusMoney(10000 * vLevel);
                });           
            }

            if (PlayerPrefs.GetInt("lastRate", 0) != -1)
            {
                if (fxRate && vLevel % 10 == 0)
                {
                    butRate.onClick.RemoveAllListeners();
                    butRate.onClick.AddListener(() =>
                    {
                        PlayerPrefs.SetInt("lastRate", -1);
                        Application.OpenURL("http://play.google.com/store/apps/details?id=" + Application.identifier);
                        fxRate.GetComponent<Animator>().SetTrigger("Close");
                    });
                    if (PlayerPrefs.GetInt("lastRate", 0) != vLevel)
                    {
                        PlayerPrefs.SetInt("lastRate", vLevel);
                        fxRate.GetComponent<Animator>().SetTrigger("Open");
                    }
                }
            }

            if (fxUnlockLV)
                for (int i = 0; i < AssetBundleController.Instance.bundleDatas.Length; i++)
                    if (AssetBundleController.Instance.bundleDatas[i].LevelRequire == vLevel)
                    {
                        var item = AssetBundleController.Instance.bundleDatas[i];
                        int x = i;
                        AssetBundleController.Instance.LoadBundle(item.Name, (process) =>
                        {
                            if (process > 1f)
                            {
                                fxUnlockLV.GetComponent<Animator>().SetTrigger("Open");
                                fxUnlockLV.transform.GetChild(0).GetComponent<Image>().sprite = item.Sprite;
                                Button b = fxUnlockLV.transform.GetChild(0).GetChild(0).GetComponent<Button>();
                                b.onClick.RemoveAllListeners();
                                b.onClick.AddListener(() =>
                                {
                                    AssetBundleController.Instance.selectedAsset = x;
                                    Application.LoadLevel("Game");
                                });
                            }
                        }, false);
                        break;
                    }

        }
        PlayerPrefs.SetString("playerCurEXP", vCurrExp.ToString());
    }

    void OffGain()
    {
        fxGain.SetActive(false);
    }
}
