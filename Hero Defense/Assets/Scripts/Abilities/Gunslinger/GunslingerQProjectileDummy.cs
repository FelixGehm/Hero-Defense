using UnityEngine;
using UnityEngine.Networking;
using System;

public class GunslingerQProjectileDummy : NetworkBehaviour
{
    [SyncVar]
    float speed;

    [SyncVar]
    Vector3 direction;

    [SyncVar]
    Boolean wasInitiated = false;

    public void Init(Vector3 direction, float speed)
    {
        this.direction = direction;
        this.speed = speed;

        wasInitiated = true;
    }

    void FixedUpdate()
    {
        if (isServer)
        {
            this.enabled = false;
            return;
        }

        if (wasInitiated)
        {
            float distanceThisFrame = speed * Time.deltaTime;
            transform.Translate(direction.normalized * distanceThisFrame, Space.World);
        }
    }

}
