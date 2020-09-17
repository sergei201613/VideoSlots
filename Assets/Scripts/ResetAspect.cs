using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class ResetAspect : MonoBehaviour
{
    public CameraProjection projection;
    public Vector2 sizeOrigin = new Vector2(800, 480);
    public bool sameWidth = true;
    void Awake()
    {
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            if (projection == CameraProjection.Orthographic)
            {
                if (sameWidth)
                {
                    float height = cam.orthographicSize * 2.0f;                         //chiều cao cơ sở
                    float width = height * sizeOrigin.x / sizeOrigin.y;                                 //chiều rộng cơ sở tính theo màn hình chuẩn
                    cam.orthographicSize = width * Screen.height / Screen.width * 0.5f;
                }
                else
                {
                    float width = cam.orthographicSize * 2.0f;                         //chiều cao cơ sở
                    float height = width * sizeOrigin.y / sizeOrigin.x;                                 //chiều rộng cơ sở tính theo màn hình chuẩn
                    cam.orthographicSize = height * Screen.width / Screen.height * 0.5f;
                }
            }
            else
            {
                cam.fieldOfView = 2 * Mathf.Atan(Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f) / (cam.aspect / 1.4f)) * Mathf.Rad2Deg;  //1.4 là aspect ở tỉ lệ 800:480
            }
        }
    }
}

public enum CameraProjection
{
    Perspective,
    Orthographic
}