using DG.Tweening;
using UnityEngine;

public class FairyController : MonoBehaviour {
    public Animator[] animator;
    public GameObject efSpell;
    public Vector3 leftPos, rightPos;
    public Vector3 leftRos, rightRos;
    public float speed = 1f;

    [SerializeField]
    AudioClip[] clipaction;

    public static FairyController Instance;
    private void Awake()
    {
        Instance = this;
        //animator = transform.GetComponent<Animator>();
        fairyState = FairyState.OnLeft;
        Invoke("RandomState", Random.Range(5f, 10f));
        HideFxSpell();
    }

    string nextTrigger = "";
    FairyState fairyState;
    void RandomState()
    {
        if (wirzarState)
        {
            float r = Random.value;
            if (r <= 0.1f) //move to left/right
            {
                //switch (fairyState)
                //{
                //    case FairyState.OnLeft: //move to right if on left
                //        AudioSource.PlayClipAtPoint(clipaction[0], Camera.main.transform.position, 1f);
                //        animator[0].SetTrigger("forward");
                //        fairyState = FairyState.FlyToRight;
                //        MoveTo(animator[0].gameObject, rightPos, rightRos, MoveToRightCompleted);
                //        break;

                //    case FairyState.OnRight: //move to left if on right
                //        AudioSource.PlayClipAtPoint(clipaction[0], Camera.main.transform.position, 1f);
                //        animator[0].SetTrigger("forward");
                //        fairyState = FairyState.FlyToLeft;
                //        MoveTo(animator[0].gameObject, leftPos, leftRos, MoveToLeftCompleted);
                //        break;
                //}
            }
            else if (r <= 0.2f) //change trigger atk
                ChangeStateAtk();
            else if(r <= 0.5f)
            {
                if (Random.value <= 0.5f)
                    animator[0].SetTrigger("cheer1");
                else
                    animator[0].SetTrigger("cherr2");
            }
        }
        Invoke("RandomState", Random.Range(5f, 10f));
    }

    public void ChangeStateAtk()
    {
        CancelInvoke("ResetPos");
        Invoke("SoundAtk", 1.5f);
        animator[0].SetTrigger("ohoh");
        Invoke("FXSpell", 1f);
        Invoke("ResetPos", 3f);
    }

    void SoundAtk()
    {
        AudioSource.PlayClipAtPoint(clipaction[1], Camera.main.transform.position, 1f);
    }

    void ResetPos()
    {
        //switch (fairyState)
        //{
        //    case FairyState.OnLeft:
        //        MoveTo(animator[0].gameObject, leftPos, leftRos, isReset: true);
        //        break;

        //    case FairyState.OnRight:
        //        MoveTo(animator[0].gameObject, rightPos, rightRos, isReset: true);
        //        break;
        //}   
    }

    void MoveTo(GameObject obj, Vector3 pos, Vector3 ros, TweenCallback oncpl = null, bool isReset = false)
    {
        if (oncpl == null)
        {
            obj.transform.DOMove(pos, speed).SetEase(Ease.InSine);
            if (!isReset)
                obj.transform.DOLookAt(pos, 1f).SetEase(Ease.Linear);
            else
                obj.transform.DORotate(ros, speed).SetEase(Ease.Linear).SetDelay(speed);
        }
        else
        {
            obj.transform.DOMove(pos, speed).SetEase(Ease.InSine).OnComplete(oncpl);
            if (!isReset)
                obj.transform.DOLookAt(pos, 1f).SetEase(Ease.Linear);
            else
                obj.transform.DORotate(ros, speed).SetEase(Ease.Linear).SetDelay(speed);
        }
    }

    void MoveToRightCompleted()
    {
        animator[0].transform.DORotate(rightRos, 1f).SetEase(Ease.Linear).OnComplete(()=> {
            animator[0].SetTrigger("idle");
        });
        fairyState = FairyState.OnRight;   
    }

    void MoveToLeftCompleted()
    {
        animator[0].transform.DORotate(leftRos, 1f).SetEase(Ease.Linear).OnComplete(() => {
            animator[0].SetTrigger("idle");
        });
        fairyState = FairyState.OnLeft;
    }

    public void FairyMoveOut(bool isOut = true)
    {
        //if (isOut)
        //{
        //   if(fairyState == FairyState.OnRight || fairyState == FairyState.FlyToRight)
        //    {
        //        AudioSource.PlayClipAtPoint(clipaction[0], Camera.main.transform.position, 1f);
        //        animator[0].SetTrigger("forward");
        //        fairyState = FairyState.FlyToRight;
        //        MoveTo(animator[0].gameObject, new Vector3(rightPos.x + 10, rightPos.y, rightPos.z), leftRos, MoveToRightCompleted);
        //    }
        //    else
        //    {
        //        AudioSource.PlayClipAtPoint(clipaction[0], Camera.main.transform.position, 1f);
        //        animator[0].SetTrigger("forward");
        //        fairyState = FairyState.FlyToLeft;
        //        MoveTo(animator[0].gameObject, new Vector3(leftPos.x - 10, leftPos.y, leftPos.z), leftRos, MoveToLeftCompleted);
        //    }
        //}
        //else
        //{
        //    if (fairyState == FairyState.OnRight || fairyState == FairyState.FlyToRight)
        //    {
        //        AudioSource.PlayClipAtPoint(clipaction[0], Camera.main.transform.position, 1f);
        //        animator[0].SetTrigger("forward");
        //        fairyState = FairyState.FlyToRight;
        //        MoveTo(animator[0].gameObject, rightPos, rightRos, MoveToRightCompleted);
        //    }
        //    else
        //    {
        //        AudioSource.PlayClipAtPoint(clipaction[0], Camera.main.transform.position, 1f);
        //        animator[0].SetTrigger("forward");
        //        fairyState = FairyState.FlyToLeft;
        //        MoveTo(animator[0].gameObject, leftPos, leftRos, MoveToLeftCompleted);
        //    }
        //}
    }

    bool wirzarState = true;
    public void WirzadMoveOut(bool isOut = true)
    {
        wirzarState = isOut;
        if (isOut)
            this.animator[1].transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InSine);
        else
            this.animator[1].transform.DOScale(Vector3.one, speed).SetEase(Ease.InOutBounce);
        FairyMoveOut(!isOut);
        FXSpell();
    }

    void FXSpell()
    {
        if(efSpell)
        efSpell.SetActive(true);
        Invoke("HideFxSpell", 7f);
    }

    public void WirzadCastSpell()
    {
        if (animator.Length == 1) return;
        animator[1].SetTrigger("cheer1");
    }

    void HideFxSpell()
    {
        if(efSpell)
        efSpell.SetActive(false);
    }
}

enum FairyState
{
    OnLeft,
    OnRight,
    FlyToLeft,
    FlyToRight
}
