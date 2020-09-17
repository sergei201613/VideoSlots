using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class WheelController : MonoBehaviour {
    public static WheelController Instance;
    public List<WheelReward> wheelRewards;

    private void Awake()
    {
        Instance = this;
        for (int i = 0;  i< wheelRewards.Count; i++)
        {
            GameObject temp;
            if (i == 0)
                temp = itemReward;
            else
                temp = Instantiate(itemReward, itemReward.transform.parent, false);
            temp.transform.GetChild(0).GetComponent<Text>().text = wheelRewards[i].Name;
            if (wheelRewards[i].Reward > 1000000)
                temp.transform.GetChild(0).GetComponent<Text>().color = Color.yellow;
            temp.SetActive(true);
        }
    }

    public Animator animator;
    public Transform wheelObj;
    public GameObject itemReward;
    public Button spinBut;
    public AudioSource audioSource, fxSouce;

    public void Wheel()
    {
        animator.enabled = false;
        fxSouce.Play();
        int random = Random.Range(0, wheelRewards.Count);
        float r = Random.value;
        if (r <= 0.05f) //>1M
        {

        }else if(r<=0.25f) //>=500k
        {
            while (wheelRewards[random].Reward > 1000000)
                random = Random.Range(0, wheelRewards.Count);
        }
        else
        {
            while (wheelRewards[random].Reward > 500000)
                random = Random.Range(0, wheelRewards.Count);
        }

        spinBut.interactable = false;
        float lastRosZ = -random * (360 / wheelRewards.Count) + Random.Range(-10f, 10f);
        wheelObj.DORotate(new Vector3(0f, 0f, lastRosZ + Random.Range(4, 7) * 360), Random.Range(3f, 5f), RotateMode.FastBeyond360).OnComplete(() =>
        {
            if (SlotMachine.Instance && SlotMachine.Instance.gameObject)
                SlotMachine.Instance.PlusMoney((ulong)wheelRewards[random].Reward);
            else
                MenuManager.Instance.PlusMoney((ulong)wheelRewards[random].Reward);

            if (SlotMachine.Instance && SlotMachine.Instance.gameObject)
                SlotMachine.Instance.SetWin(wheelRewards[random].Reward, 0);

            fxSouce.Stop();
            StartCoroutine(DelayAction(2f, () =>
            {
                spinBut.interactable = true;
                if (FairyController.Instance) FairyController.Instance.WirzadMoveOut(true);
                animator.enabled = true;
            if (SlotMachine.Instance && SlotMachine.Instance.gameObject)
                    SlotMachine.Instance.CloseBonus(TyleStyle.Bonus);
            else
                    animator.SetTrigger("Hide");
                DOTween.To(() => volume, x => volume = x, 0f, 2f).OnUpdate(() => {
                    audioSource.volume = volume;
                }).SetDelay(1f).OnComplete(() => {
                    audioSource.Stop();
                    audioSource.volume = 1f;
                    volume = 1f;
                });
                
            }));
        });
    }

    float volume = 1f;

    public void OpenWheel(int st)
    {
        animator.enabled = true;
        if (st == 2)
            audioSource.Play();
        animator.SetTrigger("Open-Step" + st);
    }

    IEnumerator DelayAction(float dTime, System.Action callback)
    {
        yield return new WaitForSeconds(dTime);
        callback();
    }
}

[System.Serializable]
public class WheelReward
{
    public string Name;
    public int Reward;
}
