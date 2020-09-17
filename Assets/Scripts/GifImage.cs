using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GifImage : MonoBehaviour
{
    public float timeWait = 0.05f;
    public string gifPath = "GIFs/";
    Image sp;
    SpriteRenderer spr;
    public List<Sprite> lstSprite;
    public bool loop = true;
    public float delayLoop = 0;
    public int from;

    public bool blurHide = false;
    public bool loadResource = true;
    public bool fromUI = false;

    private void OnEnable()
    {
        if (blurHide)
            ChangeAnpha(1f);
        Gif();
    }

    void ChangeAnpha(float a)
    {
        Color c = transform.GetComponent<Image>().color;
        transform.GetComponent<Image>().color = new Color(c.r, c.g, c.b, a);
    }

    public void Gif()
    {
        sp = GetComponent<Image>();
        spr = GetComponent<SpriteRenderer>();

        if (gifPath != "")
        {
            lstSprite = new List<Sprite>();
            Sprite[] sps = new Sprite[0];
            if (loadResource)
                sps = Resources.LoadAll<Sprite>(gifPath);
            else if(PayTable.Instance)
                sps = PayTable.Instance.GetSprites(gifPath, fromUI);

            for (int i = 0; i < sps.Length; i++)
                if (sps[i])
                    lstSprite.Add(sps[i]);    
        }

        if (lstSprite.Count > 0)
        {
            StopCoroutine(ChangeImage());
            StartCoroutine(ChangeImage());
        }
    }

    float lastLoop = 0f;
    IEnumerator ChangeImage()
    {
        int index = 0;
        while (true)
        {
            if (lstSprite[index])
            {
                if (sp) sp.sprite = lstSprite[index];
                if (spr) spr.sprite = lstSprite[index];
                yield return new WaitForSeconds(timeWait);
            }

            if (delayLoop > 0)
            {
                if (Time.time - lastLoop > delayLoop)
                    index++;
            }
            else
                index++;
            if (!blurHide && index >= lstSprite.Count)
            {
                if (loop)
                {        
                    index = 0;
                    if (from != 0) index = from;
                    lastLoop = Time.time;
                }
                else
                    yield break;
            }
            else if (blurHide && index >= lstSprite.Count - 10)
            {
                if (index >= lstSprite.Count)
                {
                    gameObject.SetActive(false);
                    yield break;
                }
                ChangeAnpha((lstSprite.Count - index) / 10f);
            }
        }
    }
}
