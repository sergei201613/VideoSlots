using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Pixelation : MonoBehaviour {
    public Camera cam;
    public VideoPlayer player;
    [Range(1, 10)]
    public float scale = 1;

    void Start()
    {
        UpdateLayout();
    }

    void OnRectTransformDimensionsChange()
    {
        UpdateLayout();
    }

    public void UpdateLayout()
    {
        if (cam)
        {
            if (cam.targetTexture != null)
                cam.targetTexture.Release();
            var v = GetComponent<RawImage>().rectTransform.rect;
            if (Mathf.RoundToInt(v.width) <= 0 || Mathf.RoundToInt(v.height) <= 0) return;
            cam.targetTexture = new RenderTexture(Mathf.RoundToInt(v.width * scale), Mathf.RoundToInt(v.height * scale), 24);
            GetComponent<RawImage>().texture = cam.targetTexture;
        }else if (player)
        {
            if (player.targetTexture != null)
                player.targetTexture.Release();
            var v = GetComponent<RawImage>().rectTransform.rect;
            if (Mathf.RoundToInt(v.width) <= 0 || Mathf.RoundToInt(v.height) <= 0) return;
            player.targetTexture = new RenderTexture(Mathf.RoundToInt(v.width * scale), Mathf.RoundToInt(v.height * scale), 24);
            GetComponent<RawImage>().texture = player.targetTexture;
        }
    }
    
}
