using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoldEGG : MonoBehaviour {
    public static GoldEGG Instance;
    public Sprite[] sprites;
    public AudioClip[] audioClips;
    public Image egg;
    public Text textCount;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GetComponent<Image>().enabled = false;
    }

    public void OpenEGG(int count = 0)
    {
        GetComponent<Image>().enabled = true;
        eggCount = count;
        textCount.text = (eggCount).ToString("#,##0");
        transform.GetChild(0).GetComponent<Animator>().SetTrigger("Open");
    }

    int eggCount = 10;
    public void EggClick()
    {
        egg.GetComponent<Button>().interactable = false;
        if (currentSP < sprites.Length - 2)
        {
            if (Random.value <= 0.5f || currentSP == 1)
            {
                StartCoroutine(DelayAction(Random.Range(0.25f, 0.5f), () =>
                {
                    egg.GetComponent<Button>().interactable = true;
                    currentSP++;
                    ChangeSprite(currentSP);
                }));
            }
            else
            {
                StartCoroutine(DelayAction(Random.Range(0.25f, 0.5f), () =>
                {
                    currentSP++;
                    ChangeSprite(currentSP);
                    StartCoroutine(DelayAction(Random.Range(0.5f, 0.75f), () =>
                    {
                        egg.GetComponent<Button>().interactable = true;
                        currentSP++;
                        ChangeSprite(currentSP);
                    }));
                }));
            }
        }
        else
        {
            currentSP++;
            ChangeSprite(currentSP);
            int r = Random.Range(50, 300) * 1000000;
            egg.transform.GetChild(0).GetComponent<Text>().text = r.ToString("#,##0");
            egg.transform.GetChild(0).GetComponent<Animator>().SetTrigger("Open");

            StartCoroutine(DelayAction(2f, () => {
                animEGG.SetTrigger("Close");
                egg.transform.GetChild(0).GetComponent<Animator>().SetTrigger("Close");
                MenuManager.Instance.PlusMoney(r);

                if(eggCount <= 0)
                {
                    transform.GetChild(0).GetComponent<Animator>().SetTrigger("Close");
                    StartCoroutine(DelayAction(1f, () => {
                        GetComponent<Image>().enabled = false;
                    }));
                }
            }));
        }
    }

    void ChangeSprite(int index)
    {
        egg.sprite = sprites[index];
        if (index < audioClips.Length && audioClips[index])
            AudioSource.PlayClipAtPoint(audioClips[index], Camera.main.transform.position, 1f);
    }

    int currentSP = 0;
    public Animator animEGG;
    public void StartEGG()
    {
        egg.GetComponent<Button>().interactable = true;
        currentSP = 0;
        egg.sprite = sprites[currentSP];
        animEGG.SetTrigger("Open");
        eggCount--;
        textCount.text = (eggCount).ToString("#,##0");
        textCount.transform.parent.gameObject.SetActive(eggCount > 0);
    }

    IEnumerator DelayAction(float dTime, System.Action callback)
    {
        yield return new WaitForSeconds(dTime);
        callback();
    }
}
