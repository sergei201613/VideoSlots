using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CoinFly : MonoBehaviour {
    //public int maxCoin = 100;
    public float inTime = 5f;
    public GameObject objFly;
    public Transform startPos, endPos;
    public Vector3 scale = Vector3.one;
    public AudioClip sound;

    public void UpdatePos(Vector3 start, Vector3 end)
    {
        //this.startPos = start;
        //this.endPos = end;
        RunAnim();
    }

    public void RunAnim(float intime = 5f)
    {
        this.inTime = intime;
        counttime = 0f;
        //Invoke("SpawObj", 0.1f);
        SpawObj();
        if (sound) AudioSource.PlayClipAtPoint(sound, Camera.main.transform.position, 1f);

    }

    float counttime = 0f;
    void SpawObj()
    {
        counttime += 0.05f;
        GameObject temp = Instantiate(objFly, transform, false);
        temp.transform.localScale = scale;
        temp.transform.position = new Vector3(startPos.position.x + Random.Range(-10f, 10f), startPos.position.y + Random.Range(-10f, 10f), startPos.position.z);
        float t = Random.Range(0.5f, inTime / 2);
        temp.transform.DOMove(endPos.position, t).OnComplete(() => {
            Destroy(temp);
        });

        temp.transform.DORotate(Vector3.one * 720, t, RotateMode.FastBeyond360);

        if (counttime <= inTime)
            Invoke("SpawObj", 0.05f);

    }
}
