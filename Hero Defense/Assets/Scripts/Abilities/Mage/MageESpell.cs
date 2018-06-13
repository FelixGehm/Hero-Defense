using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MageESpell : NetworkBehaviour
{
    public float speed = 2;
    private GameObject target;
    public void Init(GameObject firstTarget)
    {
        //transform.position = firstTarget.transform.position;
        target = firstTarget;
    }

    // Use this for initialization
    void Start()
    {

    }

    private bool reachedTarget = false;
    [Server]
    void Update()
    {
        if (isServer)
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }


            Vector3 direction = (target.transform.position - transform.position).normalized;
            transform.Translate(direction * speed * Time.deltaTime, Space.World);


            //Debug.Log((transform.position - target.transform.position).magnitude);


            if (!reachedTarget && (transform.position - target.transform.position).magnitude <= 0.05f) //transform nearly reached target
            {
                //Do Damage or Heal
                //init hit particle effect
                reachedTarget = true;
                target = SearchForNextTarget();
                Debug.Log("hit");
            }
            else
            {
                reachedTarget = false;
            }
        }
    }
    private GameObject SearchForNextTarget()
    {
        float dist = 0;
        float lastDist = float.MaxValue;
        GameObject _target = null;
        foreach (GameObject enemy in PlayerManager.instance.enemies)
        {
            if (enemy != target)
            {
                dist = Vector3.Distance(enemy.transform.position, transform.position);

                if (dist < lastDist)
                {
                    _target = enemy;
                }
                lastDist = dist;
            }
        }
        return _target;
    }
}
