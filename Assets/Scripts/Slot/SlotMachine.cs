using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// manage tile grid
/// </summary>
public class SlotMachine : MonoBehaviour {
    // tile prefab
    public GameObject cellItemPrefab, realPartical, mainGame, butRisk, butSpin, ranking, butAutoSpin, butFreeSpin;
    public PayTable payTable;

    public GameObject[] effectWin;
    public GameObject fxImage;
    public string[] pathTextWin;
    public AudioClip[] soundWin;
    public Sprite[] background;
    [SerializeField]
    Image BG;

    // tile lines
    Transform[] lines;

    // tile items
    public Tile[] items;

    // auto clear match tiles game
    public bool isAuto = false;

    // tile size
    public Vector3 cellSize = new Vector3(1.6f, 1.4f, 1f);

    // tile scale
    public Vector3 cellScale = new Vector3(1.4f, 1f, 1.4f);

    public AudioSource spinSound, stopSound, cashSound, clickSound, bgFX, anticipationSound;

    public HighlightLine highlightLine;

    Line[] lineList;

    int totalLine = 5;
    int totalCell = 7;
    [SerializeField]
    double money = 10000;
    [HideInInspector]
    public int lanes = 25;
    int lastLanes = 25;
    [HideInInspector]
    public int betIndex = 0;
    double bet = 1;
    [HideInInspector]
    public double totalBet = 25;

    public Text moneyLabel, lineBetLabel, betLabel, lineLabel, winLabel, winLabelShow, freeSpinsLabel;
    public GameObject freeSpinsObject;

    //Transform cam;
    bool isInput = true;

    int totalSymbols;
    [HideInInspector]
    public int wildIndex = -1, bonusIndex = -1, otherIndex = -1, scatterIndex = -1;
    int totalFreeSpins;
    bool showTotalWin = false;
    double totalFreeWin = 0;

    public GameObject riskBackground;
    public static SlotMachine Instance;
    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        double.TryParse(PlayerPrefs.GetString("mm", "10000000"), out money);
        totalSpin = PlayerPrefs.GetInt("totalSpin", 0);

        //load background image
        background = payTable.GetSprites("background");
        AudioClip clip = AssetBundleController.Instance.bundle.LoadAsset<AudioClip>(payTable.curPath + "sounds/background.wav");
        if(!clip) clip = AssetBundleController.Instance.bundle.LoadAsset<AudioClip>(payTable.curPath + "sounds/background.mp3");
        if (clip)
        {
            bgFX.clip = clip;
            bgFX.Play();
        }
    }

    public void LoadSymbol()
    {
        totalSymbols = payTable.settingPays.Count; // PayData.table.GetLength(0);
        for (int i = 0; i < payTable.settingPays.Count; i++)
        {
            switch (payTable.settingPays[i].tyleStyle)
            {
                case TyleStyle.Scatter:
                    scatterIndex = i;
                    break;

                case TyleStyle.Wild:
                    wildIndex = i;
                    break;

                case TyleStyle.Bonus:
                    bonusIndex = i;
                    break;

                case TyleStyle.Other:
                    otherIndex = i;
                    break;
            }
        }
        totalFreeSpins = 0;
        InitArena();
    }

    void Start()
    {
        //if (ADSController.Instance) ADSController.Instance.RequestInterstitial(false);
        LoadIAPList();
        cads = Random.Range(25, 35);

        StartCoroutine(SendJackpot(0));

        //cam = Camera.main.transform;
        //Vector3 pos = cam.position;
        //pos.y = 8f;
        lanes = PayData.line.GetLength(0);
        lastLanes = lanes;
        InitLabels();
        mainGame.GetComponent<Animator>().SetTrigger("Show");
        LevelUpdate(0);
        if (butRisk)
        {
            //butRisk.GetComponent<Button>().interactable = false;
            butRisk.GetComponent<Button>().onClick.AddListener(() =>
            {
                foreach (var item in PayTable.Instance.settingBonus)
                    if (item.tyleStyle == TyleStyle.Wild && item.bonusObj)
                    {
                        mainGame.GetComponent<Animator>().SetTrigger("Hide");
                        item.bonusObj.GetComponent<Animator>().SetTrigger("Show");
                        if (riskCoroutine != null) StopCoroutine(riskCoroutine);
                        if (autoSpinCorotine != null) StopCoroutine(autoSpinCorotine);
                        if (showWinValue != null) StopCoroutine(showWinValue);
                        if (riskBackground) riskBackground.SetActive(true);
                        inRisk = true;
                        break;
                    }
            });
        }

        UpdateBackground();

        fxImage.SetActive(false);

        butSpin.SetActive(!isAuto);
        butAutoSpin.SetActive(isAuto);
    }

    bool inRisk = false, inBonus = false;

    public void GetRank()
    {
        mainGame.GetComponent<Animator>().SetTrigger("Hide");
        ranking.GetComponent<Animator>().SetTrigger("Show");
        RankingController.Instance.GetData("getrank");
    }

    private void LateUpdate()
    {
        butSpin.GetComponent<Button>().interactable = isInput && !isAuto && !inBonus;
    }

    float lastDown = 0;
    public void OnSpinPress(bool isDown)
    {
        if (isDown)
            waitHolder = StartCoroutine(WaitHolder());
        else
            if(waitHolder != null) StopCoroutine(waitHolder);
    }

    Coroutine waitHolder;
    IEnumerator WaitHolder()
    {
        yield return new WaitForSeconds(2f);
        if (butSpin.activeSelf)
        {
            butSpin.SetActive(false);
            butAutoSpin.SetActive(true);
            ToggleAuto();
            clickSound.Play();
        }
        yield break;
    }

    int lastBG = -1;
    void UpdateBackground()
    {
        int r = Random.Range(0, background.Length);
        while (lastBG == r)
            r = Random.Range(0, background.Length);
        lastBG = r;
        if (background[r] == null) UpdateBackground();
        else
        {
            BG.sprite = background[r];
            if (background.Length > 1)
                Invoke("UpdateBackground", 180f);
        }
    }

    public void PlusLine()
    {
        if (totalFreeSpins > 0) return;
        if (lanes < PayData.line.GetLength(0)) lanes++;
        else lanes = 1;
        SetLine();
    }

    public void MinusLine()
    {
        if (totalFreeSpins > 0) return;
        if (lanes > 2) lanes--;
        else lanes = PayData.line.GetLength(0);
        SetLine();
    }

    public void AutoLine()
    {

        if (money < totalBet)
        {
            lanes = (int)(money / PayData.betline[betIndex]);
            if (lanes > 1 && (double)lanes * PayData.betline[betIndex] > money)
                lanes--;
            SetLine();
        }
    }

    void SetLine()
    {
        lineLabel.text = lanes.ToString();
        SetBet();
    }

    public void MinusBet()
    {
        //if (betIndex < 1) return;
        //betIndex = (betIndex - 1 + PayData.betting.Length) % PayData.betting.Length;
        if (totalFreeSpins > 0) return;
        betIndex = (betIndex - 1 + PayData.betline.Length) % PayData.betline.Length;

        if (money < PayData.betline[betIndex] * lanes * GetCounter())
        //AutoBet(1);
        {

        }
        else
            SetBet(true);
    }

    public void PlusBet()
    {
        //if (PayData.betting.Length - 2 < betIndex) return;
        //betIndex = (betIndex + 1) % PayData.betting.Length;
        if (totalFreeSpins > 0) return;
        int lastbet = betIndex;
        betIndex = (betIndex + 1) % PayData.betline.Length;
        if (money < PayData.betline[betIndex] * lanes * GetCounter())
            AutoBet(0, true);
        else
            SetBet(true);
    }

    public void AutoBet(int from = 0, bool isMax = false)
    {
        if (!isMax)
        {
            for (int i = from; i < PayData.betline.Length; i++)
                if (PayData.betline[i] * (double)lanes * GetCounter() <= money)
                {
                    betIndex = i;
                    SetBet();
                    return;
                }
        }
        else
        {
            for (int i = PayData.betline.Length - 1; i >= 0; i--)
                if (PayData.betline[i] * (double)lanes * GetCounter() <= money)
                {
                    betIndex = i;
                    SetBet();
                    return;
                }
        }
    }

    public GameObject loadingADS;
    public void GetFreeCoin()
    {
        if (!Application.isMobilePlatform) return;
        loadingADS.GetComponent<Animator>().SetTrigger("Show");
        if (AdController.Instance.HaveRewardVideo())
        {
            AdController.Instance.ShowReward(() =>
            {
                loadingADS.GetComponent<Animator>().SetTrigger("Hide");
                Invoke("SetAds", 0.5f);
            });
        }else
            loadingADS.GetComponent<Animator>().SetTrigger("Hide");
    }

    void SetAds()
    {
        PlusMoney(300);
    }

    int GetCounter()
    {
        int c = (int)(money / PayData.betline[PayData.betline.Length - 1] / 9);
        if (c <= 0) c = 1;
        return c;
    }

    void SetBet(bool s = false)
    {
        //totalBet = PayData.betting[betIndex];
        //bet = totalBet / lanes;
        //int counter = Experience.Instance.vLevel / 10;
        bet = PayData.betline[betIndex];
        bet *= GetCounter();
        //if (counter == 2)
        //    bet *= 2;
        //if (counter >= 3)
        //    bet *= 5;
        //if (money > 4500000)
        //{
        //    bet = PayData.betline[betIndex];
        //    bet *= 10;
        //}

        totalBet = bet * lanes;
        lineBetLabel.text = bet.ToString("#,##0");
        betLabel.text = totalBet.ToString("#,##0");
        if (s)
            AudioSource.PlayClipAtPoint(payTable.betClips[betIndex], Camera.main.transform.position, 1f);
    }

    public void SetMoney(double val)
    {
        tweenMoney = money;
        money = val;
        if (money < 0) money = 0;
        if (money - tweenMoney > 0)
            TweenMoney(money - tweenMoney);
        else
            moneyLabel.text = (money).ToString("#,##0");

        if (money <= PayData.betline[0] * (double)PayData.line.GetLength(0))
        {
            int i = PlayerPrefs.GetInt("soffer", 0);
            if (i == 0)
                SpecialOffer(0);
            else if (i == 1)
                SpecialOffer(1);
            else
                animIAP.SetTrigger("Open");
        }
        
        PlayerPrefs.SetString("mm", money.ToString());
        PlayerPrefs.Save();
    }

    public double tweenMoney;
    public CoinFly fxCoin;
    void TweenMoney(double val)
    {
        fxCoin.RunAnim(3);
        DOTween.To(() => tweenMoney, x => tweenMoney = x, tweenMoney + val, 3).OnUpdate(()=> {
            moneyLabel.text = (tweenMoney).ToString("#,##0");
        }).SetDelay(1f);
    }

    public void PlusMoney(double val)
    {
        SetMoney(money + val);
    }

    [HideInInspector]
    public double lastWin = 0;
    Coroutine showWinValue;
    bool showWin = true;
    public void SetWin(double val, float timedelay = -1)
    {
        if (totalFreeSpins > 0) totalFreeWin += val;

        winLabel.text = val.ToString("#,##0");
        if (val > 0)
            winLabelShow.text = val.ToString("#,##0");
        else
            winLabelShow.text = "";

        lastWin = val;
        //if (timedelay == -1) timedelay = PayData.timeShowChoise / 2;
        //if (val > 0)
        //    showWinValue = StartCoroutine(DelayAction(timedelay, () =>
        //    {
        //        if (showWin)
        //            ShowWin();
        //    }));
    }

    public Animator animWin;
    public void ShowWin()
    {
        if (winLabelShow.text == "") return;
        animWin.ResetTrigger("Open");
        animWin.SetTrigger("Open");
    }

    void SetFreeSpins(int val)
    {
        freeSpinsLabel.text = val.ToString();
        //if (val > 0)
        //{
        //    lastLanes = lanes;
        //    lanes = PayData.line.GetLength(0);
        //}
        //else
        //{
        //    lanes = lastLanes;
        //}
    }

    void DisplayFreeSpins(bool isON)
    {
        butFreeSpin.SetActive(isON);
        if (freeSpinsObject)
        {
            if (isON)
                freeSpinsObject.GetComponent<Animator>().SetTrigger("Open");
            else
            {
                indexRandom = -1;
                cardRandom.GetChild(1).GetComponent<Animator>().SetTrigger("Close");
                freeSpinsObject.GetComponent<Animator>().SetTrigger("Close");
            }
            //freeSpinsObject.SetActive(isON);
        }
    }

    void InitLabels()
    {
        SetFreeSpins(0);
        //DisplayFreeSpins(false);
        SetLine();
        //SetBet();
        //AutoBet();
        SetMoney(money);
        SetWin(0);
        butFreeSpin.SetActive(false);
    }

    public void ToggleAuto()
    {
        isAuto = !isAuto;
        if (isAuto && isInput) Spin();
        else if (!isAuto && autoSpinCorotine != null && totalFreeSpins <= 0) StopCoroutine(autoSpinCorotine);
        if (!isAuto)
        {
            butSpin.SetActive(true);
            butAutoSpin.SetActive(false);
        }
    }

    // init game arena, draw tile grid
    void InitArena()
    {
        lines = new Transform[totalLine];
        lineList = new Line[totalLine];

        // tile line loop
        for (int i = 0; i < totalLine; i++)
        {
            GameObject pgo = new GameObject();
            pgo.name = "Base" + (i + 1).ToString("000");
            pgo.transform.parent = transform;
            GameObject go = new GameObject();
            go.transform.parent = pgo.transform;
            Line script = go.AddComponent<Line>();
            script.idx = i;
            Transform tf = go.transform;
            lines[i] = tf;
            tf.parent = pgo.transform;
            //tf.localPosition = (i-4.5f) * Vector3.right * cellSize.x + Vector3.forward * (2 - (i % 2) * 0.5f) * cellSize.y;
            tf.localPosition = (i - 4.5f) * Vector3.right * cellSize.x + Vector3.forward * cellSize.y;
            tf.localScale = Vector3.one;
            go.name = "Line" + (i + 1).ToString("000");

            script.items = new Tile[totalCell];
            // tile loop in some line
            for (int j = 0; j < totalCell; j++)
            {
                GameObject g = Instantiate(cellItemPrefab) as GameObject;
                g.name = "Tile" + (j + 1).ToString("000");
                Transform t = g.transform;
                Tile c = g.GetComponent<Tile>();
                c.height = cellSize.y;
                c.slotMachine = this;
                c.SetTileType(Random.Range(0, totalSymbols) % totalSymbols);
                c.lineScript = script;
                script.items[j] = c;
                c.idx = j;
                t.parent = tf;
                t.localPosition = Vector3.forward * j * cellSize.y;
                t.localScale = cellScale;
                t.localRotation = Quaternion.identity;
                if (realPartical)
                {
                    if (j == 3)
                    {
                        GameObject te = Instantiate(realPartical, tf, false);
                        te.transform.localPosition = g.transform.localPosition;
                        te.SetActive(false);
                        if (objsc == null || objsc.Count == 0) objsc = new List<GameObject>();
                        objsc.Add(te);
                    }
                }
            }
            script.idx = i;
            lineList[i] = script;
        }

        items = GetComponentsInChildren<Tile>();
        ClearChoice();
    }

    // delay method coroutine
    IEnumerator DelayAction(float dTime, System.Action callback)
    {
        yield return new WaitForSeconds(dTime);
        callback();
    }

    IEnumerator RepeatAction(float dTime, int count, System.Action callback1, System.Action callback2, System.Action callback3, int col = -1)
    {
        if (col != -1 && count > 1)
        {
            if (wassc[col])
            {
                count += 7 * col;
                wassc[col] = false;
            }
        }

        if (count > 1)
        {
            callback1();
        }
        else
        {
            callback3();
        }

        if (--count > 0)
        {
            if (count == 1) callback2();
            yield return new WaitForSeconds(dTime);
            StartCoroutine(RepeatAction(dTime, count, callback1, callback2, callback3, col));
        }
    }

    bool[] wassc;
    List<GameObject> objsc;
    // auto clear match tiles and go next turn
    void DoRoll()
    {
        wasHearWild = false;
        wassc = new bool[totalLine];
        for (int i = 0; i < totalLine; i++)
        {
            wassc[i] = false;
            if (objsc != null && i < objsc.Count && objsc[i]) objsc[i].SetActive(false);
        }

        for (int i = 0; i < totalLine; i++)
        {
            Transform line = lines[i];
            StartCoroutine(RepeatAction(0.05f, 8 + i * 5, () =>
            {
                line.SendMessage("RollCells", true, SendMessageOptions.DontRequireReceiver);
            }, () =>
            {
                stopSound.Play();
            }, () =>
            {
                line.SendMessage("RollCells", false, SendMessageOptions.DontRequireReceiver);
            }, i));
        }
        isInput = false;
        //StartCoroutine(DelayAction(2.2f, () =>
        //{
        //    isInput = true;
        //    FindMatch(0, 2);
        //}));
    }

    [Header("SETTING RANDOM CARD FREE SPIN")]
    public Transform cardRandom;
    public AudioClip[] soundRandom;
    public Sprite[] spritesRandom;
    int indexRandom = -1;
    public IEnumerator FXRandom()
    {
        AudioClip c = SoundBonus(TyleStyle.Scatter);
        if (c) AudioSource.PlayClipAtPoint(c, Camera.main.transform.position, 1f);

        if (spritesRandom.Length > 2)
        {
            cardRandom.GetChild(0).GetComponent<Image>().sprite = spritesRandom[1];
            if (totalFreeSpins <= 10)
                cardRandom.GetChild(0).GetComponent<Image>().sprite = spritesRandom[0];
        }

        cardRandom.GetChild(0).gameObject.SetActive(true);
        cardRandom.GetChild(0).GetComponent<Animator>().SetTrigger("Open");
        yield return new WaitForSeconds(1f);
        if (totalFreeSpins <= PayData.totalFreeSpins)
        {
            yield return new WaitForSeconds(2f);
            cardRandom.GetChild(0).GetComponent<Animator>().SetTrigger("Close");

            cardRandom.GetChild(1).GetComponent<Floating>().enabled = false;
            cardRandom.GetChild(1).localPosition = Vector3.zero;
            cardRandom.GetChild(1).GetComponent<Animator>().SetTrigger("Open");
            float t = 3f;
            while (t >= 0)
            {
                indexRandom = RandomType();
                AudioSource.PlayClipAtPoint(soundRandom[0], Camera.main.transform.position, 1f);
                cardRandom.GetChild(1).GetComponent<Image>().sprite = payTable.GetAvatar(indexRandom);
                t -= 0.25f;
                yield return new WaitForSeconds(0.25f);
            }

            //iTween.MoveTo(cardRandom.GetChild(1).gameObject, iTween.Hash("position", cardRandom.GetChild(2).transform.position, "time", 2f, "easyType", iTween.EaseType.easeInSine));
            cardRandom.GetChild(1).DOMove(cardRandom.GetChild(2).transform.position, 2f).SetEase(Ease.InSine);
            yield return new WaitForSeconds(1f);
            cardRandom.GetChild(0).gameObject.SetActive(false);
            yield return new WaitForSeconds(2f);
            cardRandom.GetChild(1).GetComponent<Floating>().enabled = true;
        }
        else
        {
            yield return new WaitForSeconds(2f);
            cardRandom.GetChild(0).GetComponent<Animator>().SetTrigger("Close");
            yield return new WaitForSeconds(1f);
            cardRandom.GetChild(0).gameObject.SetActive(false);
        }
        DisplayFreeSpins(true);

        AutoSpin();
        yield break;
    }

     //[Header("SETTING BOSTER EXP")]
    float timeCounter;
    IEnumerator FXBooster(Transform boster)
    {
        timeCounter = 120f;
        boster.GetChild(0).GetComponent<Animator>().SetTrigger("Open");
        yield return new WaitForSeconds(2f);
        //if (tileOther) tileOther.SetTileType(RandomType());
        yield return new WaitForSeconds(1f);
        //iTween.MoveTo(boster.GetChild(0).gameObject, iTween.Hash("position", boster.GetChild(2).position, "time", 1f, "easyType", iTween.EaseType.easeInSine));
        boster.GetChild(0).DOMove(boster.GetChild(2).position, 1f).SetEase(Ease.InSine);
        boster.GetChild(0).GetComponent<Animator>().SetTrigger("Close");
        boster.GetChild(1).GetComponent<Animator>().SetTrigger("Open");
        Text timer = boster.GetChild(1).GetComponent<Text>();
        timeCounter -= 2f;
        while (timeCounter >= 0)
        {
            string minSec = string.Format("{0}:{1:00}", (int)timeCounter / 60, (int)timeCounter % 60);
            timer.text = "BOOSTER TIME REMAINING: " + minSec;
            timeCounter--;
            yield return new WaitForSeconds(1f);
        }
        boster.GetChild(1).GetComponent<Animator>().SetTrigger("Close");
        yield break;
    }


    bool allowWild = false, allowScatter = false, allowBonus = false, allowOther = false, allow2Scatter = false, allowWin = false;
    [HideInInspector]
    public int sScatter = 0, sWild = 0, sOther = 0, sBonus = 0;
    int totalSpin = 0;

    void RandomStateResult()
    {
        //allowOther = true;
        //if (timeCounter > 0) allowOther = false;
        //allowWild = true; allowScatter = true; allowBonus = true; allowOther = true; allow2Scatter = true; allowWin = true;
        //allowScatter = true;
        //allowBonus = true;
        //allowWin = true;
        //sScatter = 0;
        //if (totalFreeSpins > 0) allowScatter = false;
        //allowWild = true; allowWin = true;
        //return;
        //totalSpin = 1;
        allowWild = false; allowScatter = false; allowBonus = false; allowOther = false; allow2Scatter = false; allowWin = false;
        float r = Random.value;
        if(totalSpin == -1)
        {
            if (r <= 0.1f) allowScatter = true;
            else if (r <= 0.25f) allowWild = true;
            else if (r <= 0.4f) allowBonus = true;
            else if (r <= 0.55f) allow2Scatter = true;
            else if (r <= 0.7f) allowOther = true;
            if (r >= 0.7f) allowWin = true;
        }
        else if(totalSpin <= 100)
        {
            if (r <= 0.6f) allowScatter = true;
            if (r >= 0.87f) allowWild = true;
            if (r <= 0.6f) allowBonus = true;
            if (r >= 0.4f) allow2Scatter = true;
            if (r <= 0.95f) allowOther = true;
            if (r >= 0.6f) allowWin = true;
        }
        //else if (totalSpin <= 100)
        //{
        //    if (r <= 0.35f) allowScatter = true;
        //    else if(r <= 0.7f) allowWild = true;
        //    if (r >= 0.65f) allowBonus = true;
        //    else if (r >= 0.3f) allow2Scatter = true;
        //    if (r <= 0.95f) allowOther = true;
        //    if (r >= 0.6f) allowWin = true;
        //}
        //else if (totalSpin <= 1000)
        //{
        //    if (r <= 0.05f) allowScatter = true;
        //    else if (r <= 0.30f) allowWild = true;
        //    else if (r <= 0.45f) allowBonus = true;
        //    else if (r <= 0.6f) allow2Scatter = true;
        //    else if (r <= 0.75f) allowOther = true;
        //    if (r >= 0.7f) allowWin = true;
        //}
        else {
            if (r <= 0.03f) allowScatter = true;
            else if (r <= 0.08f) allowWild = true;
            else if (r <= 0.13f) allowBonus = true;
            else if (r <= 0.18f) allow2Scatter = true;
            else if (r <= 0.23f) allowOther = true;
            if (r >= 0.9f) allowWin = true;
        }

        if (totalSpin <= 100)
            allowWin = r <= 0.65f;
        else if (totalSpin <= 200)
            allowWin = r <= 0.75f;
        else
            allowWin = r <= 0.87f;


        if (timeCounter > 0) allowOther = false;
        if (totalFreeSpins >= 5) allowScatter = false;
        if (allowScatter) allow2Scatter = false;
        sScatter = 0;
        sOther = 0;
        sWild = 0;
        sBonus = 0;
    }
    [SerializeField]
    AudioClip[] soundScatter;

    [SerializeField]
    AudioClip[] soundBackground;

    List<Tile> result;
    public void ChangeResult(int column, Tile[] tiles)
    {
        int st = 0;
        foreach (var t in tiles)
        {
            if (t.GetTileType() == scatterIndex)
            {
                st++;
                if (st > 1) t.SetTileType(RandomType());
            }

            if (t.GetTileType() == otherIndex)
            {
                sOther++;
                if ((!allowOther && sOther >= 1) || (allowOther && sOther >= 2))
                    t.SetTileType(RandomType());
            }
        }

        if (column == 0)
            result = new List<Tile>();

        if (!allowWin)
        {
            if (column < 3)
                foreach (var t in tiles)
                    if (t.GetTileType() == wildIndex)
                        t.SetTileType(RandomType());
        }

        if(!allowWin && column == 1 && indexRandom == -1)
        {
            List<int> rs = new List<int>();
            int i = 0;
            foreach (var p in payTable.settingPays)
            {
                if (p.tyleStyle == TyleStyle.Normal
                    && i != result[0].GetTileType()
                    && i != result[1].GetTileType()
                    && i != result[2].GetTileType())
                {
                    rs.Add(i);
                }
                i++;
            }
            foreach (var t in tiles)
                t.SetTileType(rs[Random.Range(0, rs.Count)]);
        }

        int t1 = tiles[0].GetTileType(), t2 = tiles[2].GetTileType();

        if (wildIndex != -1 && wildIndex < payTable.settingPays.Count && payTable.settingPays[wildIndex].isLong)
        {
            if (t1 == wildIndex || t2 == wildIndex)
            {
                tiles[0].SetTileType(RandomType());
                tiles[1].SetTileType(wildIndex);
                tiles[2].SetTileType(RandomType());
            }
        }

        if (bonusIndex != -1 && bonusIndex < payTable.settingPays.Count && payTable.settingPays[bonusIndex].isLong)
        {
            if (t1 == bonusIndex || t2 == bonusIndex)
            {
                tiles[0].SetTileType(RandomType());
                tiles[2].SetTileType(RandomType());
                tiles[1].SetTileType(bonusIndex);
            }
        }

        if (scatterIndex != -1 && scatterIndex < payTable.settingPays.Count && payTable.settingPays[scatterIndex].isLong)
        {
            if (t1 == scatterIndex || t2 == scatterIndex)
            {
                tiles[0].SetTileType(RandomType());
                tiles[2].SetTileType(RandomType());
                tiles[1].SetTileType(scatterIndex);
            }
        }

        if (otherIndex != -1 && otherIndex < payTable.settingPays.Count && payTable.settingPays[otherIndex].isLong)
        {
            if (t1 == otherIndex || t2 == otherIndex)
            {
                tiles[0].SetTileType(RandomType());
                tiles[2].SetTileType(RandomType());
                tiles[1].SetTileType(otherIndex);
            }
        }

        if (tiles[1].GetTileType() == bonusIndex)
            sBonus++;

        if (tiles[1].GetTileType() == wildIndex)
            sWild++;

        if (tiles[1].GetTileType() == otherIndex)
            sOther++;

        if (tiles[1].GetTileType() == scatterIndex)
            sScatter++;

        foreach (var t in tiles)
            result.Add(t);

        int wild = 0, scatter = 0, bonus = 0;
        foreach(var r in result)
        {
            int type = r.GetTileType();

            if(type == wildIndex)
            {
                wild++;
                if (!allowWild)
                {
                    if (wild >= 3)
                        foreach (var t in tiles)
                            if (t.GetTileType() == wildIndex)
                            {
                                t.SetTileType(RandomType());
                                break;
                            }
                }
            }

            if(type == scatterIndex)
            {
                scatter++;
                if (!allow2Scatter && !allowScatter)
                {
                    if(scatter >= 2)
                        foreach (var t in tiles)
                            if (t.GetTileType() == scatterIndex)
                            {
                                t.SetTileType(RandomType());
                                scatter--;
                                break;
                            }
                }

                if (allow2Scatter)
                {
                    if (scatter >= 3)
                        foreach (var t in tiles)
                            if (t.GetTileType() == scatterIndex)
                            {
                                t.SetTileType(RandomType());
                                scatter--;
                                break;
                            }
                }

                if (scatter > sScatter)
                {
                    //if (!allowScatter && scatter >= 3) { }
                    //else
                    int ss = Mathf.Clamp(sScatter, 0, soundScatter.Length - 1);
                    if (soundScatter[ss]) AudioSource.PlayClipAtPoint(soundScatter[ss], Camera.main.transform.position, 1f);
                    sScatter++;
                }
            }

            if (scatter >= 2 && !wassc[column] && column < totalLine)
            {
                wassc[column] = true;
                if(objsc != null && column < objsc.Count)
                    objsc[column].SetActive(false);
                for (int i = column + 1; i < totalLine; i++)
                {
                    wassc[i] = true;
                    if (objsc != null && i < objsc.Count) objsc[i].SetActive(true);
                }
                if (anticipationSound && !anticipationSound.isPlaying && column < totalLine - 1)
                    anticipationSound.Play();
            }

            if (type == bonusIndex)
            {
                bonus++;
                if (!allowBonus)
                {
                    if (bonus >= 3)
                        foreach (var t in tiles)
                            if (t.GetTileType() == bonusIndex)
                            {
                                t.SetTileType(RandomType());
                                break;
                            }
                }
            }
        }

        if (column == 4)
        {
            spinSound.Stop();
            if (anticipationSound) anticipationSound.Stop();
            StartCoroutine(ChangeColumn(result.ToArray()));
        }
    }

    bool keepCheck = false;
    IEnumerator ChangeColumn(Tile[] tiles)
    {
        //if (indexRandom == -1) yield break;
        keepCheck = false;
        yield return new WaitForSeconds(1f);
        if (indexRandom != -1)
        {
            int totalColumn = 0;
            int i = 0;
            while (i < tiles.Length)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (tiles[i + j].GetTileType() == indexRandom)
                    {
                        totalColumn += 1;
                        break;
                    }
                }           
                i += 3;
            }


            if (totalColumn >= GetColumn(indexRandom))
            {
                //Check 3 Scatter
                int ct = 0;
                foreach (var t in tiles)
                    if (t.GetTileType() == scatterIndex)
                        ct++;

                if (ct >= 3)
                {
                    totalFreeSpins += PayData.totalFreeSpins;
                    StartCoroutine(FXRandom());
                    SetFreeSpins(totalFreeSpins);
                    yield return new WaitForSeconds(PayData.timeShowChoise);
                }
                
                //Replace column
                keepCheck = true;
                i = 0;
                while (i < tiles.Length)
                {
                    int c = 0;
                    for (int j = 0; j < 3; j++)
                        if (tiles[i + j].GetTileType() == indexRandom)
                            c += 1;

                    if (c != 0)
                        for (int j = 0; j < 3; j++)
                            if (tiles[i + j].GetTileType() != indexRandom)
                            {
                                tiles[i + j].SetTileType(indexRandom);
                                AudioSource.PlayClipAtPoint(soundRandom[1], Camera.main.transform.position, 1f);
                                yield return new WaitForSeconds(0.25f);
                            }
                    i += 3;
                }
            }
        }

        yield return new WaitForSeconds(0.25f);
        //isInput = true;
        FindMatch(0, 2);
        yield break;
    }

    int GetColumn(int index)
    {
        if (payTable.settingPays.Count > index)
            for (int i = 0; i < payTable.settingPays[index].tylePay.Length; i++)
                if (payTable.settingPays[i].tylePay[i] > 0)
                    return i + 1;
        return 3;
    }

    int RandomType()
    {
        List<int> rs = new List<int>();
        int i = 0;
        foreach (var p in payTable.settingPays)
        {
            if (p.tyleStyle == TyleStyle.Normal)
                rs.Add(i);
            i++;
        }
        int r = Random.Range(0, rs.Count);
        return rs[r];
    }

    List<int> bingoList;
    List<int> typeList;
    List<int> countList;

    Coroutine riskCoroutine, autoSpinCorotine, showChoiseCorotine, scatterChoiseCorotine;
    Tile tileOther;
    void FindMatch(int baseX, int baseY)
    {
        foreach (var tr in disableWhenSpin)
            tr.GetComponent<Button>().interactable = true;

        bingoList = new List<int>();
        typeList = new List<int>();
        countList = new List<int>();
        double totalScore = 0;
        int totalScatter = 0;
        int totalWild = 0;
        int totalBonus = 0;
        int totalOther = 0;
        Vector3 posOther = Vector3.zero;
        bool isJackpot = false;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                Tile tile = lineList[baseX + j].items[baseY + i];
                int type = tile.GetTileType();
                if (type == scatterIndex)
                    totalScatter++;
                if (type == wildIndex)
                    totalWild++;
                if (type == bonusIndex)
                    totalBonus++;
                if (type == otherIndex)
                {
                    totalOther++;
                    posOther = new Vector2(i, j);
                    tileOther = tile;
                }
            }
        }

        for (int i = 0; i < lanes; i++)
        {
            int count = 0;
            int firstType = -1;
            bool isWild = false;

            if (!keepCheck)
            {
                for (int j = 0; j < 5; j++)
                {
                    int pos = PayData.line[i, j];
                    Tile tile = lineList[baseX + j].items[baseY + pos];
                    int type = tile.GetTileType();
                    if (type != wildIndex)
                    {
                        firstType = type;
                        break;
                    }
                    else isWild = true;
                }

                if (firstType < 0 && isWild) firstType = wildIndex;
                isJackpot = isWild;

                for (int j = 1; j < 5; j++)
                {
                    int pos = PayData.line[i, j];
                    Tile tile = lineList[baseX + j].items[baseY + pos];
                    int type = tile.GetTileType();
                    isJackpot &= (type == wildIndex);
                    if (type == scatterIndex || (firstType != type && wildIndex != type)) break;
                    count++;
                }
                if (count > 0)
                {
                    int score = payTable.settingPays[firstType].tylePay[count];
                    if (score > 0)
                    {
                        totalScore += (double)score;
                        bingoList.Add(i);
                        typeList.Add(firstType);
                        countList.Add(count + 1);
                    }
                }
            }
            else
            {
                int[] tiles = new int[5];
                for (int j = 0; j < 5; j++)
                {
                    int pos = PayData.line[i, j];
                    tiles[j] = lineList[baseX + j].items[baseY + pos].GetTileType();
                }

                firstType = indexRandom;
                if (firstType >= 0)
                {
                    if (firstType == wildIndex)
                        isWild = true;

                    isJackpot = isWild;

                    for (int j = 0; j < 5; j++)
                    {
                        int pos = PayData.line[i, j];
                        Tile tile = lineList[baseX + j].items[baseY + pos];
                        int type = tile.GetTileType();
                        isJackpot &= (type == wildIndex);
                        if (type == firstType || type == wildIndex)
                            count++;
                    }
                    if (count > 1)
                    {
                        int score = payTable.settingPays[firstType].tylePay[count - 1];
                        if (score > 0)
                        {
                            totalScore += (double)score;
                            bingoList.Add(i);
                            typeList.Add(firstType);
                            countList.Add(count);
                        }
                    } 
                }
            } 
        }

        //show all choise in free spin and >= 3 column
        if (keepCheck)
        {
            for (int k = 0; k < lanes; k++)
            {
                for (int j = 0; j < 5; j++)
                {
                    int pos = PayData.line[k, j];
                    Tile tile = lineList[baseX + j].items[baseY + pos];
                    int type = tile.GetTileType();
                    if (type == indexRandom || type == wildIndex)
                        tile.ShowChoice(0.5f * (bingoList.Count + 1));
                }
            }
        }

        if (totalFreeSpins < 1)
            SetMoney(money - totalBet);
        else
        {
            if (totalFreeSpins > 0)
            {
                totalFreeSpins--;
                SetFreeSpins(totalFreeSpins);
                if (totalFreeSpins < 1)
                {
                    DisplayFreeSpins(false);
                    showTotalWin = true;
                }
            }
        }

        SetWin(totalScore * bet);
        PlusMoney(totalScore * bet);

        if (bingoList.Count > 0)
        {
            isHighlight = true;

            if (!keepCheck)
            {
                StartCoroutine(DelayAction(1f, () =>
                {
                    foreach (var i in bingoList)
                        DisplayHighlight(0, 2);
                    cashSound.Play();
                    StartCoroutine(DelayAction(PayData.timeShowChoise, () =>
                    {
                        ClearHighlight();
                    }));
                }));
            }
            else
            {
                StartCoroutine(RepeatAction(0.5f, bingoList.Count + 1, () =>
                {
                    DisplayHighlight(0, 2, true);
                }, () =>
                {
                }, () =>
                {
                    ClearHighlight();
                }));
            }
        }

        //float t = PayData.timeShowChoise * (bingoList.Count + 1);
        float t = 1f;
        
        StartCoroutine(DelayAction(1f, () =>
        {
            isInput = true;
        }));

        if (totalScatter > 1)
        {
            if (totalScatter >= 3)
            {
                totalFreeSpins += PayData.totalFreeSpins;
                StartCoroutine(FXRandom());
                SetFreeSpins(totalFreeSpins);     

                if (scatterChoiseCorotine != null) StopCoroutine(scatterChoiseCorotine);
                scatterChoiseCorotine = StartCoroutine(DelayAction(1f, () =>
                {
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < 5; j++)
                        {
                            Tile tile = lineList[baseX + j].items[baseY + i];
                            int type = tile.GetTileType();
                            if (type == scatterIndex)
                                tile.ShowChoice(PayData.timeShowChoise);
                        }
                }));
            }
            if (totalScatter == 2)
            {
                SetWin(totalBet + lastWin);
                PlusMoney(totalBet);
            }
        }

        if (totalOther > 0)
        {
            foreach (var item in PayTable.Instance.settingBonus)
                if (item.tyleStyle == TyleStyle.Other && item.bonusObj)
                {
                    Vector3 pos = listPoint.transform.GetChild((2 - (int)posOther.x) * 5 + (int)posOther.y).transform.position;
                    item.bonusObj.transform.GetChild(0).position = pos;
                    StartCoroutine(FXBooster(item.bonusObj.transform));
                    AudioClip r = SoundBonus(TyleStyle.Other);
                    if (r) AudioSource.PlayClipAtPoint(r, Camera.main.transform.position, 1f);
                }
        }

        //if (butRisk)
        //{
        //    if (totalWild >= 3)
        //        for (int i = 0; i < 3; i++)
        //        {
        //            for (int j = 0; j < 5; j++)
        //            {
        //                Tile tile = lineList[baseX + j].items[baseY + i];
        //                int type = tile.GetTileType();
        //                if (type == wild1Index || type == wild2Index)
        //                    tile.ShowChoice(PayData.timeShowChoise);
        //            }
        //        }
        //    //butRisk.GetComponent<Button>().interactable = totalWild >= 3;
        //    if (totalWild >= 3 && totalBonus < 3)
        //    {
        //        AudioClip c = SoundBonus(TyleStyle.Wild1);
        //        if (c) AudioSource.PlayClipAtPoint(c, Camera.main.transform.position, 1f);
        //        //if (autoSpinCorotine != null) StopCoroutine(autoSpinCorotine);
        //        //riskCoroutine = StartCoroutine(DelayAction(t, () => { AutoSpin(); }));
        //    }
        //}

        if (showChoiseCorotine != null) StopCoroutine(showChoiseCorotine);
        showChoiseCorotine = StartCoroutine(DelayAction(1f, () =>
        {
            if (totalBonus >= 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        Tile tile = lineList[baseX + j].items[baseY + i];
                        int type = tile.GetTileType();
                        //show bonus
                        if (type == bonusIndex)
                            tile.ShowChoice(PayData.timeShowChoise);
                    }
                }

                AudioClip c = SoundBonus(TyleStyle.Bonus);
                if (c) AudioSource.PlayClipAtPoint(c, Camera.main.transform.position, 1f);
                inBonus = true;
                StartCoroutine(DelayAction(2f, () => {
                    foreach (var item in PayTable.Instance.settingBonus)
                        if (item.tyleStyle == TyleStyle.Bonus && item.bonusObj)
                        {
                            StopCoroutine(autoSpinCorotine);
                            mainGame.GetComponent<Animator>().SetTrigger("Hide");
                            switch (AssetBundleController.Instance.bundle.name)
                            {
                                //case "asgard":
                                //    BonusController.Instance.UpdateData(0);
                                //    break;

                                //case "cherry":
                                //    BonusController.Instance.UpdateData(1);
                                //    break;

                                //case "dragon":
                                //    BonusController.Instance.UpdateData(2);
                                //    break;

                                //case "football":
                                //    BonusController.Instance.UpdateData(3);
                                //    break;

                                //case "china2":
                                //    BonusController.Instance.UpdateData(4);
                                //    break;

                                default:
                                    //WheelController.Instance.OpenWheel(2);
                                    BonusController.Instance.UpdateData(0);
                                    break;
                            }
  
                            inBonus = false;
                            break;
                        }
                }));
            }
        }));

        int w = 0;
        if (totalScore > 0 && !isJackpot)
        {
            //cashSound.Play();
            if (lastWin > totalBet)
                if (lastWin  >= 100000000) w = 1;
        }

        if (totalFreeSpins > 0 && !showTotalWin) w = 0;
        if (showTotalWin) w = 2;

        if (w > 0)
        {
            t += 5f;
            showWin = false;
            StartCoroutine(DelayAction(1f, () =>
            {
                int s = 0;
                if (lastWin + totalFreeWin >= 100000000 && lastWin + totalFreeWin < 500000000) s = 0;
                else if (lastWin + totalFreeWin >= 500000000 && lastWin + totalFreeWin <= 1000000000) s = 1;
                else if (lastWin + totalFreeWin > 1000000000) s = 2;
                if (s < bigWinClips.Length)
                    bigWinAnim.transform.GetChild(2).GetComponent<AudioSource>().clip = bigWinClips[s];
                bigWinValue = 0;
                bigWinAnim.SetTrigger("Open");
                DOTween.To(() => bigWinValue, x => bigWinValue = x, bigWinValue + lastWin + totalFreeWin, 4f).OnUpdate(() =>
                {
                    bigWinLabel.text = (bigWinValue).ToString("#,##0");
                }).SetDelay(1f).OnComplete(() => {
                    showWin = true;
                    totalFreeWin = 0;
                    showTotalWin = false;

                    if (autoSpinCorotine != null) StopCoroutine(autoSpinCorotine);
                    autoSpinCorotine = StartCoroutine(DelayAction(1f, () =>
                    {
                        if (!CheckBonus(totalBonus))
                            AutoSpin();
                    }));
                });
            }));
        }

        if (isJackpot)
        {
            if (FairyController.Instance) FairyController.Instance.ChangeStateAtk();
            Animator3D anim = effectWin[0].GetComponent<Animator3D>();
            anim.Duration = PayData.timeShowChoise * 2;// * (bingoList.Count);
            anim.Count = (int)(25 * anim.Duration);
            anim.Run();

            fxImage.GetComponent<GifImage>().gifPath = pathTextWin[4];
            fxImage.SetActive(true);

            StartCoroutine(SendJackpot(0, true));
            t += 2f;
        }


        if (bingoList.Count > 0) t += PayData.timeShowChoise;
        if (totalScatter >= 3)
        {
            t += PayData.timeShowChoise;
            if (totalFreeSpins > 10) t -= PayData.timeShowChoise;
            w = 0;
        }
        if (lastWin == 0 && totalScatter < 3) t = 0f;
        if (w == 0 && totalScatter < 3)
        {
            if (autoSpinCorotine != null) StopCoroutine(autoSpinCorotine);
            autoSpinCorotine = StartCoroutine(DelayAction(t, () =>
            {
                if (!CheckBonus(totalBonus))
                    AutoSpin();
            }));
        }
    }

    public GameObject listPoint;
    public Animator bigWinAnim;
    public Text bigWinLabel;
    double bigWinValue = 0;
    public AudioClip[] bigWinClips;

    int FindCountMax(int[] line)
    {
        for (int i = 0; i < line.Length - 1; i++)
        {
            int c = 0;
            for (int j = i + 1; j < line.Length; j++)
                if (line[i] == line[j]) c++;
            if (c >= 2)
                return line[i];
        }
        return -1;
    }

    AudioClip SoundBonus(TyleStyle style)
    {
        foreach(var item in payTable.settingPays) 
            if(item.tyleStyle == style)
                return item.soundCash;
        return null;
    }

    bool CheckBonus(int bonus)
    {
        if (bonus >= 3)
            foreach (var item in PayTable.Instance.settingBonus)
                if (item.tyleStyle == TyleStyle.Bonus && item.bonusObj)
                    return true;
        return false;
    }

    void AutoSpin()
    {
        StartCoroutine(DelayAction(1f, () =>
        {
            if (inRisk) return;
            if (isAuto || totalFreeSpins > 0)
            {
                isInput = true;
                Spin();
            }
        }));
    }

    public void CloseBonus(TyleStyle style)
    {
        foreach(var item in PayTable.Instance.settingBonus)
            if (item.tyleStyle == style && item.bonusObj)
            {
                //if (item.bonusObj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Hided"))
                //item.bonusObj.GetComponent<Animator>().SetTrigger("Hide");
                mainGame.GetComponent<Animator>().SetTrigger("Show");
                break;
            }
        AutoSpin();
        if (style == TyleStyle.Wild)
        {
            if (riskBackground) riskBackground.SetActive(false);
            inRisk = false;
        }
    }

    public void CloseRank()
    {
        ranking.GetComponent<Animator>().SetTrigger("Hide");
        mainGame.GetComponent<Animator>().SetTrigger("Show");
    }

    int highlightCounter = 0;
    bool isHighlight = false;

    void ClearHighlight(bool isSingle = false)
    {
        isHighlight = false;
        highlightCounter = 0;
        highlightLine.SetVisible(false);
        if (hightlines == null || hightlines.Count == 0) return;
        foreach (var h in hightlines)
            if (h) Destroy(h);
    }

    List<GameObject> hightlines;
    bool wasHearWild = false;
    void DisplayHighlight(int baseX, int baseY, bool isSingle = false)
    {
        if (hightlines == null) hightlines = new List<GameObject>();
        if (!isHighlight) return;
        if (bingoList.Count < 1) return;

        int i = highlightCounter % bingoList.Count;
        {
            int idx = bingoList[i];
            int type = typeList[i];
            int count = countList[i];
            //string str = type + " : " + count + " = ";
            List<Vector3> path = new List<Vector3>();
            for (int ln = 0; ln < 5; ln++)
            {
                int pos = PayData.line[idx, ln];
                Tile choiceTile = lineList[ln + baseX].items[pos + baseY];
                path.Add(choiceTile.transform.position);
            }

            if (!isSingle)
            {
                GameObject temp = Instantiate(highlightLine.gameObject, highlightLine.transform.parent, false);
                hightlines.Add(temp);
                HighlightLine hightline = temp.GetComponent<HighlightLine>();
                hightline.path = path.ToArray();
                hightline.DrawLines();

                for (int ln = 0; ln < count; ln++)
                {
                    int pos = PayData.line[idx, ln];
                    Tile choiceTile = lineList[ln + baseX].items[pos + baseY];
                    if (!wasHearWild && choiceTile.GetTileType() == wildIndex)
                    {
                        AudioClip c = SoundBonus(TyleStyle.Wild);
                        if (c) AudioSource.PlayClipAtPoint(c, Camera.main.transform.position, 1f);
                        wasHearWild = true;
                    }
                    choiceTile.ShowChoice(PayData.timeShowChoise);
                }
            }
            else
            {
                highlightLine.SetVisible(true);
                HighlightLine hightline = highlightLine.GetComponent<HighlightLine>();
                hightline.path = path.ToArray();
                hightline.DrawLines();

                if (payTable.settingPays[type].soundCash)
                {
                    if ((payTable.settingPays[type].soundCash.length > 2f && i == 0) || payTable.settingPays[type].soundCash.length <= 2f)
                        AudioSource.PlayClipAtPoint(payTable.settingPays[type].soundCash, Camera.main.transform.position, 1f);
                }
                else
                    cashSound.Play();
            }
        }
        highlightCounter++;
    }

    void ClearChoice()
    {
        ClearHighlight();
        foreach (Line lScript in lineList)
            foreach (Tile tScript in lScript.items) tScript.UnSetChoice();
    }

    public List<GameObject> disableWhenSpin; 
    int cads = 0;
    void Spin()
    {
        cads--;
        if(cads == 0)
        {
            //if (ADSController.Instance) ADSController.Instance.ShowInterstitial();
            cads = Random.Range(25, 35);
        }
        if (!isInput) return;
        if (scatterChoiseCorotine != null) StopCoroutine(scatterChoiseCorotine);
        if (showChoiseCorotine != null) StopCoroutine(showChoiseCorotine);

        if (money < totalBet) {

            AutoBet();
            if (money < totalBet)
            {
                //AutoLine();
                if (money < totalBet) return;
            }
        }
        
        if (lanes == 0 || money == 0) return;
        foreach (var tr in disableWhenSpin)
            tr.GetComponent<Button>().interactable = false;
        ClearChoice();

        RandomStateResult();

        DoRoll();
        StartCoroutine(SendJackpot(totalBet * 0.1f));
        double exp = totalBet * 0.1f;
        if (timeCounter > 0) exp *= 2;
        LevelUpdate(exp);
        spinSound.Play();

        if (totalSpin != -1)
        {
            totalSpin++;
            PlayerPrefs.SetInt("totalSpin", totalSpin);
            PlayerPrefs.Save();
        }
    }

    public Animator[] specialOffer;
    public AudioClip specialClip;
    public void SpecialOffer(int i)
    {
        specialOffer[i].SetTrigger("Open");
        AudioSource.PlayClipAtPoint(specialClip, Camera.main.transform.position, 1f);
    }

    public Text LevelName, LevelProcess;
    public Image processLevel;
    void LevelUpdate(double exp)
    {
        Experience.Instance.GainExp(exp);
        LevelName.text = Experience.Instance.vLevel.ToString();
        LevelProcess.text = Experience.Instance.vCurrExp + "/" + Experience.Instance.vExpLeft;
        processLevel.fillAmount = (float)Experience.Instance.vCurrExp / (float)Experience.Instance.vExpLeft;
        if(RankingController.Instance) RankingController.Instance.GetData("update");
    }

    public void DoSpin()
    {
        if (isAuto) return;
        Spin();
    }

    public Text labelJackpot;
    double jackpot = 0;
    IEnumerator SendJackpot(double addjp, bool isjp = false)
    {
        string url = "https://appstore.com.ru/SLOTS/PHP/jackpot.php";
        WWWForm form = new WWWForm();
        form.AddField("name", Application.identifier);
        form.AddField("add", addjp.ToString());
        form.AddField("isJackpot", isjp.ToString());
        WWW www = new WWW(url, form);
        while (!www.isDone)
            yield return new WaitForSeconds(0.1f);

        jackpot = 0;
        double.TryParse(www.text, out jackpot);
        if (isjp)
        {
            SetWin(totalBet * 2 + lastWin + jackpot);
            PlusMoney(jackpot);
            jackpot = 0;
        }
        labelJackpot.text = "<color=yellow>JACKPOT:</color> " + jackpot.ToString("#,##0");
    }

    public void ExitGame()
    {
        Application.LoadLevel("Menu");
        AdController.Instance.ShowInterstitial();
    }

    public Animator animIAP;
    public GameObject itemIAP;
    public void LoadIAPList()
    {
        itemIAP.SetActive(false);
        for (int i = 0; i < Purchaser.Instance.infomationProducts.Count; i++)
        {
            var p = Purchaser.Instance.infomationProducts[i];
            if (p.Modification == Modification.Coin && p.ID != "com.slots.credit07" && p.ID != "com.slots.credit08")
            {
                GameObject temp = Instantiate(itemIAP, itemIAP.transform.parent, false);
                temp.GetComponent<ItemIAP>().UpdateData(i);
                temp.SetActive(true);
            }
        }
    }
}
