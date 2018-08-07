using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CanonProjectile : NetworkBehaviour
{

    private float damage;
    private float lifetime;
    private Rigidbody rb;

    [SyncVar]
    private float force;
    [SyncVar]
    private Vector3 direction;


    private float spawnTime;
    public void Init(float damage, float force, Vector3 direction, float lifetime)
    {
        this.damage = damage;
        this.force = force;
        this.direction = direction;
        this.lifetime = lifetime;


        //RpcSyncForce(force, direction);
        spawnTime = Time.time;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(direction * force);
    }


    private void Update()
    {
        if (!isServer) return;

        if (Time.time - spawnTime >= lifetime)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (isServer && collision.transform.CompareTag("Player"))
        {
            //Debug.Log("HITTTTTTTT");
            collision.gameObject.GetComponent<PlayerStats>().TakePhysicalDamage(damage);
            NetworkServer.Destroy(gameObject);
        }
    }
}
