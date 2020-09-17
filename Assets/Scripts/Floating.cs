using UnityEngine;

public class Floating : MonoBehaviour
{
    float originalY;
    public float floatStrength = 1;
    public float speed = 1f;

    void Start()
    {
        this.originalY = this.transform.position.y;
    }

    void Update()
    {
            transform.position = new Vector3(
                transform.position.x,
                originalY + ((float)Mathf.Sin(Time.time * speed) * floatStrength),
                transform.position.z);
    }
}
