using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BonusController : MonoBehaviour {
    public Sprite[] bonus;
    public GameObject[] itemBonus;
    public AudioClip[] clipTreasure;

    int count = 0;

    public static BonusController Instance;
    private void Awake()
    {
        Instance = this;
        //UpdateData();
    }

    public Text counter;
    public bool UpdateData(int type = 0)
    {
        counter.text = (5).ToString();
        GetComponent<Animator>().SetTrigger("Open");
        count = 0;
        totalBonus = 0;
        totalOpen = 0; minor = 0; major = 0; grand = 0;
        foreach (var item in itemBonus)
        {
            item.transform.GetChild(1).transform.localScale = Vector3.zero;
            Button b = item.GetComponent<Button>();
            b.transform.GetChild(2).GetChild(0).GetComponent<Animator>().SetTrigger("Close");
            b.interactable = true;
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() =>
            {   
                b.interactable = false;
                if (totalOpen >= 5) return;
                b.transform.GetChild(2).GetChild(0).GetComponent<Animator>().SetTrigger("Open");
                totalOpen++;
                counter.text = (5 - totalOpen).ToString();
                float r = Random.value;
                if (r >= 0.4f)
                {
                    AudioSource.PlayClipAtPoint(clipTreasure[1], Camera.main.transform.position, 1f);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(clipTreasure[0], Camera.main.transform.position, 1f);
                    if (r <= 0.25f)
                    {
                        item.transform.GetChild(1).GetComponent<Image>().sprite = bonus[0];
                        minor++;
                    }
                    else if (r <= 0.4f)
                    {
                        item.transform.GetChild(1).GetComponent<Image>().sprite = bonus[1];
                        major++;
                    }
                    //else
                    //{
                    //    item.transform.GetChild(1).GetComponent<Image>().sprite = bonus[2];
                    //    grand++;
                    //}

                    item.transform.GetChild(1).DOScale(1f, 0.5f);
                }
                CheckWin();
            });
        }
        //if (SlotMachine.Instance.background.Length > 0)
        //    GetComponent<Image>().sprite = SlotMachine.Instance.background[0];
        return true;
    }

    int totalOpen = 0;
    int minor, major, grand;
    public Animator animator;
    public Image jackpot;
    public Text reward;
    public Button butCollect;
    void CheckWin()
    {
        double rew = 0;
        if (minor >= 3)
        {
            jackpot.sprite = bonus[0];
            rew = SlotMachine.Instance.totalBet * 10;
        }

        else if (major >= 3)
        {
            jackpot.sprite = bonus[1];
            rew = SlotMachine.Instance.totalBet * 25;
        }

        else if (grand >= 3)
        {
            jackpot.sprite = bonus[2];
            rew = SlotMachine.Instance.totalBet * 250;
        }

        if(rew > 0)
        {
            AudioSource.PlayClipAtPoint(clipTreasure[2], Camera.main.transform.position, 1f);
            reward.text = rew.ToString("#,##0");
            animator.SetTrigger("Open");
            butCollect.onClick.RemoveAllListeners();
            butCollect.onClick.AddListener(() =>
            {
                animator.SetTrigger("Close");
                GetComponent<Animator>().SetTrigger("Close");
                SlotMachine.Instance.PlusMoney(rew);
                if (SlotMachine.Instance && SlotMachine.Instance.gameObject)
                    SlotMachine.Instance.CloseBonus(TyleStyle.Bonus);
            });
        }else if(totalOpen == 5)
        {
            AudioSource.PlayClipAtPoint(clipTreasure[1], Camera.main.transform.position, 1f);
            GetComponent<Animator>().SetTrigger("Close");
            if (SlotMachine.Instance && SlotMachine.Instance.gameObject)
                SlotMachine.Instance.CloseBonus(TyleStyle.Bonus);
        }
        
    }

    double totalBonus = 0;
    public void OnSelected(GameObject obj, int bonus)
    {
        if (count < 0) return;

        if (clipTreasure.Length > 0)
        {
            if (bonus > 0)
                AudioSource.PlayClipAtPoint(clipTreasure[0], Camera.main.transform.position, 1f);
            else
                AudioSource.PlayClipAtPoint(clipTreasure[1], Camera.main.transform.position, 1f);
        }

        if (bonus > 0 && count > 0)
        {
            obj.GetComponent<Button>().interactable = false;
            obj.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = bonus.ToString();
            obj.transform.GetChild(0).gameObject.SetActive(false);
            obj.transform.GetChild(1).gameObject.SetActive(true);
            SlotMachine.Instance.PlusMoney((ulong)bonus);
            totalBonus += bonus;
            count--;

            if (count <= 0)
            {
                SlotMachine.Instance.SetWin(totalBonus, 0);
                StartCoroutine(DelayAction(3f, () => {
                    if (FairyController.Instance) FairyController.Instance.WirzadMoveOut(true);
                    SlotMachine.Instance.CloseBonus(TyleStyle.Bonus);
                }));
            }
        }else
        {
            count = -1;
            obj.GetComponent<Button>().interactable = false;
            obj.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = bonus.ToString();
            obj.transform.GetChild(0).gameObject.SetActive(false);
            obj.transform.GetChild(1).gameObject.SetActive(true);
            if (FairyController.Instance)
            {
                FairyController.Instance.ChangeStateAtk();
                FairyController.Instance.WirzadCastSpell();
            }

            StartCoroutine(DelayAction(3f, () => {
                SlotMachine.Instance.SetWin(totalBonus, 0);
                if (FairyController.Instance) FairyController.Instance.WirzadMoveOut(true);
                SlotMachine.Instance.CloseBonus(TyleStyle.Bonus);
            }));
        }
    }

    IEnumerator DelayAction(float dTime, System.Action callback)
    {
        yield return new WaitForSeconds(dTime);
        callback();
    }
}
