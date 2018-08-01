using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public enum Axis
    {
        X, Y, Z
    }
    public Axis axis;
    public float speed;
    // Update is called once per frame

    void Update()
    {
        switch (axis)
        {
            case Axis.X:
                gameObject.transform.Rotate(new Vector3(speed * Time.deltaTime, 0, 0));
                break;
            case Axis.Y:
                gameObject.transform.Rotate(new Vector3(0, speed * Time.deltaTime, 0));
                break;
            case Axis.Z:
                gameObject.transform.Rotate(new Vector3(0, 0, speed * Time.deltaTime));
                break;
        }

    }
}
