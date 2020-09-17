using System.Collections.Generic;
using UnityEngine;

public class Animator3D : MonoBehaviourBase
{
    private int _launchedCount;
    private bool _isRunning;
    private float _delay;
    private float _startTime;

    public string Name;
    public Transform ObjectPrefab;
    public Vector3 SpawnEllipsoid;
    public int Count;
    public float Duration;
    public Vector3 MinVelocity;
    public Vector3 MaxVelocity;
    public Vector3 Acceleration;
    public float StartSpeed;
    public Vector3 MinStartRotation;
    public Vector3 MaxStartRotation;
    public Vector3 MinRotation;
    public Vector3 MaxRotation;
    public float LifeTime;
    public float RenderDelay;

    public void Awake()
    {
        _animationsMap[Name] = this;
    }

    public bool runOnStart = false;
    private void OnEnable()
    {
        if (runOnStart)
            Run();
    }

    public void Run()
    {
        _isRunning = true;
        _launchedCount = 0;
        _delay = Duration / Count;
        _startTime = Time.time;
    }

    public void Update()
    {
        if (_isRunning)
        {
            int nextCount = (int)((Time.time - _startTime) / _delay) - _launchedCount;
            for (int i = 0; i <= nextCount; i++)
            {
                Launch();
                _launchedCount++;
                if (_launchedCount == Count)
                {
                    _isRunning = false;
                    break;
                }
            }
        }
    }

    private void Launch()
    {
        Transform spawnedObject = Spawn();
        Emit(spawnedObject);
    }

    private Transform Spawn()
    {
        Transform spawnedObject = Instantiate<Transform>(ObjectPrefab);
        spawnedObject.transform.parent = Transform;
        Vector3 spawnPoint = Random.insideUnitSphere;
        spawnPoint.Scale(SpawnEllipsoid);
        spawnedObject.transform.localPosition = spawnPoint;
        return spawnedObject;
    }

    private void Emit(Transform spawnedObject)
    {
        spawnedObject.transform.Rotate(Range(MinStartRotation, MaxStartRotation));
        MovingObject movingObject = spawnedObject.gameObject.AddComponent<MovingObject>();
        movingObject.Velocity = Range(MinVelocity, MaxVelocity);
        movingObject.Speed = StartSpeed;
        movingObject.Acceleration = Acceleration;
        movingObject.Rotation = Range(MinRotation, MaxRotation);
        movingObject.SetRenderDelay(RenderDelay);
        Destroy(movingObject.gameObject, LifeTime);
    }

    private Vector3 Range(Vector3 min, Vector3 max)
    {
        return new Vector3(
            Random.Range(min.x, max.x),
            Random.Range(min.y, max.y),
            Random.Range(min.z, max.z));
    }

    #region Static

    private static Dictionary<string, Animator3D> _animationsMap;

    static Animator3D()
    {
        _animationsMap = new Dictionary<string, Animator3D>();
    }

    public static void Run(string name)
    {
        if (_animationsMap.ContainsKey(name))
            _animationsMap[name].Run();
        else
            Debug.Log(string.Format("Can't find animation with name: {0}", name));
    }

    #endregion

}