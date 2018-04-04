using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

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

    void HitTarget()
    {
        //TODO: instantiate particles       ???
        //TODO: substract hitpoints         ???
        Damage(target.GetComponent<CharacterStats>());
        Destroy(gameObject);

    }

    void Damage(CharacterStats targetStats)
    {
        targetStats.TakePhysicalDamage(damage);
    }

    public void SetDamage(float _damage)
    {
        damage = _damage;
    }
}
