using UnityEngine;
using UnityEngine.Networking;
using System;

public class GunslingerEGrenadeDummy : NetworkBehaviour
{
    [SyncVar]
    Vector3 startPos;

    [SyncVar]
    Vector3 endPos;

    [SyncVar]
    float height;

    [SyncVar]
    float speedFaktor;

    float time = 0;

    [SyncVar]
    Boolean wasInitiated = false;

    public void Init(Vector3 startPos, Vector3 endPos, float height, float speedFaktor)
    {
        this.startPos = startPos;
        this.endPos = endPos;
        this.height = height;
        this.speedFaktor = speedFaktor;

        wasInitiated = true;
    }

    void FixedUpdate()
    {
        if (isServer)
        {
            this.enabled = false;
            return;
        }

        if(wasInitiated)
        {
            transform.position = Parabola(startPos, endPos, height, time * speedFaktor);
            time += Time.deltaTime;  
        }      
    }


    // found at: https://gist.github.com/ditzel/68be36987d8e7c83d48f497294c66e08
    public static Vector3 Parabola(Vector3 start, Vector3 end, float height, float t)
    {
        Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

        var mid = Vector3.Lerp(start, end, t);

        return new Vector3(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t), mid.z);
    }
}