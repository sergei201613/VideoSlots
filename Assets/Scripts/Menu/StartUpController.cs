using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartUpController : MonoBehaviour {
    public static StartUpController Instance;
    private void Awake()
    {
        Instance = this;
    }

    public Transform[] daysRewards;
    private void Start()
    {
        for (int i = 0; i < daysRewards.Length; i++)
            if (i < reward.Length)
            {
                daysRewards[i].GetChild(1).GetComponent<Text>().text = reward[i].Name;
                if (i < 4)
                    daysRewards[i].GetChild(3).GetComponent<Text>().text = "DAY " + (i + 1);
                else
                    daysRewards[i].GetChild(3).GetComponent<Text>().text = "DAY " + (i + 1) + "+";
            }

        RewardByDay();
    }

    public WheelReward[] reward;
    public Animator animator;
    public void RewardByDay()
    {
        int day = PlayerPrefs.GetInt("Day", 0);
        int lastDay = PlayerPrefs.GetInt("lastDay");
        int nowDay = System.DateTime.UtcNow.Day;
        if (day == 0 || Mathf.Abs(nowDay - lastDay) == 1)
        {
            day++;
            animator.SetTrigger("Open");
            if (day > 5)
            {
                WheelController.Instance.OpenWheel(1);
            }
        }
        else if (nowDay - lastDay > 1)
            day = 1;

        day = Mathf.Clamp(day, 1, 5);
        rw = reward[day - 1].Reward;
        daysRewards[day - 1].GetComponent<Toggle>().isOn = true;

        day++;
        day = Mathf.Clamp(day, 1, 5);
        Experience.Instance.SendNotification(new string[] { "Collect your daily " + reward[day - 1].Reward.ToString("#,##0") + " reward, don't miss out on the chance to hit the jackpot!", "Become a billionaire with us." });

    }

    int rw = 0;
    public void Collect()
    {
        MenuManager.Instance.PlusMoney(rw);
        animator.SetTrigger("Close");
        UpdateDate();
    }

    void UpdateDate()
    {
        int day = PlayerPrefs.GetInt("Day", 0);
        int lastDay = PlayerPrefs.GetInt("lastDay");
        int nowDay = System.DateTime.UtcNow.Day;
        if (day == 0 || Mathf.Abs(nowDay - lastDay) == 1)
            day++;
        else if (nowDay - lastDay > 1)
            day = 1;

        day = Mathf.Clamp(day, 1, 5);

        PlayerPrefs.SetInt("Day", day);
        PlayerPrefs.SetInt("lastDay", nowDay);
        PlayerPrefs.Save();
    }
}
