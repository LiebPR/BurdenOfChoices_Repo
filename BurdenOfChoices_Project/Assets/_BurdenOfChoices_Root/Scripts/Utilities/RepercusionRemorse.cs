using System;
using UnityEngine;

public enum RemorseLevel
{
    Regular,
    Mid,
    Max
}

public class RepercusionRemorse : MonoBehaviour
{

    [SerializeField] Remorse remorse;
    
    public RemorseLevel GetRemorseLevel()
    {
        int value = remorse.CurrentRemorse;

        if (value <= 1)
            return RemorseLevel.Regular;
        else if (value <= 3)
            return RemorseLevel.Mid;
        else
            return RemorseLevel.Max;
    }
}
