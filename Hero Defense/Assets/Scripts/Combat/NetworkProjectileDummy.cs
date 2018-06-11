using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class NetworkProjectileDummy : NetworkBehaviour
{

    [SyncVar]
    float speed = 70;

    [SyncVar]
    Transform target;    
    
    [SyncVar]
    Boolean wasInitiated = false;

    public void Init(Transform target, float speed)
    {
        this.target = target;
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
            if (target == null)
            {
                NetworkServer.Destroy(gameObject);
                return;
            }

            Vector3 direction = target.position - transform.position;
            float distanceThisFrame = speed * Time.deltaTime;
            
            transform.Translate(direction.normalized * distanceThisFrame, Space.World);
        }
    }
}
