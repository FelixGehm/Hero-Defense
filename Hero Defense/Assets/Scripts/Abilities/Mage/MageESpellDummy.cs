﻿using System;
using UnityEngine;
using UnityEngine.Networking;

public class MageESpellDummy : NetworkBehaviour {

    [SyncVar]
    float speed;

    [SyncVar]
    GameObject target;

    [SyncVar]
    Boolean wasInitiated = false;

    public void Init(float speed, GameObject target)
    {
        this.speed = speed;
        this.target = target;
        wasInitiated = true;
    }

    void Update () {
        if (isServer)
        {
            this.enabled = false;
            return;
        }

        if (wasInitiated)
        {
            Vector3 dir = (target.transform.position - transform.position).normalized;
            transform.Translate(dir * speed * Time.deltaTime, Space.World);
        }
    }

    public void SetTarget(GameObject target)
    {
        this.target = target;
    }
}
