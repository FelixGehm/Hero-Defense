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

    

    //private int[] targets



    public void SetMaxDistance(float dist)
    {
        maxDistance = dist;
    }

    public void SetDirection(Vector3 _direction)
    {
        direction = _direction;
    }

    [Server]
    void Update()
    {
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
            collision.gameObject.GetComponent<CharacterStats>().TakePhysicalDamage(damage);
        }
    }
}
