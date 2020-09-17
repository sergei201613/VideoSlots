using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AssetBundleController : MonoBehaviour {
    public static AssetBundleController Instance;
    public string Url = "";
    public string numberedServer;
    [HideInInspector]
    public int selectedAsset = 0;
    public AssetBundle bundle {
        get { return bundleDatas[selectedAsset].assetBundle; }
    }

    private void Awake()
    {
        if (Instance)
        {
            //Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        if (MenuManager.Instance && MenuManager.Instance.gameObjectLoad) MenuManager.Instance.gameObjectLoad.SetActive(true);
        if (Instance.gameObject != this.gameObject)
        {
            if (MenuManager.Instance && MenuManager.Instance.gameObjectLoad) MenuManager.Instance.gameObjectLoad.SetActive(false);
            //if (Application.loadedLevelName == "Menu") MenuManager.Instance.LoadGameList();
            Destroy(this.gameObject);
        }
        else
            Instance.StartCoroutine(CheckUpdate());
    }

    public Sprite[] GetSprites(string ass, string gifPath, bool isUI = false)
    {
        AssetBundle asset = null;
        if (isUI) asset = AssetBundleController.Instance.bundleUI;
        else
        {
            foreach (var item in bundleDatas)
                if (item.Name == ass)
                    asset = item.assetBundle;
        }
        if (!asset) return null;
        List<Sprite> sp = new List<Sprite>();
        string[] ss = asset.GetAllAssetNames();
        foreach (var s in ss)
        {
            if (s.Contains(gifPath) && !s.Contains("avatar"))
            {
                var sss = asset.LoadAsset<Sprite>(s);
                if (sss)
                    sp.Add(sss);
            }
        }
        return sp.ToArray();
    }


    IEnumerator CheckUpdate()
    {
        WWW www = new WWW(Url + "version.txt");
        while (!www.isDone)
            yield return new WaitForSeconds(0.1f);
        int ver = 0;
        int.TryParse(www.text, out ver);
        if (ver > PlayerPrefs.GetInt("ver", 0))
        {
            Caching.ClearCache(); 
            PlayerPrefs.SetInt("ver", ver);
        }

        www = new WWW(Url + "data" + numberedServer + ".txt");
        while (!www.isDone)
            yield return new WaitForSeconds(0.1f);
        if (string.IsNullOrEmpty(www.error))
        {
            bundleDatas = JsonHelper.FromJson<BundleData>(www.text);
            foreach (var item in bundleDatas)
            {
                item.LevelRequire = 0;
                if (File.Exists(Application.persistentDataPath + "/" + item.Name + ".png"))
                {
                    byte[] bytes = File.ReadAllBytes(Application.persistentDataPath + "/" + item.Name + ".png");
                    Texture2D text = new Texture2D(4, 4);
                    text.LoadImage(bytes);
                    Rect rec = new Rect(0, 0, text.width, text.height);
                    item.Sprite = Sprite.Create(text, rec, new Vector2(0.5f, 0.5f), 0.01f);
                }
                else
                {
                    WWW avatar = new WWW(Url + "avatars" + numberedServer + "/" + item.Name + ".png");
                    while (!avatar.isDone)
                    {
                        if (MenuManager.Instance && MenuManager.Instance.gameObjectLoad)
                            MenuManager.Instance.gameObjectLoad.transform.GetChild(1).GetComponent<Text>().text = "LOADING GAME AVATAR " + item.Name + " " + (int)(avatar.progress * 100) + "%";
                        yield return new WaitForSeconds(0.1f);
                    }

                    if (string.IsNullOrEmpty(avatar.error))
                    {
                        Rect rec = new Rect(0, 0, avatar.texture.width, avatar.texture.height);
                        item.Sprite = Sprite.Create(avatar.texture, rec, new Vector2(0.5f, 0.5f), 0.01f);
                        byte[] bytes = avatar.texture.EncodeToPNG();
                        File.WriteAllBytes(Application.persistentDataPath + "/" + item.Name + ".png", bytes);
                    }
                }
            }

            if (!bundleUI)
            {
                StartCoroutine(IELoadBundle("ui", (process) =>
                {
                    if (MenuManager.Instance && MenuManager.Instance.gameObjectLoad)
                    {
                        if (Time.time > 13)
                            MenuManager.Instance.gameObjectLoad.SetActive(process <= 1);
                        if (process > 1f)
                            MenuManager.Instance.LoadGameList();
                        if (process > 1f) process = 1f;
                        MenuManager.Instance.gameObjectLoad.transform.GetChild(1).GetComponent<Text>().text = "INTERFACE LOADING " + (int)(process * 100) + "%";
                    }
                }, true));
            }
        }
        else
            if (MenuManager.Instance && MenuManager.Instance.gameObjectLoad)
                MenuManager.Instance.gameObjectLoad.transform.GetChild(1).GetComponent<Text>().text = "ERROR LOADING DATA";
        yield break;
    }

    public AssetBundle bundleUI;

    public BundleData[] bundleDatas;
    public void LoadBundle(string idBundle, System.Action<float> callProcess = null, bool isUI = false)
    {
        if(idBundle == "" && callProcess != null)
        {
            callProcess(0f);
            return;
        }
        int i = -1;
        foreach (var item in bundleDatas)
        {
            i++;
            if (item.Name == idBundle)
            {
                if (item.assetBundle == null)
                    StartCoroutine(IELoadBundle(i.ToString(), callProcess, isUI));
                else
                    if (callProcess != null) callProcess(1.1f);
                break;
            }
        }
    }

    IEnumerator IELoadBundle(string idBundle, System.Action<float> callProcess = null, bool isUI = false)
    {
        while (!Caching.ready)
            yield return null;
        string platform = "";
#if UNITY_ANDROID
        platform = "Android/";
#elif UNITY_IOS
        platform = "IOS/";
#endif
        WWW www;
        int id = -1;
        string url = "";
        if (int.TryParse(idBundle, out id))
        {
            bundleDatas[id].action = callProcess;
            if (bundleDatas[id].isLoading) yield break;
            url = Url + platform + bundleDatas[id].Name;
            bundleDatas[id].isLoading = true;
        }
        else
            url = Url + platform + idBundle;
        www = WWW.LoadFromCacheOrDownload(url, 0);

        while (!www.isDone)
        {
            if (id != -1 && bundleDatas[id].action != null)
                bundleDatas[id].action(www.progress);
            else
                if (callProcess != null) callProcess(www.progress);
            yield return new WaitForSeconds(0.5f);
        }

        yield return www;
        if (!string.IsNullOrEmpty(www.error))
            yield break;
        if (!isUI && id != -1)
        {
            bundleDatas[id].assetBundle = www.assetBundle;
            if (bundleDatas[id].action != null)
                bundleDatas[id].action(1.1f);
        }
        else
        {
            bundleUI = www.assetBundle;
            if (callProcess != null) callProcess(1.1f);
        }       
    }
}

[System.Serializable]
public class BundleData
{
    public string Name;
    public Sprite Sprite;
    public int LevelRequire = 0;
    public AssetBundle assetBundle;
    public bool isLoading = false;
    public System.Action<float> action;
}
