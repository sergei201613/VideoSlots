using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimaCtrl : MonoBehaviour {

    public Animator DragonAnimator;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Btn(string Act)
    {
        DragonAnimator.SetTrigger(Act);
    }
}
