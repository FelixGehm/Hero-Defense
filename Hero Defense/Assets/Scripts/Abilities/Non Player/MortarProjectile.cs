using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MortarProjectile : NetworkBehaviour
{

    private Vector3 startPos = new Vector3(0, 0, 0);
    private Vector3 endPos = new Vector3(4, 0, 0);
    private float height = 2;

    public float yTreshold = 0.2f;      // Höhe über dem Boden wenn Granate explodieren soll
    public float speedFactor = 1.5f;

    private float explosionRange = 1;

    private float explosionDamage = 0;

    private float time = 0;

    [Header("FX")]
    public GameObject explosionEffect;

    public void Init(Vector3 start, Vector3 end, float height, float range, float damage)
    {
        startPos = start;
        endPos = end;
        this.height = height;

        explosionRange = range;

        SphereCollider col = GetComponent<SphereCollider>();
        col.radius = explosionRange;

        explosionDamage = damage;

        GunslingerEGrenadeDummy dummy = GetComponent<GunslingerEGrenadeDummy>();
        dummy.Init(start, end, height, speedFactor);
    }
    void FixedUpdate()
    {
        if (!isServer)
        {
            this.enabled = false;
            return;
        }

        transform.position = Parabola(startPos, endPos, height, time * speedFactor);
        time += Time.deltaTime;

        if (transform.position.y <= yTreshold)
        {


            Collider[] allOverlappingColliders = Physics.OverlapSphere(transform.position, explosionRange);

            foreach (Collider c in allOverlappingColliders)
            {
                if (c.CompareTag("Player"))
                {
                    //Debug.Log("boom: " + explosionDamage);
                    c.gameObject.GetComponent<PlayerStats>().TakePhysicalDamage(explosionDamage);
                }
            }
            if (explosionEffect != null)
                CmdSpawnExplosionEffectOnServer(transform.position);
            NetworkServer.Destroy(this.gameObject);
        }
    }


    // found at: https://gist.github.com/ditzel/68be36987d8e7c83d48f497294c66e08
    public static Vector3 Parabola(Vector3 start, Vector3 end, float height, float t)
    {
        Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

        var mid = Vector3.Lerp(start, end, t);

        return new Vector3(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t), mid.z);
    }

    [Command]
    void CmdSpawnExplosionEffectOnServer(Vector3 spawnPosition)
    {
        GameObject g = Instantiate(explosionEffect, spawnPosition, explosionEffect.transform.rotation);
        NetworkServer.Spawn(g);
    }
}
