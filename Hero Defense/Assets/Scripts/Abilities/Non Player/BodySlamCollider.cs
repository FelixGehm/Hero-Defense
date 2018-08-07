using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BodySlamCollider : NetworkBehaviour
{

    public float timeAlive = 0.2f;
    private float damage = 0;
    private float time;
    private bool active = true;

    public void Init(float dmg)
    {
        damage = dmg;
        active = true;
        Destroy(gameObject, 1);
    }
    private void Start()
    {
        time = Time.time;
    }

    private void Update()
    {
        if (Time.time - time >= timeAlive)
        {
            active = false;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        //Debug.Log("Collison with: " + collision.gameObject.name);
        if (!active) return;

        if (isServer)
        {
            if (collision.transform.CompareTag("Player"))
            {
                collision.gameObject.GetComponent<CharacterStats>().TakePhysicalDamage(damage);
            }
        }
    }
}
