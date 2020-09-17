using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ItemGame : MonoBehaviour {
    public Image loadState, gameAvatar;
    public Button play;
    public GameObject unlock;
    string nameAsset = "";
    public Image image;

    bool unlocked = false;
    public void UpdateData(int index)
    {
        image.gameObject.SetActive(false);
        var temp = AssetBundleController.Instance.bundleDatas[index];
        unlocked = MenuManager.Instance.currentLevel >= temp.LevelRequire;
        int requireFromLVL = temp.LevelRequire;
        string nameAsset = temp.Name;

        gameAvatar.sprite = temp.Sprite;
        loadState.sprite = temp.Sprite;
        this.nameAsset = nameAsset;
        play.gameObject.SetActive(unlocked);
        unlock.gameObject.SetActive(!unlocked);
        unlock.transform.GetChild(0).GetComponent<Text>().text = "NEED " + requireFromLVL + " LVL";
        loadState.fillAmount = 1f;

        play.onClick.AddListener(() => {
            AssetBundleController.Instance.selectedAsset = index;
            Application.LoadLevel("Game");
            AdController.Instance.ShowInterstitial();
        });

        if (unlocked)
            play.interactable = false; 
        LoadBundle();
    }

    void LoadBundle()
    {
        AssetBundleController.Instance.LoadBundle(nameAsset, (process) =>
        {
            //if(unlock) unlock.transform.GetChild(0).GetComponent<Text>().text = "LOADING";
            if(loadState) loadState.fillAmount = 1 - process;
            //if (process == 1f)
            //    if (unlock) unlock.transform.GetChild(0).GetComponent<Text>().text = "COMPLETED";
            if (process == 1.1f)
            {
                if (play && unlocked)
                {
                    play.interactable = unlocked;
                    play.gameObject.SetActive(unlocked);
                }
                if (unlock)
                {
                    unlock.gameObject.SetActive(!unlocked);
                    //unlock.transform.GetChild(0).GetComponent<Text>().text = "COMPLETED";
                }

                if (image)
                {
                    image.GetComponent<GifImage>().lstSprite = AssetBundleController.Instance.GetSprites(nameAsset, "wild").ToList();
                    if (image.GetComponent<GifImage>().lstSprite[0].texture.height > 500)
                        image.rectTransform.sizeDelta = new Vector2(150f, 450f);
                    else
                        image.rectTransform.sizeDelta = new Vector2(150f, 150f);
                    image.gameObject.SetActive(true);
                }
            }
        });
    }
}
