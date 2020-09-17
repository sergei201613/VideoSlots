using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokerController : MonoBehaviour {
    public string pokerPath = "Cards/";
    Sprite faceDown;
    List<Poker> pokers;
    public Transform startPos, endPos;
    public GameObject itemPoker;
    List<Transform> cards;
    List<Transform> points;

    public AudioClip[] audioClips;

    private void Awake()
    {
        pokers = new List<Poker>();
        Sprite[] sps = Resources.LoadAll<Sprite>(pokerPath);
        for (int i = 0; i < sps.Length; i++)
            if (sps[i])
            {
                if (sps[i].name.Contains("FaceDown"))
                    faceDown = sps[i];
                else
                {
                    Poker p = new Poker();
                    p.sprite = sps[i];
                    if (sps[i].name.Contains("Diamonds"))
                        p.pokerType = PokerType.Diamonds;
                    if (sps[i].name.Contains("Hearts"))
                        p.pokerType = PokerType.Hearts;
                    if (sps[i].name.Contains("Spade"))
                        p.pokerType = PokerType.Spade;
                    if (sps[i].name.Contains("Clubs"))
                        p.pokerType = PokerType.Clubs;
                    pokers.Add(p);
                }       
            }
    }

    void Start()
    {
        cards = new List<Transform>();
        points = new List<Transform>();
        foreach (Transform tr in endPos)
            points.Add(tr);
    }

    bool allowBet = true;
    string typeBet = "";
    public void BetPoker(string type)
    {
        if (!allowBet) return;

        allowBet = false;
        typeBet = type;
        InitPoker();
    }

    GameObject curP;
    void InitPoker()
    {
        curP = Instantiate(itemPoker, startPos, false);
        curP.transform.position = startPos.position;
        curP.GetComponent<ItemPoker>().UpdateData(faceDown);
        StartCoroutine(MoveThenFlipPoker());
        riskTimes++;
    }

    IEnumerator MoveThenFlipPoker()
    {
        curP.transform.DOMove(transform.position, 0.5f).SetEase(Ease.InSine);
        curP.transform.DOScale(new Vector3(1.2f, 1.2f), 0.5f).SetEase(Ease.InSine);
        yield return new WaitForSeconds(0.5f);
        Vector3 v = curP.transform.eulerAngles;
        v.y = 90f;
        curP.transform.DORotate(v, 0.5f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(0.5f);
        int r = Random.Range(0, pokers.Count);
        curP.GetComponent<ItemPoker>().UpdateData(pokers[r].sprite);
        v.y = 0f;
        curP.transform.DORotate(v, 0.5f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(0.5f);
        FlipCompleted();
        curP.transform.DOMove(points[cards.Count - 1].position, 0.5f).SetEase(Ease.InSine);
        curP.transform.DOScale(endPos.localScale, 0.5f).SetEase(Ease.InSine);
        yield return new WaitForSeconds(0.5f);
        //check res
        switch (typeBet)
        {
            case "red":
                if (pokers[r].pokerType == PokerType.Diamonds || pokers[r].pokerType == PokerType.Hearts)
                    Win(2);
                else
                    Lose();
                break;

            case "black":
                if (pokers[r].pokerType == PokerType.Spade || pokers[r].pokerType == PokerType.Clubs)
                    Win(2);
                else
                    Lose();
                break;

            case "diamonds":
                if (pokers[r].pokerType == PokerType.Diamonds)
                    Win(4);
                else
                    Lose();
                break;

            case "hearts":
                if (pokers[r].pokerType == PokerType.Hearts)
                    Win(4);
                else
                    Lose();
                break;

            case "spade":
                if (pokers[r].pokerType == PokerType.Spade)
                    Win(4);
                else
                    Lose();
                break;

            case "clubs":
                if (pokers[r].pokerType == PokerType.Clubs)
                    Win(4);
                else
                    Lose();
                break;
        }

        allowBet = true;
        yield break;
    }

    double lastWin = 0;
    int riskTimes = 0;
    void Win(int x)
    {
        if (riskTimes == 1) lastWin = SlotMachine.Instance.lastWin;
        lastWin *= x;
        ResetTimes();
        if(audioClips.Length > 0 && audioClips[0])
            AudioSource.PlayClipAtPoint(audioClips[0], Camera.main.transform.position, 1f);
    }

    void Lose()
    {
        lastWin = -SlotMachine.Instance.lastWin;
        if (audioClips.Length > 1 && audioClips[1])
            AudioSource.PlayClipAtPoint(audioClips[1], Camera.main.transform.position, 1f);
        Close();
    }

    public void Close()
    {   
        SlotMachine.Instance.PlusMoney(this.lastWin);
        if (lastWin > 0)
            SlotMachine.Instance.SetWin(this.lastWin, 0);
        else
            SlotMachine.Instance.SetWin(0);

        SlotMachine.Instance.CloseBonus(TyleStyle.Wild);
        lastWin = 0;
        riskTimes = 0;
        ResetTimes();
        //foreach (Transform tr in startPos)
        //    if (tr) Destroy(tr.gameObject);
    }

    void FlipCompleted()
    {
        MovePoker();
        cards.Add(curP.transform);
        if (cards.Count > 5)
        {
            Destroy(cards[0].gameObject, 1f);
            cards.RemoveAt(0);
        }
    }

    void MovePoker()
    {
        if (cards.Count < 5) return;
        for (int i = 1; i < cards.Count; i++)
            cards[i].DOMove(points[i - 1].position, 1f).SetEase(Ease.InOutQuad);
    }

    void ResetTimes()
    {
        if (riskTimes > 5)
        {
            Close();
            riskTimes = 0;
        }
    }
}

[System.Serializable]
public class Poker
{
    public PokerType pokerType;
    public Sprite sprite;
}

public enum PokerType
{
    Diamonds,   //Rô
    Hearts,     //Cơ
    Spade,      //Bích
    Clubs       //Nhép
}