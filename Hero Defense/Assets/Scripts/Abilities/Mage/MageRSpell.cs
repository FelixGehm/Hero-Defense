using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MageRSpell : NetworkBehaviour
{
    public float damage = 10;
    public float heal;
    public float tickRate = 0.5f;

    private List<GameObject> enemiesInsideCollider;
    private List<GameObject> playersInsideCollider;

    private float timeStemp;

    void Start()
    {
        if (!isServer)
        {
            this.enabled = false;
            return;
        }

        enemiesInsideCollider = new List<GameObject>();
        playersInsideCollider = new List<GameObject>();

        timeStemp = Time.time;
    }


    void Update()
    {
        if (!isServer)
        {
            this.enabled = false;
            return;
        }

        if (Time.time - timeStemp >= tickRate)
        {
            //Debug.Log("tick");
            HandleUnitsInsideCollider();
            timeStemp = Time.time;
        }


    }

    private void HandleUnitsInsideCollider()
    {
        foreach (GameObject enemy in enemiesInsideCollider)
        {
            if (enemy != null)
                enemy.GetComponent<CharacterStats>().TakeMagicDamage(damage);
        }

        foreach (GameObject player in playersInsideCollider)
        {
            if (player != null)
                player.GetComponent<CharacterStats>().TakeHeal(heal);
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!isServer)
        {
            this.enabled = false;
            return;
        }


        if (collision.gameObject.CompareTag("Player"))
        {
            playersInsideCollider.Add(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            enemiesInsideCollider.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (!isServer)
        {
            this.enabled = false;
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            playersInsideCollider.Remove(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            enemiesInsideCollider.Remove(collision.gameObject);
        }
    }
}
