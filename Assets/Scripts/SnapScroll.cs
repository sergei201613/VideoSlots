using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SnapScroll : MonoBehaviour
{
    //[HideInInspector]
    public StyleList styleList;
    public int spacing = 50;
    public RectTransform centerFrame;
    public GameObject itemSpawn;

    private List<GameObject> lstObj;
    private ScrollRect RectPanel;
    private RectTransform ScrollPanel;
    private float[] distance;
    private bool isDragging = false;
    private int bttnDistance;
    private int minButtonNum;
    private bool isRunning = false;
    private bool isEnable = false;
    public int Selected = 0;

    bool enable = false;
    public void SpawnData(int countPawn)
    {
        enable = false;
        if (lstObj != null && lstObj.Count > 1)
            for (int i = 1; i < lstObj.Count; i++)
                if (lstObj[i].gameObject) Destroy(lstObj[i].gameObject);

        RectPanel = GetComponent<ScrollRect>();
        switch (styleList)
        {
            case StyleList.Horizontal:  //List Ngang
                RectPanel.vertical = false;
                RectPanel.horizontal = true;
                break;

            case StyleList.Vertical:    //List doc
                RectPanel.vertical = true;
                RectPanel.horizontal = false;
                break;
        }

        ScrollPanel = RectPanel.content;
        lstObj = new List<GameObject>();
        Vector3 sPos = itemSpawn.transform.localPosition;
        Vector2 sSize = itemSpawn.GetComponent<RectTransform>().sizeDelta;

        //bool sp = true;

        //if (sp)
        //{
        //    lstObj.Add(itemSpawn);
        //    itemSpawn.SetActive(true);
        //    itemSpawn.SendMessage("UpdateData", 0, SendMessageOptions.DontRequireReceiver);

        //    for (int i = 1; i < countPawn; i++)
        //    {
        //        GameObject temp = Instantiate(itemSpawn, itemSpawn.transform.parent, true);
        //        switch (styleList)
        //        {
        //            case StyleList.Horizontal:  //List Ngang
        //                sPos.x += sSize.x + spacing;
        //                break;

        //            case StyleList.Vertical:    //List doc
        //                sPos.y -= sSize.y + spacing;
        //                break;
        //        }
        //        temp.transform.localPosition = new Vector2(sPos.x, sPos.y);
        //        temp.SetActive(true);
        //        temp.SendMessage("UpdateData", i, SendMessageOptions.DontRequireReceiver);
        //        lstObj.Add(temp);
        //    }
        //}
        //else
        //    itemSpawn.SetActive(false);

        isEnable = false;
        int bttnLenght = lstObj.Count;
        distance = new float[bttnLenght];
        switch (styleList)
        {
            case StyleList.Horizontal:  //List Ngang
                bttnDistance = (int)sSize.x + spacing;
                break;

            case StyleList.Vertical:    //List doc
                bttnDistance = (int)sSize.y + spacing;
                break;
        }
        enable = true;
    }

    float minDistance;
    void Update()
    {
        if (!enable) return;
        switch (styleList)
        {
            case StyleList.Horizontal:  //List Ngang
                for (int i = 0; i < lstObj.Count; i++)
                    if (lstObj[i] == null) return;
                    else
                        distance[i] = Mathf.Abs(centerFrame.transform.position.x - lstObj[i].transform.position.x);
                break;

            case StyleList.Vertical:    //List doc
                for (int i = 0; i < lstObj.Count; i++)
                    if (lstObj[i] == null) return;
                    else
                        distance[i] = Mathf.Abs(centerFrame.transform.position.y - lstObj[i].transform.position.y);
                break;
        }

        minDistance = Mathf.Min(distance);
        for (int k = 0; k < lstObj.Count; k++)
            if (minDistance == distance[k])
            {
                minButtonNum = k;
                Selected = k;
                lstObj[Selected].SendMessage("Selected", 0, SendMessageOptions.DontRequireReceiver);
                ScaleUpAndScaleDown(k);
            }

        if (!Input.GetMouseButton(0))
            switch (styleList)
            {
                case StyleList.Horizontal:  //List Ngang
                    LerpToTargetPosition(minButtonNum * -bttnDistance);
                    break;

                case StyleList.Vertical:    //List doc
                    LerpToTargetPosition(minButtonNum * +bttnDistance);
                    break;
            }

    }

    void LerpToTargetPosition(int pos)
    {
        float newP; Vector2 newPosition = new Vector2();
        switch (styleList)
        {
            case StyleList.Horizontal:  //List Ngang
                newP = Mathf.Lerp(ScrollPanel.anchoredPosition.x, pos, Time.deltaTime * 5f);
                newPosition = new Vector2(newP, ScrollPanel.anchoredPosition.y);
                break;

            case StyleList.Vertical:    //List doc
                newP = Mathf.Lerp(ScrollPanel.anchoredPosition.y, pos, Time.deltaTime * 5f);
                newPosition = new Vector2(ScrollPanel.anchoredPosition.x, newP);
                break;
        }
        ScrollPanel.anchoredPosition = newPosition;
    }

    Vector3 scale = new Vector3(0.0085f, 0.0085f, 0.0085f);
    void ScaleUpAndScaleDown(int index)
    {
        for (int i = 0; i < lstObj.Count; i++)
            if (i == index)
            {
                if (lstObj[i].GetComponent<RectTransform>().localScale.x <= 1.0f)
                    lstObj[i].GetComponent<RectTransform>().localScale += Vector3.Lerp(scale, lstObj[i].GetComponent<RectTransform>().localScale, Time.deltaTime * 0.5f);
            }
            else
            {
                if (lstObj[i].GetComponent<RectTransform>().localScale.x >= 0.85f)
                    lstObj[i].GetComponent<RectTransform>().localScale -= Vector3.Lerp(scale, lstObj[i].GetComponent<RectTransform>().localScale, Time.deltaTime * 0.5f);
            }
    }
}

public enum StyleList
{
    Vertical,
    Horizontal
}