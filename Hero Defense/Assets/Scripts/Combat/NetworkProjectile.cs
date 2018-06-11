using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkProjectile : NetworkBehaviour {

    public float speed = 70;

    private Transform target;
    private float damage = 0;

    private Transform sender;
    
	void Update () {
        if (!isServer)
        {
            this.enabled = false;
            return;
        }

        if (target == null)
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

    public void InitBullet(Transform target, float _damage, Transform _sender)
    {
        this.target = target;
        damage = _damage;
        sender = _sender;

        NetworkProjectileDummy dummy = GetComponent<NetworkProjectileDummy>();
        dummy.Init(target, speed);
    }

    public void InitBullet(Transform target, float _damage)
    {
        this.target = target;
        damage = _damage;

        NetworkProjectileDummy dummy = GetComponent<NetworkProjectileDummy>();
        dummy.Init(target, speed);
    }

    [Server]
    void HitTarget()
    {
        if(sender != null)
        {
            Damage(target.GetComponent<EnemyStats>());
        } else
        {
            Damage(target.GetComponent<CharacterStats>());
        }
        

        NetworkServer.Destroy(gameObject);
    }

    [Server]
    void Damage(CharacterStats targetStats)
    {
        targetStats.TakePhysicalDamage(damage);        
    }

    [Server]
    void Damage(EnemyStats targetStats)
    {
        targetStats.TakePhysicalDamage(damage,sender);
    }
}
