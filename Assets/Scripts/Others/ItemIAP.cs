using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemIAP : MonoBehaviour
{
    public Text modification, price;
    public Image Icon;

    public int index;
    public void Start()
    {
        if (index >= 8 && index != 14)
            StartCoroutine(WaitPurchaser());
    }

    IEnumerator WaitPurchaser()
    {
        while (!Purchaser.Instance.IsInitialized())
            yield return new WaitForSeconds(0.1f);
        UpdateData(index);
        yield break;
    }

    public void UpdateData(int index)
    {
        this.index = index;
        InforProducts inp = Purchaser.Instance.infomationProducts[index];
        if(modification) modification.text = inp.Description.ToString();

        if (inp.ID != "")
        {
            price.text = Purchaser.Instance.LocalizedPrice(inp.ID);
            if (price.text == "") price.text = "$" + inp.Price.ToString();
        }
        else
            price.text = "FREE";

        if (Icon && inp.Sprite)
            Icon.sprite = inp.Sprite;
    }

    public void OnClick()
    {
        //print(this.index);
        string ID = Purchaser.Instance.infomationProducts[index].ID;
        if (ID != "")
            Purchaser.Instance.BuyProductID(ID);
        else
        {
            if (Application.isMobilePlatform)
            {
                AdController.Instance.ShowReward(OnVideoClosed);
            }
            else {
            }
        }
    }

    public void OnVideoClosed()
    {
        StopAllCoroutines();
        //if (obj)
        {
            switch (Purchaser.Instance.infomationProducts[index].Modification)
            {
                case Modification.Coin:
                    break;

                case Modification.Hint:
                    break;
            }
        }
    }
}
