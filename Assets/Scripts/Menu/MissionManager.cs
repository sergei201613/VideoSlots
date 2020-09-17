using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour {
    public Mission[] missions;
    public static MissionManager Instance;

    private void Awake()
    {
        if(Instance)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public void CheckMission()
    {

    }


}

[System.Serializable]
public class Mission
{
    public string Name;
    public bool State;
    public int SpinRequire;
    public int Reward;
}