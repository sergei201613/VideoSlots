using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ItemRank : MonoBehaviour {
    public Text Name, EXP, Rank, Level, Credit;
    public Image Avatar;

    [Header("ONLY FOR TOP 1 USER")]
    public GameObject frameFire;

    [Header("ONLY FOR THIS USER RANK")]
    public GameObject updateObj;
    public InputField textUpdate;

    public void UpdateData (UserRank rank)
    {
        if (rank.EXP < 0) rank.EXP = -rank.EXP;
        if (rank.Credit < 0) rank.Credit = -rank.Credit;
        Name.text = rank.Name;
        //EXP.text = "<color=red>LEVEL:  </color>" + rank.Level + "<color=red>     EXP:  </color>" + rank.EXP + "<color=red>     CREDIT:  </color>" + rank.Credit;
        Level.text = rank.Level.ToString();
        EXP.text = rank.EXP.ToString("#,##0");
        Credit.text = rank.Credit.ToString("#,##0");
        Rank.text = rank.Rank;
        int a = -1;
        if (int.TryParse(rank.Avatar, out a))
        {
            if (a > 0)
                Avatar.sprite = RankingController.Instance.avatar[a];
        }
        else
            StartCoroutine(GetAvatar(rank.Avatar));
        if (frameFire) frameFire.SetActive(rank.Rank == "1");
        if (textUpdate) textUpdate.text = rank.Name;
        if(updateObj) updateObj.SetActive(false);
    }

    public void SaveUpdate()
    {
        Name.text = textUpdate.text;
        PlayerPrefs.SetString("playerName", textUpdate.text);
        PlayerPrefs.Save();
        updateObj.SetActive(false);
        RankingController.Instance.GetData("getrank");
    }

    IEnumerator GetAvatar(string avatar)
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
                Avatar.sprite = Sprite.Create(www.texture, rec, new Vector2(0.5f, 0.5f), 100f);
            }
        }
        yield break;
    }
}
