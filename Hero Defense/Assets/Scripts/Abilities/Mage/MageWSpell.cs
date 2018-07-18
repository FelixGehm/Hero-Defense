﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MageWSpell : NetworkBehaviour
{
    private float damage = 20;
    private float heal = 40;
    public float delay = 1;
    public float timeAlive = 2;
    Collider col;

    private Transform sender;

    public void Init(Transform sender, float damage, float healAmount)
    {
        this.sender = sender;
        this.damage = damage;
        this.heal = healAmount;
    }


    private float time;
    void Start()
    {
        col = GetComponent<Collider>();
        time = Time.time;
        StartCoroutine(ActivateColliderAfterDelay());
    }

    void Update()
    {
        /*
        if (isServer && Time.time - time >= timeAlive)
        {
            Destroy(gameObject);
        }
        */

        if (Time.time - time >= timeAlive)
        {
            Destroy(gameObject);
        }

    }

    private void OnTriggerEnter(Collider collision)
    {
        //Debug.Log("Collison with: " + collision.gameObject.name);

        if (isServer)
        {
            if (collision.transform.CompareTag("Enemy"))
            {
                //collision.gameObject.GetComponent<CharacterStats>().TakePhysicalDamage(damage);
                //collision.gameObject.GetComponent<CharacterStats>().TakeMagicDamage(damage);
                collision.gameObject.GetComponent<EnemyStats>().TakeMagicDamage(damage, sender);
                //Destroy(gameObject);
            }

            if (collision.transform.CompareTag("Player"))
            {
                collision.gameObject.GetComponent<CharacterStats>().TakeHeal(heal);
                //Destroy(gameObject);
            }
        }
    }

    public IEnumerator ActivateColliderAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        col.enabled = true;
    }

}
