using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GunslingerQProjectile : NetworkBehaviour
{

    public float speed = 10;
    public float damage = 0;

    // Have to be initaliszed after spawn
    private Vector3 direction;
    private float maxDistance = 0;

    private bool hasStartPoint = false;
    private Vector3 startPoint;

    private int hitCounter = 0;
    private float[] damageStages = { 1.0f, 0.75f, 0.5f };

    public Transform damageCauser;


    public void Init(Vector3 direction, float maxDistance, float speed, float damage, Transform damageSender)
    {
        this.direction = direction;
        this.maxDistance = maxDistance;
        this.speed = speed;
        this.damage = damage;
        this.damageCauser = damageSender;

        GunslingerQProjectileDummy dummy = GetComponent<GunslingerQProjectileDummy>();
        dummy.Init(direction, speed);
    }

    void FixedUpdate()
    {
        if (!isServer)
        {
            this.enabled = false;
            return;
        }


        if (isServer)
        {
            if (!hasStartPoint)
            {
                startPoint = transform.position;
                hasStartPoint = true;
            }

            float distanceThisFrame = speed * Time.deltaTime;
            transform.Translate(direction.normalized * distanceThisFrame, Space.World);

            if (Vector3.Distance(startPoint, transform.position) >= maxDistance)
            {
                //Debug.Log("Distance Reached");
                Destroy(this.gameObject);
            }
        }
    } 

    private void OnTriggerEnter(Collider collision)
    {
        //Debug.Log("Collison with: " + collision.gameObject.name);

        if (isServer && collision.transform.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<EnemyStats>().TakePhysicalDamage(damage* damageStages[hitCounter], damageCauser);

            if(hitCounter < damageStages.Length-1)
            {
                hitCounter++;
            }
        }
    }
}
