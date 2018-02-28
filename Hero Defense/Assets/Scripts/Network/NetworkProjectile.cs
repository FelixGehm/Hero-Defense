using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkProjectile : NetworkBehaviour {

    public float speed = 70;

    private Transform target;
    private float damage = 0;

	void Update () {
		if(target == null)
        {
            Destroy(gameObject);
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

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    public void SetTarget(NetworkInstanceId targetId)
    {
        Transform target = NetworkServer.FindLocalObject(targetId).transform;
        this.target = target;
    }

    void HitTarget()
    {
        Damage(target.GetComponent<NetworkCharacterStats>());
        
        Destroy(gameObject);
    }

    void Damage(NetworkCharacterStats targetStats)
    {
        targetStats.TakeDamage(damage);
    }

    public void SetDamage(float _damage)
    {
        damage = _damage;
    }
}
