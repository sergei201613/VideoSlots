using UnityEngine;
using System.Collections;
using DG.Tweening;

/// <summary>
/// Tile cell effect and touch/click event.
/// </summary>
public class Tile : MonoBehaviour {
    public SlotMachine slotMachine;

    // tile order
    public int idx = 0;

    public float height = 1f;

    // tile type
    int _type = 0;

    Transform tf;

    //public Sprite[] sprites;
    SpriteRenderer shapeRenderer;
    GameObject choideRenderer;

    // Tile Line Component
    public Line lineScript;

    // Condition when tile is moving
    public bool isMove = false;

    void Awake()
    {
        tf = transform;
        shapeRenderer = tf.Find("Shape").GetComponent<SpriteRenderer>();
        choideRenderer = tf.Find("Choice").gameObject;
        UnSetChoice();
    }

    // Setup Tile Type.
    public void SetTileType(int type)
    {
        if (type < 0) return;
        _type = type;

        //WILD
        if (type == slotMachine.wildIndex && slotMachine.payTable.shortWild.Length > 0 && slotMachine.sWild % slotMachine.payTable.shortWild.Length < slotMachine.payTable.shortWild.Length)
        {
            shapeRenderer.sprite = slotMachine.payTable.shortWild[slotMachine.sWild % slotMachine.payTable.shortWild.Length];
            choideRenderer.GetComponent<GifImage>().gifPath = slotMachine.payTable.settingPays[type].gifPath + "-" + (slotMachine.sWild % slotMachine.payTable.shortWild.Length + 1);
        }
        //BONUS
        else if (type == slotMachine.bonusIndex && slotMachine.payTable.shortBonus.Length > 0 && slotMachine.sBonus % slotMachine.payTable.shortBonus.Length < slotMachine.payTable.shortBonus.Length)
        {
            shapeRenderer.sprite = slotMachine.payTable.shortBonus[slotMachine.sBonus % slotMachine.payTable.shortBonus.Length];
            choideRenderer.GetComponent<GifImage>().gifPath = slotMachine.payTable.settingPays[type].gifPath + "-" + (slotMachine.sBonus % slotMachine.payTable.shortBonus.Length + 1);
        }
        //SCATTER
        else if (type == slotMachine.scatterIndex && slotMachine.payTable.shortScatter.Length > 0 && slotMachine.sScatter % slotMachine.payTable.shortScatter.Length < slotMachine.payTable.shortScatter.Length)
        {
            shapeRenderer.sprite = slotMachine.payTable.shortScatter[slotMachine.sScatter % slotMachine.payTable.shortScatter.Length];
            choideRenderer.GetComponent<GifImage>().gifPath = slotMachine.payTable.settingPays[type].gifPath + "-" + (slotMachine.sScatter % slotMachine.payTable.shortScatter.Length + 1);
        }
        //OTHER
        else if (type == slotMachine.otherIndex && slotMachine.payTable.shortOther.Length > 0 && slotMachine.sOther % slotMachine.payTable.shortOther.Length < slotMachine.payTable.shortOther.Length)
        {
            shapeRenderer.sprite = slotMachine.payTable.shortOther[slotMachine.sOther % slotMachine.payTable.shortOther.Length];
            choideRenderer.GetComponent<GifImage>().gifPath = slotMachine.payTable.settingPays[type].gifPath + "-" + (slotMachine.sOther % slotMachine.payTable.shortOther.Length + 1);
        }
        else
        {
            shapeRenderer.sprite = slotMachine.payTable.GetAvatar(type);
            choideRenderer.GetComponent<GifImage>().gifPath = slotMachine.payTable.settingPays[type].gifPath;
        }

        foreach (Transform tr in choideRenderer.transform) if (tr.gameObject) Destroy(tr.gameObject);
        if (slotMachine.payTable.settingPays[type].fxSquare)
        {
            GameObject fx = Instantiate(slotMachine.payTable.settingPays[type].fxSquare, choideRenderer.transform, false);
            fx.transform.localPosition = Vector3.zero;
        }

        if (GetTileType() == slotMachine.wildIndex)
            choideRenderer.GetComponent<SpriteRenderer>().sortingOrder = 1;
        else
            choideRenderer.GetComponent<SpriteRenderer>().sortingOrder = 0;
    }

    public void SetChoice()
    {
        choideRenderer.SetActive(true);
        shapeRenderer.gameObject.SetActive(false);
    }

    public void UnSetChoice()
    {
        choideRenderer.SetActive(false);
        shapeRenderer.gameObject.SetActive(true);
    }

    // Get Tile Type.
    public int GetTileType()
    {
        return _type;
    }

    Coroutine unsetchoise;
    public void ShowChoice(float t)
    {
        SetChoice();
        if (unsetchoise != null) StopCoroutine(unsetchoise);
        unsetchoise = StartCoroutine(DelayAction(t, () =>
        {
            UnSetChoice();
        }));
        shapeRenderer.transform.localScale = Vector3.one;
    }

    // Move To Order Position
    public void MoveTo(int seq)
    {
        tf.localPosition = Vector3.forward * (seq * height);
    }

    // Move with Tweening Animation
    public void TweenMoveTo(int seq, bool isLinear)
    {
        if (isLinear)
            TweenMove(tf, tf.localPosition, Vector3.forward * (seq * height));
        else
            TweenMove2Back(tf, tf.localPosition, Vector3.forward * ((seq-0.4f) * height), Vector3.forward * (seq * height));
    }

    // Move with Tweening Animation
    void TweenMove(Transform tr, Vector3 pos1, Vector3 pos2)
    {
        tr.localPosition = pos1;
        tr.DOLocalMove(pos2, 0.1f).SetEase(Ease.Linear);
    }
    void TweenMove2Back(Transform tr, Vector3 pos1, Vector3 pos2, Vector3 pos3)
    {
        tr.localPosition = pos1;
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(tr.DOLocalMove(pos2, 0.1f).SetEase(Ease.Linear));
        mySequence.Append(tr.DOLocalMove(pos3, 0.1f).SetEase(Ease.Linear));
    }

    // Coroutine Delay Method
    IEnumerator DelayAction(float dTime, System.Action callback)
    {
        yield return new WaitForSeconds(dTime);
        callback();
    }
}
