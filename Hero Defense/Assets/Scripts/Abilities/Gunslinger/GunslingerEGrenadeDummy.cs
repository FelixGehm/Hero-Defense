using UnityEngine;
using UnityEngine.Networking;
using System;

public class GunslingerEGrenadeDummy : NetworkBehaviour
{
    Vector3 startPos, endPos;

    float height, time, speedFaktor, yTreshold;


    public void Init(Vector3 startPos, Vector3 endPos, float height, float speedFaktor, float yTreshold)
    {
        this.startPos = startPos;
        this.endPos = endPos;
        this.height = height;
        this.speedFaktor = speedFaktor;
        this.yTreshold = yTreshold;

        
    }

    void FixedUpdate()
    {
        if (isServer)
        {
            this.enabled = false;
            return;
        }

        try
        {
            transform.position = Parabola(startPos, endPos, height, time * speedFaktor);
            time += Time.deltaTime;

            if (transform.position.y <= yTreshold)
            {
                Destroy(this.gameObject);
            }
        } catch
        {
            // DO NOTHING
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