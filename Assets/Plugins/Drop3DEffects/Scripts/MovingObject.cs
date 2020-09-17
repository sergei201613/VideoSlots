using UnityEngine;

public class MovingObject : MonoBehaviourBase
{
    public Vector3 Velocity;
    public Vector3 Acceleration;
    public Vector3 Rotation;
    public float Speed;

    public float? RenderDelay;
    private float _renderTime;

    public void Update()
    {
        Transform.Translate(Velocity * Time.deltaTime * Speed, Space.World);
        Transform.Rotate(Rotation * Time.deltaTime * Speed);
        Velocity += Acceleration * Time.deltaTime * Speed;

        if (RenderDelay.HasValue && Time.time >= _renderTime)
        {
            GetComponent<Renderer>().enabled = true;
            RenderDelay = null;
        }
    }

    public void SetRenderDelay(float renderDelay)
    {
        if (renderDelay > 0f)
        {
            GetComponent<Renderer>().enabled = false;
            RenderDelay = renderDelay;
            _renderTime = Time.time + renderDelay;
        }
    }

}
