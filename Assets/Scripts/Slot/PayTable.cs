using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PayTable : MonoBehaviour {
    [Header("PAY SETTING")]
    public GameObject itemPay;
    public List<SettingPay> settingPays;
    //[HideInInspector]
    public Sprite[] shortWild, shortBonus, shortOther, shortScatter;
    [HideInInspector]
    public string curPath = "";
    public void InitPay()
    {
        shortWild = new Sprite[0];
        shortBonus = new Sprite[0];
        shortOther = new Sprite[0];
        shortScatter = new Sprite[0];
        settingPays = new List<SettingPay>();
        var asset = AssetBundleController.Instance.bundle;
        string[] ss = asset.GetAllAssetNames();
        string path = "";
        string[] s = ss[0].Split('/');
        for (int i = 0; i < s.Length - 2; i++)
            path += s[i] + "/";
        curPath = path;
        TextAsset textdata = asset.LoadAsset<TextAsset>(path + "DATA.txt");
        string[] pathdata = textdata.text.Split('\n');
        foreach(var p in pathdata)
        {
            if (
                p.Contains("a1") || p.Contains("a2") || p.Contains("a3") || p.Contains("a4") || p.Contains("a5")
                || p.Contains("a6") || p.Contains("a7") || p.Contains("a8") || p.Contains("a9") || p.Contains("a10")
                || p.Contains("a11") || p.Contains("a12") || p.Contains("a13") || p.Contains("a14") || p.Contains("a15")
                )
            {
                settingPays.Add(CPay(path, p.Trim()));
            }else if(p.Contains("WILD") || p.Contains("OTHER") || p.Contains("BONUS") || p.Contains("SCATTER"))
            {
                if (p.Contains("WILD") && shortWild.Length == 0)
                {
                    int c = 1;
                    for (int i = pathdata.Length - 1; i >= 0; i--)
                    {
                        if (pathdata[i].Contains("WILD-5")) c = 5;
                        else if (pathdata[i].Contains("WILD-4")) c = 4;
                        else if (pathdata[i].Contains("WILD-3")) c = 3;
                        else if (pathdata[i].Contains("WILD-2")) c = 2;
                        if (c > 1) break;
                    }
                    if (c > 1)
                    {
                        shortWild = new Sprite[c];
                        for (int i = 0; i < shortWild.Length; i++)
                            shortWild[i] = GetAvatar("wild-" + (i + 1));
                        settingPays.Add(CPay(path, "wild"));
                    }
                    else
                        settingPays.Add(CPay(path, p.Trim()));
                }

                if (p.Contains("OTHER") && shortOther.Length == 0)
                {
                    int c = 1;
                    for (int i = pathdata.Length - 1; i >= 0; i--)
                    {
                        if (pathdata[i].Contains("OTHER-5")) c = 5;
                        else if (pathdata[i].Contains("OTHER-4")) c = 4;
                        else if (pathdata[i].Contains("OTHER-3")) c = 3;
                        else if (pathdata[i].Contains("OTHER-2")) c = 2;
                        if (c > 1) break;
                    }
                    if (c > 1)
                    {
                        shortOther = new Sprite[c];
                        for (int i = 0; i < shortOther.Length; i++)
                            shortOther[i] = GetAvatar("other-" + (i + 1));
                        settingPays.Add(CPay(path, "other"));
                    }
                    else
                        settingPays.Add(CPay(path, p.Trim()));

                }

                if (p.Contains("BONUS") && shortBonus.Length == 0)
                {
                    int c = 1;
                    for (int i = pathdata.Length - 1; i >= 0; i--)
                    {
                        if (pathdata[i].Contains("BONUS-5")) c = 5;
                        else if (pathdata[i].Contains("BONUS-4")) c = 4;
                        else if (pathdata[i].Contains("BONUS-3")) c = 3;
                        else if (pathdata[i].Contains("BONUS-2")) c = 2;
                        if (c > 1) break;
                    }

                    if (c > 1)
                    {
                        shortBonus = new Sprite[c];
                        for (int i = 0; i < shortBonus.Length; i++)
                            shortBonus[i] = GetAvatar("bonus-" + (i + 1));
                        settingPays.Add(CPay(path, "bonus"));
                    }
                    else
                        settingPays.Add(CPay(path, p.Trim()));
                }

                if (p.Contains("SCATTER") && shortScatter.Length == 0)
                {
                    int c = 1;
                    for (int i = pathdata.Length - 1; i >= 0; i--)
                    {
                        if (pathdata[i].Contains("SCATTER-5")) c = 5;
                        else if (pathdata[i].Contains("SCATTER-4")) c = 4;
                        else if (pathdata[i].Contains("SCATTER-3")) c = 3;
                        else if (pathdata[i].Contains("SCATTER-2")) c = 2;
                        if (c > 1) break;
                    }

                    if (c > 1)
                    {
                        shortScatter = new Sprite[c];
                        for (int i = 0; i < shortScatter.Length; i++)
                            shortScatter[i] = GetAvatar("scatter-" + (i + 1));
                        settingPays.Add(CPay(path, "scatter"));
                    }
                    else
                        settingPays.Add(CPay(path, p.Trim()));
                }
            }
        }
    }

    public AudioClip[] betClips;
    public Image imageOther;
    SettingPay CPay(string path, string p)
    {
        SettingPay st = new SettingPay();
        st.gifPath = (path + p).ToLower();
        st.Name = p.ToLower().Trim();
        switch (p.Trim().ToLower())
        {
            case "wild":
                st.tylePay = new int[5];
                for (int i = 0; i < 5; i++)
                    st.tylePay[i] = PayData.table[0, i];
                st.tyleStyle = TyleStyle.Wild;
                break;

            case "bonus":
                st.tylePay = new int[5];
                st.tyleStyle = TyleStyle.Bonus;
                break;

            case "scatter":
                st.tylePay = new int[5];
                st.tyleStyle = TyleStyle.Scatter;
                break;

            case "other":
                st.tylePay = new int[5];
                st.tyleStyle = TyleStyle.Other;
                imageOther.GetComponent<GifImage>().lstSprite = GetSprites(st.gifPath).ToList();
                imageOther.GetComponent<GifImage>().Gif();
                break;

            default:
                string s = p.Trim().Replace("a", "");
                int c = 0;
                if (int.TryParse(s, out c))
                {
                    st.tylePay = new int[5];
                    for (int i = 0; i < 5; i++)
                        st.tylePay[i] = PayData.table[c, i];
                    st.tyleStyle = TyleStyle.Normal;
                }
                break;
        }
        var asset = AssetBundleController.Instance.bundle;
        string[] ss = asset.GetAllAssetNames();
        foreach (var s in ss)
        {
            if (!st.isLong && s.Contains("avatar/" + p.ToLower()))
            {
                st.isLong = true;
                //break;
            }
            if(s.Contains("sounds/" + p.ToLower()))
            {
                AudioClip audioClip = asset.LoadAsset<AudioClip>(s);
                if (audioClip) st.soundCash = audioClip;
            }
        }
        return st;
    }

    public void CreatePay()
    {
        foreach (Transform tran in itemPay.transform.parent)
            if (tran.gameObject != itemPay) Destroy(tran.gameObject);
        for (int i = 0; i < settingPays.Count; i++)
            //if (settingPays[i].tyleStyle == TyleStyle.Normal)
            {
                GameObject temp = Instantiate(itemPay, itemPay.transform.parent, false);
                temp.GetComponent<ItemPay>().UpdateData(i);
                temp.SetActive(true);
            }
        itemPay.SetActive(false);
    }

    public Sprite GetAvatar(string type)
    {
        var asset = AssetBundleController.Instance.bundle;
        string[] ss = asset.GetAllAssetNames();
        Sprite sp1 = null, sp2 = null;
        foreach (var s in ss)
        {
            if (s.Contains("avatar/" + type))
            {
                sp1 = asset.LoadAsset<Sprite>(s);
                break;
            }

            if (s.Contains(type) && sp2 == null)
                sp2 = asset.LoadAsset<Sprite>(s);
        }
        if (sp1 != null)
            return sp1;
        else
            return sp2;
    }

    public Sprite GetAvatar(int type)
    {
        var asset = AssetBundleController.Instance.bundle;
        string[] ss = asset.GetAllAssetNames();
        Sprite sp1 = null, sp2 = null;
        foreach (var s in ss)
        {
            if (s.Contains("avatar/" + settingPays[type].Name))
            {
                sp1 = asset.LoadAsset<Sprite>(s);
                break;
            }

            if (s.Contains(settingPays[type].gifPath) && sp2 == null)
                sp2 = asset.LoadAsset<Sprite>(s);
        }
        if (sp1 != null)
            return sp1;
        else
            return sp2;
    }

    public Sprite[] GetSprites(string gifPath, bool isUI = false)
    {
        var asset = AssetBundleController.Instance.bundle;
        if (isUI) asset = AssetBundleController.Instance.bundleUI;

        List<Sprite> sp = new List<Sprite>();
        string[] ss = asset.GetAllAssetNames();
        foreach (var s in ss)
        {
            if (s.Contains(gifPath))
                sp.Add(asset.LoadAsset<Sprite>(s));
        }
        return sp.ToArray();
    }

    [Header("RULE SETTING")]
    public GameObject itemRule;
    public List<SettingRule> settingRules;
    public void CreateRule()
    {
        foreach (Transform tran in itemRule.transform.parent)
            if (tran.gameObject != itemRule) Destroy(tran.gameObject);
        for (int i = 0; i < settingRules.Count; i++)
        {
            GameObject temp = Instantiate(itemRule, itemRule.transform.parent, false);
            temp.GetComponent<ItemRule>().UpdateData(i);
            temp.SetActive(true);
        }
        itemRule.SetActive(false);
    }

    [Header("BONUS SETTING")]
    public GameObject itemBonus;
    public List<SettingBonus> settingBonus;
    public void CreateBonus()
    {
        foreach (Transform tran in itemBonus.transform.parent)
            if (tran.gameObject != itemBonus) Destroy(tran.gameObject);
        for (int i = 0; i < settingBonus.Count; i++)
        {
            GameObject temp = Instantiate(itemBonus, itemBonus.transform.parent, false);
            temp.GetComponent<ItemBonus>().UpdateData(i);
            temp.SetActive(true);
        }
        itemBonus.SetActive(false);
    }

    [Header("LINE SETTING")]
    public GameObject itemLine;
    public void CreateLine()
    {
        foreach (Transform tran in itemLine.transform.parent)
            if (tran.gameObject != itemLine) Destroy(tran.gameObject);
        for(int i = 0; i < PayData.line.GetLength(0); i++)
        {
            GameObject temp = Instantiate(itemLine, itemLine.transform.parent, false);
            temp.GetComponent<ItemLine>().UpdateData(i);
            temp.SetActive(true);
        }
        itemLine.SetActive(false);
    }

    public static PayTable Instance;
    private void Awake()
    {
        Instance = this;
        InitPay();
        CreateBonus();
        CreateLine();
        CreatePay();
        CreateRule();    
    }

    private void Start()
    {
        SlotMachine.Instance.LoadSymbol();
    }
}

[System.Serializable]
public class SettingRule
{
    public Sprite sprite;
    [Multiline]
    public string description;
}

[System.Serializable]
public class SettingPay
{
    public string Name;
    public TyleStyle tyleStyle;
    public bool isLong = false;
    public string gifPath;
    public GameObject fxSquare;
    public AudioClip soundCash;
    public int[] tylePay;
}

[System.Serializable]
public class SettingBonus
{
    //public Sprite sprite;
    public TyleStyle tyleStyle;
    [Multiline]
    public string description;
    public GameObject bonusObj;
}

public enum TyleStyle
{
    Normal,
    Scatter,
    Wild,
    Bonus,
    Other,
}