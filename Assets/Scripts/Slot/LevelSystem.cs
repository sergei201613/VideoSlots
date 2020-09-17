using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSystem : MonoBehaviour
{
    float _CurrentLevelExp = 10;
    int _GeometricProgressionFactor = 3;
    float _NextLevelExp;

    void GetNextLevelExp()
    {
        _NextLevelExp = _CurrentLevelExp * _GeometricProgressionFactor;
    }

    void IncreaseLevel()
    {
        if (_NextLevelExp <= _CurrentLevelExp)
        {
            _CurrentLevelExp = _NextLevelExp;
            GetNextLevelExp();
        }
    }
}
