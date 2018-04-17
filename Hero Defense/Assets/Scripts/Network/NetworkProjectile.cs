﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkProjectile : NetworkBehaviour {

    public float speed = 70;

    private Transform target;
    private float damage = 0;
    
    [Server]
	void Update () {
		if(target == null)
        {
            NetworkServer.Destroy(gameObject);
            return;
        }

        Vector3 direction = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (direction.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(direction.normalized * distanceThisFrame, Space.World);
    }

    [Server]
    public void InitBullet(Transform target, float _damage)
    {
        this.target = target;
        damage = _damage;
    }

    [Server]
    void HitTarget()
    {
        Damage(target.GetComponent<CharacterStats>());

        NetworkServer.Destroy(gameObject);
    }

    [Server]
    void Damage(CharacterStats targetStats)
    {
        targetStats.TakePhysicalDamage(damage);        
    }
}
