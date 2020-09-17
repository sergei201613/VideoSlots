using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_IOS || UNITY_ANDROID
//using ImageAndVideoPicker;
#endif

#if UNITY_WSA
using Newtonsoft.Json;
#endif

public class RankingController : MonoBehaviour
{
    public static RankingController Instance;
    public GameObject itemRank, userRank, itemAvatar;
    public Sprite[] avatar;

    private void Awake()
    {
        Instance = this;
        itemRank.SetActive(false);
        userRank.SetActive(false);
        itemAvatar.SetActive(false);
        int i = 0;
        foreach(var item in avatar)
        {
            GameObject temp = Instantiate(itemAvatar, itemAvatar.transform.parent, false);
            temp.GetComponent<Image>().sprite = item;
            temp.GetComponent<Button>().onClick.RemoveAllListeners();
            temp.GetComponent<Button>().onClick.AddListener(() => { ChangeAvatar(temp.transform.GetSiblingIndex() - 1); });
            i++;
            temp.SetActive(true);
        }

        if (PlayerPrefs.GetString("playerAvatar", "-1") == "-1")
            ChangeAvatar();
    }

    #if UNITY_IOS || UNITY_ANDROID
    //void OnEnable()
    //{
    //    PickerEventListener.onImageSelect += OnImageSelect;
    //    PickerEventListener.onImageLoad += OnImageLoad;
    //}

    //void OnDisable()
    //{
    //    PickerEventListener.onImageSelect -= OnImageSelect;
    //    PickerEventListener.onImageLoad -= OnImageLoad;
    //}

    //void OnImageSelect(string imgPath, ImageOrientation imgOrientation)
    //{
    //    Debug.Log("Image Location : " + imgPath);
    //    StartCoroutine(LoadImage(imgPath));
    //}

    //void OnImageLoad(string imgPath, Texture2D tex, ImageOrientation imgOrientation)
    //{
    //    Debug.Log("Image Location : " + imgPath);
    //    Rect rec = new Rect(0, 0, tex.width, tex.height);
    //    userRank.GetComponent<ItemRank>().Avatar.sprite = Sprite.Create(tex, rec, new Vector2(0.5f, 0.5f), 100f);
    //}
#endif
    IEnumerator LoadImage(string path)
    {
        var url = "file://" + path;
        var www = new WWW(url);
        while (!www.isDone)
            yield return new WaitForSeconds(0.1f);
        yield return www;
        var texture = www.texture;
        if (texture != null)
        {
            var t = Resize(texture);
            StartCoroutine(UploadPNG(t));
        }
    }

    public void SelectAvatar()
    {
#if UNITY_ANDROID
                //AndroidPicker.BrowseImage(true);
#elif UNITY_IPHONE
			    //IOSPicker.BrowseImage(true); // true for pick and crop
#endif
    }


    public void ChangeAvatar(int index = -1)
    {
        if (index == -1)
            index = Random.Range(0, avatar.Length);

        PlayerPrefs.SetString("playerAvatar", index.ToString());
        PlayerPrefs.Save();

        userRank.GetComponent<ItemRank>().Avatar.sprite = avatar[index];
        GetData("getrank");
    }

    public void GetData(string type)
    {
        StartCoroutine(PostData(type));
    }

    UserRank[] lstRank;
    IEnumerator PostData(string type)
    {
        string id = SystemInfo.deviceUniqueIdentifier;
        string url = "https://appstore.com.ru/SLOTS/PHP/Control.php";
        WWWForm form = new WWWForm();
        form.AddField("type", type);
        form.AddField("game", Application.identifier);
        form.AddField("name", PlayerPrefs.GetString("playerName", "PLAYER"));
        form.AddField("device", id);
        double exp = double.Parse(PlayerPrefs.GetString("playerEXP", "0"));
        form.AddField("exp", ((long)exp).ToString());
        form.AddField("level", PlayerPrefs.GetInt("playerLevel", 1).ToString());
        form.AddField("credit", PlayerPrefs.GetString("mm", "0"));
        form.AddField("avatar", PlayerPrefs.GetString("playerAvatar", "-1"));
        WWW www = new WWW(url, form);

        while (!www.isDone)
            yield return new WaitForSeconds(0.1f);
        print("rank "+www.text);
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
                    foreach (Transform tr in itemRank.transform.parent)
                        if (tr != itemRank.transform && tr.gameObject) Destroy(tr.gameObject);
                    for (int i = 0; i < lstRank.Length; i++)
                    {
                        int r = 0;
                        int.TryParse(lstRank[i].Rank, out r);
                        if (r <= lstRank.Length)
                        {
                            GameObject temp = Instantiate(itemRank, itemRank.transform.parent, false);
                            temp.SetActive(true);
                            temp.GetComponent<ItemRank>().UpdateData(lstRank[i]);
                        }
                        if (lstRank[i].User == id)
                        {
                            userRank.SetActive(true);
                            userRank.GetComponent<ItemRank>().UpdateData(lstRank[i]);                     
                        }
                    }
                    break;
                }
                yield break;
        }
    }

    IEnumerator UploadPNG(Texture2D tex)
    {
        string id = SystemInfo.deviceUniqueIdentifier;
        string url = "https://appstore.com.ru/SLOTS/PHP/uploadPNG.php";
        byte[] bytes = tex.EncodeToJPG();
        WWWForm form = new WWWForm();
        form.AddField("game", Application.identifier);
        form.AddField("device", id);
        form.AddBinaryData("fileUpload", bytes, id + ".jpg");
        
        WWW www = new WWW(url, form);
        while (!www.isDone)
            yield return new WaitForSeconds(0.1f);

        if (www.text == "Done")
        {
            PlayerPrefs.SetString("playerAvatar", SystemInfo.deviceUniqueIdentifier);
            PlayerPrefs.Save();
            GetData("getrank");
        }
    }

    static Texture2D Resize(Texture2D source)
    {
        int newWidth = 128;
        int newHeight = 128;
        if (source.width > source.height)
            newWidth = (int)newHeight * source.width / source.height;
        else
            newHeight = (int)newWidth * source.height / source.width;

        source.filterMode = FilterMode.Bilinear;
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Bilinear;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        var nTex = new Texture2D(newWidth, newHeight);
        nTex.ReadPixels(new Rect(0, 0, newWidth, newWidth), 0, 0);
        nTex.Apply();
        RenderTexture.active = null;
        return nTex;
    }
}

[System.Serializable]
public class UserRank
{
    public string User;
    public string Name;
    public double EXP;
    public int Level;
    public string Rank;
    public double Credit;
    public string Avatar;
}
