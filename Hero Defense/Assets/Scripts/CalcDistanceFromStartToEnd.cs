using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalcDistanceFromStartToEnd : MonoBehaviour{


    public Transform startPoint;
    public Transform endPoint;

    public float GetDistance()
    {
        return Vector3.Distance(startPoint.position, endPoint.position);
    }

    public Vector3 GetStartPos()
    {
        return startPoint.position;
    }
}
