using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadResources : MonoBehaviour {
    public string[] resourcePath;

    private void Awake()
    {
        foreach (var p in resourcePath)
        {
            Sprite[] sps = Resources.LoadAll<Sprite>(p);
        }

        //Application.LoadLevel("Game");
    }

}
