using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightFlash : MonoBehaviour {
    public Transform lightsParent;

    int _selectReward, _coins, count = 0, cost = 300;
    AudioSource[] audSource;
    WheelPart[] wheelParts;
    DotLight[] lightObjs;
    public Sprite[] dots = new Sprite[2];
    public int lightCount { get { return lightObjs.Length; } }
  

    void Awake()
    {
        lightObjs = lightsParent.GetComponentsInChildren<DotLight>();
        RandomAnimate();
    }

    void RandomAnimate()
    {
        StopAllCoroutines();
        AnimateWheel((Random.value <= 0.5f));
        Invoke("RandomAnimate", Random.Range(5f, 10f));
    }
    
    public void AnimateWheel(bool playAnim)
    {
        StopAllCoroutines();
        foreach (var item in lightObjs)
        {
            if (item.spRend) item.spRend.sprite = dots[0];
        }

        if (playAnim)
        {
            StartCoroutine(PlayAnimationWhenStationary(dots[0], dots[1]));
        }
        else
        {
            StartCoroutine(LightAnimDuringSpinning(0));
            StartCoroutine(LightAnimDuringSpinning(lightObjs.Length / 3));
             StartCoroutine(LightAnimDuringSpinning(lightObjs.Length * 2 /  3));
            //StartCoroutine(LightAnimDuringSpinning(Random.Range(0, lightObjs.Length)));
        }
    }

    IEnumerator PlayAnimationWhenStationary(Sprite sp1, Sprite sp2)
    {
        yield return new WaitForSeconds(0.2f);
        count++;
        for (int i = 0; i < lightObjs.Length; i++)
        {
            lightObjs[i].spRend.sprite = (i % 2 == 0) ? sp1 : sp2;
        }
        if (count < Random.Range(10, 30))
        {
            StartCoroutine(PlayAnimationWhenStationary(sp2, sp1));
        }
        else
        {
            StartCoroutine(SymetricLightMovement(0));
        }
    }
    IEnumerator LightAnimDuringSpinning(int index)
    {
        yield return new WaitForSeconds(0.05f);
        if (index < lightObjs.Length - 1)
        {
            lightObjs[index].spRend.sprite = dots[1];
            lightObjs[index + 1].spRend.sprite = dots[1];
            yield return new WaitForSeconds(0.1f);
            lightObjs[index].spRend.sprite = dots[0];
            lightObjs[index + 1].spRend.sprite = dots[0];
            StartCoroutine(LightAnimDuringSpinning(index + 2));
        }
        else
        {
            StartCoroutine(LightAnimDuringSpinning(0));
        }
    }
    IEnumerator SymetricLightMovement(int index)
    {
        if (index >= lightObjs.Length)
        {
            count = 0;
            StartCoroutine(PlayAnimationWhenStationary(dots[0], dots[1]));
        }
        else
        {
            lightObjs[index].spRend.sprite = dots[1];
            yield return new WaitForSeconds(0.05f);
            lightObjs[index].spRend.sprite = dots[0];
            yield return new WaitForSeconds(0.0f);
            StartCoroutine(SymetricLightMovement(index + 1));
        }
    }
}
